// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReactiveUI.Binding.Documentation.AnalyzersDispatchReach;

/// <summary>
/// Gives an analyzer the root namespace the way a build does. MSBuild passes the project's root namespace to
/// analyzers as the <c>build_property.RootNamespace</c> global option.
/// </summary>
/// <param name="rootNamespace">The root namespace of the project.</param>
public sealed class RootNamespaceOptionsProvider(string rootNamespace) : AnalyzerConfigOptionsProvider
{
    /// <summary>The global option MSBuild uses to pass the root namespace.</summary>
    private const string RootNamespaceKey = "build_property.RootNamespace";

    /// <summary>The options of a file, which carry nothing.</summary>
    private static readonly OptionMap NoOptions = new(string.Empty, string.Empty);

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GlobalOptions { get; } = new OptionMap(RootNamespaceKey, rootNamespace);

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => NoOptions;

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => NoOptions;

    /// <summary>A set of options that holds one option.</summary>
    /// <param name="optionName">The name of the option; empty for none.</param>
    /// <param name="optionValue">The value of the option.</param>
    private sealed class OptionMap(string optionName, string optionValue) : AnalyzerConfigOptions
    {
        /// <inheritdoc/>
        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            var found = optionName.Length > 0 && string.Equals(optionName, key, StringComparison.Ordinal);
            value = found ? optionValue : null;
            return found;
        }
    }
}
