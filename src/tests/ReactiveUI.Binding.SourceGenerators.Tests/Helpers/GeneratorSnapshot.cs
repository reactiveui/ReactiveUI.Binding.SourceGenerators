// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>Compares every file a generator run produced against the snapshots stored beside the calling test.</summary>
/// <remarks>
/// A snapshot is named <c>{type}.{method}#{hint name}.verified.cs</c> and starts with a <c>//HintName:</c> line. A file
/// that differs, or has no snapshot yet, is written next to it as <c>.received.cs</c> and fails the test, as does a
/// snapshot the run no longer produces. With the <c>ACCEPT_SNAPSHOTS</c> environment variable set, the run's output
/// replaces the snapshots instead.
/// </remarks>
internal static class GeneratorSnapshot
{
    /// <summary>The environment variable that makes a run write its output over the snapshots.</summary>
    private const string AcceptVariable = "ACCEPT_SNAPSHOTS";

    /// <summary>The suffix of a stored snapshot.</summary>
    private const string VerifiedSuffix = ".verified.cs";

    /// <summary>The suffix of the output written beside a snapshot it does not match.</summary>
    private const string ReceivedSuffix = ".received.cs";

    /// <summary>Asserts that the generated files match the stored snapshots.</summary>
    /// <param name="driver">The driver after the generator has run.</param>
    /// <param name="typeName">The snapshot name's type segment.</param>
    /// <param name="methodName">The snapshot name's method segment.</param>
    /// <param name="sourceFilePath">The calling test's source file; its directory holds the snapshots.</param>
    /// <returns>A task that completes once every file has been compared.</returns>
    internal static async Task VerifyAsync(
        GeneratorDriver driver,
        string typeName,
        string methodName,
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentNullException.ThrowIfNull(driver);

        var directory = Path.GetDirectoryName(sourceFilePath) ?? string.Empty;
        var prefix = $"{typeName}.{methodName}#";
        var accept = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(AcceptVariable));
        var produced = new HashSet<string>(StringComparer.Ordinal);
        var failures = new List<string>();

        foreach (var result in driver.GetRunResult().Results)
        {
            foreach (var source in result.GeneratedSources)
            {
                var name = prefix + Path.GetFileNameWithoutExtension(source.HintName);
                _ = produced.Add(name);

                var output = $"//HintName: {source.HintName}\n{Normalize(source.SourceText.ToString())}";
                if (!await StoreAsync(Path.Combine(directory, name), output, accept))
                {
                    failures.Add($"{name}{VerifiedSuffix} does not match the generated output; see {name}{ReceivedSuffix}");
                }
            }
        }

        foreach (var snapshot in Directory.EnumerateFiles(directory, $"{prefix}*{VerifiedSuffix}"))
        {
            var fileName = Path.GetFileName(snapshot);
            if (produced.Contains(fileName[..^VerifiedSuffix.Length]))
            {
                continue;
            }

            if (accept)
            {
                File.Delete(snapshot);
            }
            else
            {
                failures.Add($"{fileName} is no longer generated");
            }
        }

        await Assert.That(failures).IsEmpty();
    }

    /// <summary>Settles one generated file against its snapshot, writing the received or accepted output as needed.</summary>
    /// <param name="basePath">The snapshot path without its suffix.</param>
    /// <param name="output">The generated output, in snapshot form.</param>
    /// <param name="accept">Whether the output replaces the snapshot.</param>
    /// <returns><see langword="true"/> when the output equals the snapshot or was accepted as it.</returns>
    private static async Task<bool> StoreAsync(string basePath, string output, bool accept)
    {
        var verifiedPath = basePath + VerifiedSuffix;
        var receivedPath = basePath + ReceivedSuffix;

        if (accept)
        {
            await File.WriteAllTextAsync(verifiedPath, output);
            File.Delete(receivedPath);
            return true;
        }

        if (File.Exists(verifiedPath) && Normalize(await File.ReadAllTextAsync(verifiedPath)) == output)
        {
            File.Delete(receivedPath);
            return true;
        }

        await File.WriteAllTextAsync(receivedPath, output);
        return false;
    }

    /// <summary>Normalises line endings so a snapshot compares the same on every checkout.</summary>
    /// <param name="text">The text to normalise.</param>
    /// <returns>The text with LF line endings.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Normalize(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal);
}
