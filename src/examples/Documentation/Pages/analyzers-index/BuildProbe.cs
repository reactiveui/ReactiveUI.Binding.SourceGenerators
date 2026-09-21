// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Globalization;

namespace ReactiveUI.Binding.Documentation.AnalyzersIndex;

/// <summary>
/// Evaluates the package's MSBuild props and targets in a real MSBuild process, as a build of a project that
/// references the package does, and reads the value of a property afterwards.
/// </summary>
public static class BuildProbe
{
    /// <summary>The path of the targets file below the source folder of the repository.</summary>
    private const string TargetsPath = "ReactiveUI.Binding.SourceGenerators/build/ReactiveUI.Binding.SourceGenerators.targets";

    /// <summary>The path of the props file below the source folder of the repository.</summary>
    private const string PropsPath = "ReactiveUI.Binding.SourceGenerators/build/ReactiveUI.Binding.SourceGenerators.props";

    /// <summary>The target of the package that settles the compiler slot and the interceptor opt-in.</summary>
    private const string SelectAnalyzerSlotTarget = "ReactiveUIBindingSelectAnalyzerSlot";

    /// <summary>Runs the package's targets and reads a property.</summary>
    /// <param name="property">The name of the property to read once the targets have run.</param>
    /// <param name="globalProperties">The properties the build sets, each written as <c>Name=Value</c>.</param>
    /// <returns>The value of the property; empty when the build leaves it unset.</returns>
    /// <exception cref="InvalidOperationException">MSBuild failed.</exception>
    public static async Task<string> ReadPropertyAsync(string property, params string[] globalProperties)
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

            var (exitCode, output) = await RunMsBuildAsync(projectPath, property, globalProperties).ConfigureAwait(false);

            return exitCode == 0
                ? output.Trim()
                : throw new InvalidOperationException($"MSBuild failed: {output}");
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
    /// <param name="property">The property to print.</param>
    /// <param name="globalProperties">The properties the build sets, each written as <c>Name=Value</c>.</param>
    /// <returns>The exit code and everything MSBuild printed.</returns>
    private static async Task<(int ExitCode, string Output)> RunMsBuildAsync(string projectPath, string property, string[] globalProperties)
    {
        var dotnet = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";
        ProcessStartInfo start = new(dotnet) { RedirectStandardOutput = true, RedirectStandardError = true };

        string[] arguments = ["msbuild", projectPath, "-nologo", "-nodeReuse:false", "-p:Language=C#", $"-t:{SelectAnalyzerSlotTarget}", $"-getProperty:{property}"];
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        foreach (var globalProperty in globalProperties)
        {
            start.ArgumentList.Add($"-p:{globalProperty}");
        }

        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var errors = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync().ConfigureAwait(false);

        return (process.ExitCode, await output.ConfigureAwait(false) + await errors.ConfigureAwait(false));
    }
}
