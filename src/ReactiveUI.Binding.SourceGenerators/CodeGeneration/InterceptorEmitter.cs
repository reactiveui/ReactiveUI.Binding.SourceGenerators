// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Writes the one thing every intercepted call site needs, whichever API it belongs to: the attribute naming
/// the call, and the declaration of that attribute.
/// </summary>
/// <remarks>
/// An interceptor redirects a call the compiler has already bound, so nothing about it depends on which API is
/// being generated - only on where the call is and what it should run instead. That is why every generator
/// shares this rather than growing its own copy, and why the tier needs none of the namespace placement the
/// dispatch overloads do: lookup never sees an interceptor.
/// </remarks>
internal static class InterceptorEmitter
{
    /// <summary>The indentation a member of the generated class is written at.</summary>
    internal const string MemberIndent = "        ";

    /// <summary>The attribute a generated method carries to claim a call site.</summary>
    private const string AttributeName = "global::System.Runtime.CompilerServices.InterceptsLocation";

    /// <summary>Room for the declaration, which is a fixed block of text.</summary>
    private const int DeclarationCapacity = 640;

    /// <summary>Writes the parameter list one API's members declare, closing it.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="first">The invocation whose types the parameters are written from.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    internal delegate void ParameterListWriter(StringBuilder builder, InvocationInfo first, in LanguageFeatures features);

    /// <summary>Writes the attribute that binds a generated method to one call site.</summary>
    /// <param name="builder">The builder receiving the attribute line.</param>
    /// <param name="location">The call site the compiler described.</param>
    /// <param name="indent">The indentation the enclosing class is written at.</param>
    internal static void AppendAttribute(StringBuilder builder, in InterceptorLocation location, string indent) =>
        _ = builder.Append(indent)
            .Append('[')
            .Append(AttributeName)
            .Append('(')
            .Append(location.Version)
            .Append(", \"")
            .Append(location.Data)
            .AppendLine("\")]");

    /// <summary>Builds the declaration of the interception attribute.</summary>
    /// <returns>The source of a file declaring the attribute.</returns>
    /// <remarks>
    /// The attribute is not part of any framework, so the compiler expects the consumer's own compilation to
    /// declare it. It is emitted once for the whole compilation rather than per dispatch file, which would
    /// declare the same type repeatedly, and it is written to the rules the oldest supported consumer parses:
    /// no file-scoped namespace, no file-local type.
    /// </remarks>
    internal static string BuildAttributeDeclaration()
    {
        var builder = PooledBuilder.Rent(DeclarationCapacity);

        _ = builder.AppendLine("namespace System.Runtime.CompilerServices")
            .AppendLine("{")
            .AppendLine("    /// <summary>Binds a generated method to the call site it replaces.</summary>")
            .AppendLine("    [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]")
            .AppendLine("    internal sealed class InterceptsLocationAttribute : global::System.Attribute")
            .AppendLine("    {")
            .AppendLine("        /// <summary>Initializes a new instance of the <see cref=\"InterceptsLocationAttribute\"/> class.</summary>")
            .AppendLine("        /// <param name=\"version\">The encoding of <paramref name=\"data\"/>.</param>")
            .AppendLine("        /// <param name=\"data\">The call site being replaced.</param>")
            .AppendLine("        public InterceptsLocationAttribute(int version, string data)")
            .AppendLine("        {")
            .AppendLine("            _ = version;")
            .AppendLine("            _ = data;")
            .AppendLine("        }")
            .AppendLine("    }")
            .AppendLine("}");

        return PooledBuilder.ToStringAndReturn(builder);
    }

    /// <summary>Emits one interceptor per generated body, claiming every call site that reaches it.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="group">The type group whose call sites are being claimed.</param>
    /// <param name="methodPrefix">The method name prefix the generated bodies carry.</param>
    /// <param name="suffixOf">Names the body a call site reaches.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot, handed to the writer.</param>
    /// <param name="appendParameterList">Writes the whole parameter list, closing it.</param>
    /// <remarks>
    /// The grouping and the attribute are the whole of what every API shares here, so they live in one place
    /// and each API supplies only the signature it is being called with. Call sites that reach one body are
    /// claimed by one method carrying an attribute each, which is what the attribute allowing repeats is for.
    /// <para>
    /// The feature snapshot travels as an argument rather than being closed over, so the writer each API hands
    /// in captures nothing and the compiler caches one delegate for the whole compilation.
    /// </para>
    /// </remarks>
    internal static void GenerateInterceptors(
        StringBuilder builder,
        ObservationCodeGenerator.TypeGroup group,
        string methodPrefix,
        Func<InvocationInfo, string> suffixOf,
        in LanguageFeatures features,
        ParameterListWriter appendParameterList)
    {
        foreach (var entry in GroupCallSitesByBody(group, suffixOf))
        {
            var first = entry.Value[0];

            foreach (var callSite in entry.Value)
            {
                AppendAttribute(builder, callSite.Interceptor, MemberIndent);
            }

            _ = builder.Append("        internal static global::System.IObservable<").Append(first.ReturnTypeFullName)
                .Append("> __Intercept_").Append(methodPrefix).Append('_').Append(entry.Key).AppendLine("(");

            appendParameterList(builder, first, in features);

            _ = builder.Append("            => __").Append(methodPrefix).Append('_').Append(entry.Key)
                .Append("(objectToMonitor").Append(first.HasSelector ? ", selector" : string.Empty).AppendLine(");").AppendLine();
        }
    }

    /// <summary>Gathers call sites under the generated method each of them reaches.</summary>
    /// <typeparam name="T">The per-call-site model this API extracts.</typeparam>
    /// <param name="invocations">The call sites of one group.</param>
    /// <param name="locationOf">Reads where a call site is.</param>
    /// <param name="suffixOf">Names the generated method a call site reaches.</param>
    /// <returns>Each generated method, against every call site that resolves to it.</returns>
    /// <remarks>
    /// Every API claims its call sites the same way, so the gathering is written once over whatever model the
    /// API happens to carry. A call site the compiler declined to describe is left out: nothing can claim it,
    /// and it keeps whatever the call already resolved to.
    /// </remarks>
    internal static Dictionary<string, List<T>> GroupCallSites<T>(
        IReadOnlyList<T> invocations,
        Func<T, InterceptorLocation> locationOf,
        Func<T, string> suffixOf)
    {
        var claimed = new Dictionary<string, List<T>>(StringComparer.Ordinal);
        for (var i = 0; i < invocations.Count; i++)
        {
            var invocation = invocations[i];
            if (!locationOf(invocation).IsAvailable)
            {
                continue;
            }

            var suffix = suffixOf(invocation);
            if (!claimed.TryGetValue(suffix, out var callSites))
            {
                callSites = [];
                claimed[suffix] = callSites;
            }

            callSites.Add(invocation);
        }

        return claimed;
    }

    /// <summary>Gathers the call sites of a group under the body each of them reaches.</summary>
    /// <param name="group">The type group whose call sites are being gathered.</param>
    /// <param name="suffixOf">Names the body a call site reaches.</param>
    /// <returns>Each generated body, against every call site that resolves to it.</returns>
    /// <remarks>
    /// A call site the compiler declined to describe is left out: nothing can claim it, and it keeps whatever
    /// the call already resolved to.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Dictionary<string, List<InvocationInfo>> GroupCallSitesByBody(
        ObservationCodeGenerator.TypeGroup group,
        Func<InvocationInfo, string> suffixOf) =>
        GroupCallSites(group.Invocations, static x => x.Interceptor, suffixOf);
}
