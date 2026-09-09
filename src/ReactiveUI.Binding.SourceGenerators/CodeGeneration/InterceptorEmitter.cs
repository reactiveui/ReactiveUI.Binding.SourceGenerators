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
}
