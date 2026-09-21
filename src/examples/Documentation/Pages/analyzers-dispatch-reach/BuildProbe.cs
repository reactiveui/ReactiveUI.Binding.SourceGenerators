// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Globalization;

namespace ReactiveUI.Binding.Documentation.AnalyzersDispatchReach;

/// <summary>
/// Runs the package's MSBuild targets in a real MSBuild process, as a build of a project that references the
/// package does, and reports what MSBuild printed.
/// </summary>
public static class BuildProbe
{
    /// <summary>The path of the targets file below the source folder of the repository.</summary>
    private const string TargetsPath = "ReactiveUI.Binding.SourceGenerators/build/ReactiveUI.Binding.SourceGenerators.targets";

    /// <summary>The path of the props file below the source folder of the repository.</summary>
    private const string PropsPath = "ReactiveUI.Binding.SourceGenerators/build/ReactiveUI.Binding.SourceGenerators.props";

    /// <summary>The target of the package that checks the compiler version.</summary>
    private const string SelectAnalyzerSlotTarget = "ReactiveUIBindingSelectAnalyzerSlot";

    /// <summary>The prefix MSBuild prints before the text of an error.</summary>
    private const string ErrorMarker = "error RXUIBIND100: ";

    /// <summary>The separator MSBuild prints between the text of an error and the project that raised it.</summary>
    private const string ProjectMarker = " [";

    /// <summary>Runs the compiler check of the package as a build with a given compiler.</summary>
    /// <param name="compilerApiVersion">The compiler the build reports, such as <c>roslyn4.8</c>.</param>
    /// <returns>The message of the RXUIBIND100 error the build printed, or an empty string when the build passed.</returns>
    /// <exception cref="InvalidOperationException">MSBuild failed without printing RXUIBIND100.</exception>
    public static async Task<string> RunCompilerCheckAsync(string compilerApiVersion)
    {
        var sourceFolder = FindSourceFolder();
        var projectFolder = Path.Combine(Path.GetTempPath(), $"rxui-binding-probe-{Environment.ProcessId.ToString(CultureInfo.InvariantCulture)}");
        _ = Directory.CreateDirectory(projectFolder);

        try
        {
            var projectPath = Path.Combine(projectFolder, "Probe.proj");
            await File.WriteAllTextAsync(
                projectPath,
                $"""
                <Project>
                  <Import Project="{Path.Combine(sourceFolder, PropsPath)}" />
                  <Import Project="{Path.Combine(sourceFolder, TargetsPath)}" />
                </Project>
                """).ConfigureAwait(false);

            var (exitCode, output) = await RunMsBuildAsync(projectPath, compilerApiVersion).ConfigureAwait(false);
            var errorStart = output.IndexOf(ErrorMarker, StringComparison.Ordinal);
            if (errorStart < 0)
            {
                return exitCode == 0
                    ? string.Empty
                    : throw new InvalidOperationException($"MSBuild failed without RXUIBIND100: {output}");
            }

            var messageStart = errorStart + ErrorMarker.Length;
            var messageEnd = output.IndexOf(ProjectMarker, messageStart, StringComparison.Ordinal);
            return output[messageStart..messageEnd];
        }
        finally
        {
            Directory.Delete(projectFolder, recursive: true);
        }
    }

    /// <summary>Finds the source folder of the repository, which holds the build files of the package.</summary>
    /// <returns>The path of the source folder.</returns>
    /// <exception cref="DirectoryNotFoundException">The program does not run from inside the repository.</exception>
    private static string FindSourceFolder()
    {
        for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder is not null; folder = folder.Parent)
        {
            if (File.Exists(Path.Combine(folder.FullName, TargetsPath)))
            {
                return folder.FullName;
            }
        }

        throw new DirectoryNotFoundException("The package build files were not found above the program.");
    }

    /// <summary>Runs MSBuild on a project that imports the package's build files.</summary>
    /// <param name="projectPath">The path of the project.</param>
    /// <param name="compilerApiVersion">The compiler the build reports.</param>
    /// <returns>The exit code and everything MSBuild printed.</returns>
    private static async Task<(int ExitCode, string Output)> RunMsBuildAsync(string projectPath, string compilerApiVersion)
    {
        var dotnet = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";
        ProcessStartInfo start = new(dotnet) { RedirectStandardOutput = true, RedirectStandardError = true };

        string[] arguments = ["msbuild", projectPath, "-nologo", "-nodeReuse:false", "-p:Language=C#", $"-p:CompilerApiVersion={compilerApiVersion}", $"-t:{SelectAnalyzerSlotTarget}"];
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var errors = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync().ConfigureAwait(false);

        return (process.ExitCode, await output.ConfigureAwait(false) + await errors.ConfigureAwait(false));
    }
}
