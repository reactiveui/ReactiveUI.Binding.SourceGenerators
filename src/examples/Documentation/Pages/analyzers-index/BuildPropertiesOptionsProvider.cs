// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReactiveUI.Binding.Documentation.AnalyzersIndex;

/// <summary>
/// Gives a source generator the MSBuild properties a build exposes to it. A project lists a property under
/// <c>CompilerVisibleProperty</c>, and the compiler passes it as a <c>build_property.Name</c> global option.
/// </summary>
/// <param name="properties">The value of each property, by property name.</param>
public sealed class BuildPropertiesOptionsProvider(IReadOnlyDictionary<string, string> properties) : AnalyzerConfigOptionsProvider
{
    /// <summary>The options of a file, which carry nothing.</summary>
    private static readonly OptionMap NoOptions = new(new Dictionary<string, string>());

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GlobalOptions { get; } = new OptionMap(properties);

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => NoOptions;

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => NoOptions;

    /// <summary>A set of options read from a dictionary of build properties.</summary>
    /// <param name="values">The value of each property, by property name.</param>
    private sealed class OptionMap(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
    {
        /// <summary>The prefix the compiler puts before the name of an exposed MSBuild property.</summary>
        private const string BuildPropertyPrefix = "build_property.";

        /// <inheritdoc/>
        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            if (key.StartsWith(BuildPropertyPrefix, StringComparison.Ordinal) && values.TryGetValue(key[BuildPropertyPrefix.Length..], out var found))
            {
                value = found;
                return true;
            }

            value = null;
            return false;
        }
    }
}
