// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Analyzer.Tests.Helpers;

/// <summary>
/// Utilities for testing analyzers and code fixes.
/// </summary>
public static class TestUtilities
{
    /// <summary>
    /// Normalizes whitespace and line endings in source code for comparison.
    /// </summary>
    /// <param name="source">The source code to normalize.</param>
    /// <returns>Normalized source code suitable for comparison.</returns>
    public static string NormalizeWhitespace(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        return string.Join(
            "\n",
            source.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None)
                .Select(line => line.Trim()));
    }

    /// <summary>
    /// Compares two source code strings with whitespace normalization.
    /// </summary>
    /// <param name="expected">The expected source code.</param>
    /// <param name="actual">The actual source code.</param>
    /// <returns>True if the sources are equivalent after normalization.</returns>
    public static bool AreEquivalent(string expected, string actual)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expected);
        ArgumentException.ThrowIfNullOrWhiteSpace(actual);
        var result = NormalizeWhitespace(expected) == NormalizeWhitespace(actual);

        if (!result)
        {
            Console.WriteLine("=== EXPECTED ===");
            Console.WriteLine(expected);
            Console.WriteLine();
            Console.WriteLine("=== ACTUAL ===");
            Console.WriteLine(actual);
        }

        return result;
    }
}
