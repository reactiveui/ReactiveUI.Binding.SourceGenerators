// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
    /// <summary>The attribute a generated method carries to claim a call site.</summary>
    private const string AttributeName = "global::System.Runtime.CompilerServices.InterceptsLocation";

    /// <summary>Room for the declaration, which is a fixed block of text.</summary>
    private const int DeclarationCapacity = 640;

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
    /// <param name="appendParameters">Writes the parameters after the receiver, closing the list.</param>
    /// <remarks>
    /// The grouping and the attribute are the whole of what every API shares here, so they live in one place
    /// and each API supplies only the signature it is being called with. Call sites that reach one body are
    /// claimed by one method carrying an attribute each, which is what the attribute allowing repeats is for.
    /// </remarks>
    internal static void GenerateInterceptors(
        StringBuilder builder,
        ObservationCodeGenerator.TypeGroup group,
        string methodPrefix,
        Func<InvocationInfo, string> suffixOf,
        Action<StringBuilder, InvocationInfo> appendParameters)
    {
        foreach (var entry in GroupCallSitesByBody(group, suffixOf))
        {
            var first = entry.Value[0];

            foreach (var callSite in entry.Value)
            {
                AppendAttribute(builder, callSite.Interceptor, "        ");
            }

            _ = builder.Append("        internal static global::System.IObservable<").Append(first.ReturnTypeFullName)
                .Append("> __Intercept_").Append(methodPrefix).Append('_').Append(entry.Key).AppendLine("(")
                .Append("            ").Append(first.SourceTypeFullName).AppendLine(" objectToMonitor,");

            appendParameters(builder, first);

            _ = builder.Append("            => __").Append(methodPrefix).Append('_').Append(entry.Key)
                .Append("(objectToMonitor").Append(first.HasSelector ? ", selector" : string.Empty).AppendLine(");");
        }
    }

    /// <summary>Emits the observed-property parameters, and the projection when the overload takes one.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="first">The invocation whose types the parameters are written from.</param>
    /// <param name="propCount">How many observed properties the overload takes.</param>
    /// <param name="selectorType">The projection's type, or <see langword="null"/> when it takes none.</param>
    internal static void AppendPropertyParameters(
        StringBuilder sb,
        InvocationInfo first,
        int propCount,
        string? selectorType)
    {
        var hasSelector = selectorType is not null;
        for (var i = 0; i < propCount; i++)
        {
            var type = first.PropertyPaths[i][first.PropertyPaths[i].Length - 1].PropertyTypeFullName;
            _ = sb.Append("            global::System.Linq.Expressions.Expression<global::System.Func<")
                .Append(first.SourceTypeFullName).Append(", ").Append(type).Append(">> property").Append(i + 1);
            _ = hasSelector || i < propCount - 1 ? sb.AppendLine(",") : sb.AppendLine(")");
        }

        if (!hasSelector)
        {
            return;
        }

        _ = sb.Append("            ").Append(selectorType).AppendLine(" selector)");
    }

    /// <summary>Gathers the call sites of a group under the body each of them reaches.</summary>
    /// <param name="group">The type group whose call sites are being gathered.</param>
    /// <param name="suffixOf">Names the body a call site reaches.</param>
    /// <returns>Each generated body, against every call site that resolves to it.</returns>
    /// <remarks>
    /// A call site the compiler declined to describe is left out: nothing can claim it, and it keeps whatever
    /// the call already resolved to.
    /// </remarks>
    private static Dictionary<string, List<InvocationInfo>> GroupCallSitesByBody(
        ObservationCodeGenerator.TypeGroup group,
        Func<InvocationInfo, string> suffixOf)
    {
        var claimed = new Dictionary<string, List<InvocationInfo>>(StringComparer.Ordinal);
        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            if (!inv.Interceptor.IsAvailable)
            {
                continue;
            }

            var suffix = suffixOf(inv);
            if (!claimed.TryGetValue(suffix, out var callSites))
            {
                callSites = [];
                claimed[suffix] = callSites;
            }

            callSites.Add(inv);
        }

        return claimed;
    }
}
