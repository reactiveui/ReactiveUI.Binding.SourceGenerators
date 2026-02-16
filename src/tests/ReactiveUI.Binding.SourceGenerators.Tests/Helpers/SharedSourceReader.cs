// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using System.Text.RegularExpressions;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// Reads shared scenario source files from the test output directory.
/// These files are copied via MSBuild None items with CopyToOutputDirectory.
/// </summary>
internal static partial class SharedSourceReader
{
    /// <summary>
    /// Reads all source files from a shared scenario directory and merges them
    /// into a single valid C# compilation unit with deduplicated usings
    /// and a single namespace block.
    /// </summary>
    /// <param name="scenarioPath">The path relative to the SharedScenarios root (e.g. "WhenChanged/SinglePropertyINPC").</param>
    /// <returns>A single merged source text containing all types from the scenario.</returns>
    public static string ReadScenario(string scenarioPath)
    {
        var dir = Path.Combine(FindRoot(), scenarioPath);
        var files = Directory.GetFiles(dir, "*.cs");
        Array.Sort(files);

        var copyrightLines = new List<string>();
        var usingDirectives = new LinkedHashSet<string>();
        string? namespaceName = null;
        var typeBlocks = new List<string>();

        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            ParseFile(lines, copyrightLines, usingDirectives, ref namespaceName, typeBlocks);
        }

        var sb = new StringBuilder();

        // Copyright header
        foreach (var line in copyrightLines)
        {
            sb.AppendLine(line);
        }

        sb.AppendLine();

        // Deduplicated using directives (preserve blank line separators between groups)
        var usingList = usingDirectives.ToList();
        string? prevPrefix = null;
        foreach (var u in usingList)
        {
            var currentPrefix = ExtractUsingPrefix(u);
            if (prevPrefix != null && currentPrefix != prevPrefix)
            {
                sb.AppendLine();
            }

            sb.AppendLine(u);
            prevPrefix = currentPrefix;
        }

        sb.AppendLine();

        // Single namespace block with all type blocks
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");

        for (var i = 0; i < typeBlocks.Count; i++)
        {
            if (i > 0)
            {
                sb.AppendLine();
            }

            sb.AppendLine(typeBlocks[i]);
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    internal static string FindRoot()
        => Path.Combine(Path.GetDirectoryName(typeof(SharedSourceReader).Assembly.Location)!, "SharedScenarios");

    private static void ParseFile(
        string[] lines,
        List<string> copyrightLines,
        LinkedHashSet<string> usingDirectives,
        ref string? namespaceName,
        List<string> typeBlocks)
    {
        var i = 0;

        // 1. Copyright header (only capture from first file)
        while (i < lines.Length && (lines[i].StartsWith("//", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(lines[i])))
        {
            if (copyrightLines.Count == 0 && lines[i].StartsWith("//", StringComparison.Ordinal))
            {
                // First file - start collecting
                copyrightLines.Add(lines[i]);
            }
            else if (copyrightLines.Count > 0 && copyrightLines.Count < 3 && lines[i].StartsWith("//", StringComparison.Ordinal))
            {
                copyrightLines.Add(lines[i]);
            }

            i++;
        }

        // 2. Using directives
        while (i < lines.Length)
        {
            var trimmed = lines[i].Trim();
            if (trimmed.StartsWith("using ", StringComparison.Ordinal))
            {
                usingDirectives.Add(trimmed);
                i++;
            }
            else if (string.IsNullOrWhiteSpace(trimmed))
            {
                i++;
            }
            else
            {
                break;
            }
        }

        // 3. Namespace declaration
        if (i < lines.Length)
        {
            var nsMatch = NamespaceRegex().Match(lines[i]);
            if (nsMatch.Success)
            {
                namespaceName ??= nsMatch.Groups[1].Value;
                i++;
            }
        }

        // 4. Skip opening brace
        while (i < lines.Length && lines[i].Trim() != "{")
        {
            i++;
        }

        i++; // skip {

        // 5. Collect the body (everything until the final closing brace)
        var bodyStart = i;
        var braceDepth = 1;
        var bodyEnd = bodyStart;
        while (i < lines.Length && braceDepth > 0)
        {
            foreach (var ch in lines[i])
            {
                if (ch == '{')
                {
                    braceDepth++;
                }
                else if (ch == '}')
                {
                    braceDepth--;
                    if (braceDepth == 0)
                    {
                        break;
                    }
                }
            }

            if (braceDepth > 0)
            {
                bodyEnd = i;
                i++;
            }
        }

        // Trim leading/trailing blank lines from body
        while (bodyStart <= bodyEnd && string.IsNullOrWhiteSpace(lines[bodyStart]))
        {
            bodyStart++;
        }

        while (bodyEnd >= bodyStart && string.IsNullOrWhiteSpace(lines[bodyEnd]))
        {
            bodyEnd--;
        }

        if (bodyStart <= bodyEnd)
        {
            var body = string.Join(Environment.NewLine, lines[bodyStart..(bodyEnd + 1)]);
            typeBlocks.Add(body);
        }
    }

    private static string ExtractUsingPrefix(string usingDirective)
    {
        // Group usings by top-level namespace (e.g., "System" vs "ReactiveUI")
        var match = UsingPrefixRegex().Match(usingDirective);
        return match.Success ? match.Groups[1].Value : usingDirective;
    }

    [GeneratedRegex(@"^\s*namespace\s+([\w.]+)")]
    private static partial Regex NamespaceRegex();

    [GeneratedRegex(@"^using\s+(?:static\s+)?(\w+)")]
    private static partial Regex UsingPrefixRegex();

    /// <summary>
    /// A set that preserves insertion order while preventing duplicates.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    private sealed class LinkedHashSet<T> : IEnumerable<T>
        where T : notnull
    {
        private readonly HashSet<T> _set = [];
        private readonly List<T> _list = [];

        public bool Add(T item)
        {
            if (_set.Add(item))
            {
                _list.Add(item);
                return true;
            }

            return false;
        }

        public List<T> ToList() => new(_list);

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
