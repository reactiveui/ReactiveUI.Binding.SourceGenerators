// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Retargets generated code from the lean runtime library onto the System.Reactive flavour.</summary>
/// <remarks>
/// <para>
/// The two runtime packages are the same source compiled twice, and they share no type names: everything the
/// lean package puts in <c>ReactiveUI.Binding.*</c> the other puts in <c>ReactiveUI.Binding.Reactive.*</c>, and
/// the scheduler abstraction differs outright. Generated code that names the lean types does not compile at all
/// against the other package.
/// </para>
/// <para>
/// The emitters write the lean names, and this pass shifts them when the consumer references the other package.
/// It runs on the finished text rather than being threaded through the emitters because most of the generated
/// bodies are non-interpolated raw string literals, whose braces are the braces of the generated code - making
/// them interpolated to inject a namespace would mean escaping every one.
/// </para>
/// <para>
/// The shift is anchored on the names the runtime namespace actually declares, read from the referenced
/// assembly rather than listed here, so it cannot go stale as the runtime grows and it cannot touch a consumer
/// type that merely happens to sit under <c>ReactiveUI.Binding</c> - which is exactly what a consumer whose own
/// root namespace starts that way would otherwise hit.
/// </para>
/// </remarks>
internal static class RuntimeFlavourRewriter
{
    /// <summary>The prefix every generated reference to the lean runtime library starts with.</summary>
    private const string LeanPrefix = "global::ReactiveUI.Binding.";

    /// <summary>The segment the System.Reactive flavour inserts after the shared root.</summary>
    private const string ReactiveSegment = "Reactive.";

    /// <summary>How much headroom to leave for the segments the shift inserts, as a fraction of the source.</summary>
    private const int GrowthDivisor = 8;

    /// <summary>Retargets generated source onto the runtime flavour the consumer actually references.</summary>
    /// <param name="source">The generated source, written against the lean runtime library.</param>
    /// <param name="features">The consumer compilation's snapshot, naming the flavour and its members.</param>
    /// <returns>The source, unchanged for a lean consumer.</returns>
    internal static string Retarget(string source, in LanguageFeatures features) =>
        features.UsesReactiveRuntime
            ? ShiftRuntimeNamespace(
                source.Replace(Constants.LeanSchedulerTypeName, Constants.ReactiveSchedulerTypeName),
                features.RuntimeNamespaceMembers)
            : source;

    /// <summary>Inserts the flavour segment into every reference the runtime namespace declares.</summary>
    /// <param name="source">The generated source.</param>
    /// <param name="runtimeNamespaceMembers">The names the runtime namespace declares, types and namespaces alike.</param>
    /// <returns>The shifted source.</returns>
    private static string ShiftRuntimeNamespace(string source, EquatableArray<string> runtimeNamespaceMembers)
    {
        var index = source.IndexOf(LeanPrefix, StringComparison.Ordinal);
        if (index < 0)
        {
            return source;
        }

        var builder = PooledBuilder.Rent(source.Length + (source.Length / GrowthDivisor));
        var copiedTo = 0;

        while (index >= 0)
        {
            var segmentStart = index + LeanPrefix.Length;
            var segmentEnd = segmentStart;
            while (segmentEnd < source.Length && IsIdentifierCharacter(source[segmentEnd]))
            {
                segmentEnd++;
            }

            if (Declares(runtimeNamespaceMembers, source, segmentStart, segmentEnd - segmentStart))
            {
                _ = builder.Append(source, copiedTo, segmentStart - copiedTo).Append(ReactiveSegment);
                copiedTo = segmentStart;
            }

            index = source.IndexOf(LeanPrefix, segmentEnd, StringComparison.Ordinal);
        }

        return PooledBuilder.ToStringAndReturn(builder.Append(source, copiedTo, source.Length - copiedTo));
    }

    /// <summary>Determines whether the runtime namespace declares the name at the given span of the source.</summary>
    /// <param name="runtimeNamespaceMembers">The names the runtime namespace declares.</param>
    /// <param name="source">The generated source.</param>
    /// <param name="start">The index the name starts at.</param>
    /// <param name="length">The length of the name.</param>
    /// <returns><see langword="true"/> when the name is one the runtime declares.</returns>
    private static bool Declares(EquatableArray<string> runtimeNamespaceMembers, string source, int start, int length)
    {
        for (var i = 0; i < runtimeNamespaceMembers.Length; i++)
        {
            var member = runtimeNamespaceMembers[i];
            if (member.Length == length && string.CompareOrdinal(member, 0, source, start, length) == 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a character can appear inside a C# identifier.</summary>
    /// <param name="character">The character to test.</param>
    /// <returns><see langword="true"/> when the character continues an identifier.</returns>
    private static bool IsIdentifierCharacter(char character) =>
        char.IsLetterOrDigit(character) || character == '_';
}
