// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// The fixed fragments of generated C# that more than one emitter writes, named once so the text and the
/// indentation that goes with it are decided in a single place.
/// </summary>
/// <remarks>
/// Generated output is written as a chain of <see cref="System.Text.StringBuilder.Append(string)"/> calls
/// rather than interpolated into one, so the fixed part of every line is a literal in its own right. These
/// are the ones that recur, and each carries its own indentation because indentation is part of what the
/// fragment is - a member's brace and a statement's brace are different fragments, not one fragment used
/// twice. Every member is a <c>const</c>, so referencing it costs nothing at run time.
/// </remarks>
internal static class GeneratedSyntax
{
    /// <summary>Opens the block of a generated member.</summary>
    internal const string MemberBodyOpen = "        {";

    /// <summary>Closes the block of a generated member.</summary>
    internal const string MemberBodyClose = "        }";

    /// <summary>Opens a block nested inside a generated member's body.</summary>
    internal const string StatementBlockOpen = "            {";

    /// <summary>Closes a block nested inside a generated member's body.</summary>
    internal const string StatementBlockClose = "            }";

    /// <summary>Opens an attribute on a parameter of a generated dispatch overload.</summary>
    internal const string ParameterAttributeOpen = "            [";

    /// <summary>Declares a local in a generated member's body.</summary>
    internal const string BodyLocalDeclaration = "            var ";

    /// <summary>Declares a local at the indentation an inline emitter writes into.</summary>
    internal const string InlineLocalDeclaration = "        var ";

    /// <summary>Opens a property-selector parameter of a generated dispatch overload.</summary>
    internal const string SelectorParameterOpen =
        "            global::System.Linq.Expressions.Expression<global::System.Func<";

    /// <summary>Opens a delegate parameter of a generated dispatch overload.</summary>
    internal const string FuncParameterOpen = "            global::System.Func<";

    /// <summary>Closes the cast of an observation callback's argument, ready for the property name.</summary>
    internal const string ObserverCastClose = ")__o).";

    /// <summary>Opens the quoted property-name argument of an observation.</summary>
    internal const string QuotedArgumentOpen = "                \"";
}
