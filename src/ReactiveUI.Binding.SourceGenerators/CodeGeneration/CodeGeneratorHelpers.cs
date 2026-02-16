// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Shared utility methods for code generation: property path building, string escaping,
/// and class info lookup.
/// </summary>
internal static class CodeGeneratorHelpers
{
    /// <summary>
    /// Builds a dotted property access chain from a root variable and property path segments.
    /// </summary>
    /// <param name="root">The root variable name (e.g., "obj", "source").</param>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted access chain like "obj.Address.City".</returns>
    internal static string BuildPropertyAccessChain(string root, EquatableArray<PropertyPathSegment> path)
    {
        if (path.Length == 0)
        {
            return root;
        }

        var sb = new StringBuilder(root.Length + (path.Length * 16));
        sb.Append(root);
        for (int i = 0; i < path.Length; i++)
        {
            sb.Append('.').Append(path[i].PropertyName);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Builds a property access expression for use in a lambda body.
    /// </summary>
    /// <param name="param">The lambda parameter name.</param>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted access chain like "x.Address.City".</returns>
    internal static string BuildPropertyAccessLambda(string param, EquatableArray<PropertyPathSegment> path)
    {
        return BuildPropertyAccessChain(param, path);
    }

    /// <summary>
    /// Builds a property setter chain for assignment (e.g., target.Header.Title).
    /// </summary>
    /// <param name="root">The root variable name.</param>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted access chain suitable for the left side of an assignment.</returns>
    internal static string BuildPropertySetterChain(string root, EquatableArray<PropertyPathSegment> path)
    {
        return BuildPropertyAccessChain(root, path);
    }

    /// <summary>
    /// Builds a human-readable dotted property path string for comments.
    /// </summary>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted string like "Address.City".</returns>
    internal static string BuildPropertyPathString(EquatableArray<PropertyPathSegment> path)
    {
        if (path.Length == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder(path.Length * 16);
        for (int i = 0; i < path.Length; i++)
        {
            if (i > 0)
            {
                sb.Append('.');
            }

            sb.Append(path[i].PropertyName);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Computes a path suffix for CallerFilePath dispatch matching.
    /// Takes the last 2 path segments (e.g., "ViewModels/MyViewModel.cs").
    /// </summary>
    /// <param name="filePath">The full caller file path.</param>
    /// <returns>The last 2 path segments normalized with forward slashes.</returns>
    internal static string ComputePathSuffix(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return string.Empty;
        }

        // Normalize separators (skip allocation on Unix paths that have no backslashes)
        if (filePath.IndexOf('\\') >= 0)
        {
            filePath = filePath.Replace('\\', '/');
        }

        // Take the last 2 segments (e.g., "src/MyFile.cs" or "ViewModels/MyViewModel.cs")
        int lastSlash = filePath.LastIndexOf('/');
        if (lastSlash < 0)
        {
            return filePath;
        }

        int secondLastSlash = filePath.LastIndexOf('/', lastSlash - 1);
        if (secondLastSlash < 0)
        {
            return filePath;
        }

        return filePath.Substring(secondLastSlash + 1);
    }

    /// <summary>
    /// Escapes a string for embedding in a C# string literal.
    /// </summary>
    /// <param name="value">The string to escape.</param>
    /// <returns>The escaped string.</returns>
    internal static string EscapeString(string value)
    {
        // Fast path: most lambda expressions contain no backslashes or quotes
        bool needsEscape = false;
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (c == '\\' || c == '"')
            {
                needsEscape = true;
                break;
            }
        }

        if (!needsEscape)
        {
            return value;
        }

        var sb = new StringBuilder(value.Length + 4);
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (c == '\\')
            {
                sb.Append("\\\\");
            }
            else if (c == '"')
            {
                sb.Append("\\\"");
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Normalizes a CallerArgumentExpression lambda text by stripping the <c>static</c> modifier.
    /// C# allows <c>static x =&gt; x.Name</c> to prevent captures, but CallerArgumentExpression
    /// captures the literal text including "static ". This method strips that prefix so dispatch
    /// table lookups match regardless of whether the user wrote <c>static</c>.
    /// </summary>
    /// <param name="expressionText">The raw expression text (e.g., "static x =&gt; x.Name").</param>
    /// <returns>The normalized text (e.g., "x =&gt; x.Name").</returns>
    internal static string NormalizeLambdaText(string expressionText)
    {
        const string staticPrefix = "static ";
        if (expressionText.Length > staticPrefix.Length
            && expressionText[0] == 's'
            && expressionText.StartsWith(staticPrefix, StringComparison.Ordinal))
        {
            return expressionText.Substring(staticPrefix.Length);
        }

        return expressionText;
    }

    /// <summary>
    /// Appends the standard auto-generated file header and opens the extension partial class.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    internal static void AppendExtensionClassHeader(StringBuilder sb)
    {
        sb.Append("""
            // <auto-generated/>
            #pragma warning disable
            #nullable enable

            using System;

            namespace ReactiveUI.Binding
            {
                internal static partial class __ReactiveUIGeneratedBindings
                {
            """);
    }

    /// <summary>
    /// Appends the closing braces for the extension partial class and namespace.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    internal static void AppendExtensionClassFooter(StringBuilder sb)
    {
        sb.Append("""
                }
            }
            """);
    }

    /// <summary>
    /// Computes a stable method suffix based on source type, caller file path, caller line number,
    /// and an additional discriminator (typically expression text).
    /// Uses a deterministic hash (FNV-1a) so that method names remain stable across compilations
    /// and do not shift when new invocations are added above or below.
    /// </summary>
    /// <param name="sourceType">The fully qualified source type name.</param>
    /// <param name="callerFilePath">The caller file path.</param>
    /// <param name="callerLineNumber">The caller line number.</param>
    /// <param name="discriminator">Additional discriminator for uniqueness (e.g., expression text).</param>
    /// <returns>A 16-character uppercase hex string suitable for use as a method name suffix.</returns>
    internal static string ComputeStableMethodSuffix(string sourceType, string callerFilePath, int callerLineNumber, string discriminator = "")
    {
        unchecked
        {
            long hash = 17L;
            hash = (hash * 31L) + StableStringHash(sourceType);
            hash = (hash * 31L) + StableStringHash(callerFilePath);
            hash = (hash * 31L) + callerLineNumber;
            hash = (hash * 31L) + StableStringHash(discriminator);
            return (hash & 0x7FFFFFFFFFFFFFFFL).ToString("X16");
        }
    }

    /// <summary>
    /// Finds a <see cref="ClassBindingInfo"/> by fully qualified type name.
    /// </summary>
    /// <param name="allClasses">All detected class binding infos.</param>
    /// <param name="fullyQualifiedName">The fully qualified name to match.</param>
    /// <returns>The matching class info, or null if not found.</returns>
    internal static ClassBindingInfo? FindClassInfo(ImmutableArray<ClassBindingInfo> allClasses, string fullyQualifiedName)
    {
        for (int i = 0; i < allClasses.Length; i++)
        {
            if (allClasses[i].FullyQualifiedName == fullyQualifiedName)
            {
                return allClasses[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Computes a deterministic hash for a string using FNV-1a.
    /// Unlike <see cref="string.GetHashCode()"/>, this is stable across processes and .NET versions.
    /// </summary>
    /// <param name="s">The string to hash.</param>
    /// <returns>A deterministic 32-bit hash code.</returns>
    internal static int StableStringHash(string s)
    {
        if (s == null)
        {
            return 0;
        }

        unchecked
        {
            int hash = (int)2166136261;
            for (int i = 0; i < s.Length; i++)
            {
                hash = (hash ^ s[i]) * 16777619;
            }

            return hash;
        }
    }
}
