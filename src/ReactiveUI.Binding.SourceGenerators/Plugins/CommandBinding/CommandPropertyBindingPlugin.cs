// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;

/// <summary>
/// Command binding plugin for controls that have a <c>Command</c> property (ICommand)
/// and optionally a <c>CommandParameter</c> property.
/// Replaces the runtime <c>CreatesCommandBindingViaCommandParameter</c> binder.
/// Affinity 5 (highest priority).
/// </summary>
/// <remarks>
/// Platforms covered: WPF Button, WinUI Button, MAUI Button, and any control
/// with Command + CommandParameter properties.
/// No Enabled synchronization is needed because these frameworks handle it
/// internally through the Command property binding.
/// </remarks>
internal sealed class CommandPropertyBindingPlugin : ICommandBindingPlugin
{
    /// <summary>The affinity score for the Command property binder (highest priority among command binding plugins).</summary>
    private static readonly int CommandPropertyAffinity = BindingAffinity.Explicit;

    /// <inheritdoc/>
    public int Affinity => CommandPropertyAffinity;

    /// <inheritdoc/>
    public bool RequiresCustomBinderFallback => false;

    /// <inheritdoc/>
    /// <remarks>
    /// Both properties, not just the command. This binder drives execution by assigning them, so a control that
    /// takes a command but no parameter cannot carry a parameter the call site supplied - and binding it anyway
    /// would drop that parameter silently. Stepping aside sends the binding to a mechanism that can take it,
    /// which is what the runtime engine does when either property is missing.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanHandle(BindCommandInvocationInfo inv) =>
        inv.HasCommandProperty && inv.HasCommandParameterProperty;

    /// <inheritdoc/>
    public void EmitBinding(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        bool supportsNullable)
    {
        if (inv.HasObservableParameter)
        {
            EmitObservableParameterBinding(sb, inv, controlAccess, supportsNullable);
            return;
        }

        var tracksParameter = inv is { HasExpressionParameter: true, ParameterPropertyPath: not null };

        _ = sb.AppendLine();
        AppendCapturedOriginals(sb, controlAccess);

        _ = sb.AppendLine("            var serial = new global::ReactiveUI.Primitives.Disposables.SwapDisposable();")
            .AppendLine("            var __cmdSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(commandObs, cmd =>")
            .AppendLine("            {")
            .AppendLine("                serial.Disposable = global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;");

        if (tracksParameter)
        {
            // The parameter is a stream, so the control follows every value the property takes for as long as
            // the command is bound. Subscribing lands the current value before the command is assigned.
            _ = sb.AppendLine("                serial.Disposable = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(")
                .Append("                    withParameter, __p => ").Append(controlAccess)
                .AppendLine(".CommandParameter = __p);");
        }

        // The command is assigned last, so the control never sees one with a parameter still to arrive.
        _ = sb.Append("                ").Append(controlAccess).AppendLine(".Command = cmd;")
            .AppendLine("            });")
            .AppendLine();

        AppendRestoringReturn(sb, controlAccess, "new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__cmdSub, serial)");
    }

    /// <summary>Appends the reads that remember what the control carried before the binding touched it.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="controlAccess">The access chain to the bound control.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendCapturedOriginals(StringBuilder sb, string controlAccess) =>
        sb.Append("            var __originalCommand = ").Append(controlAccess).AppendLine(".Command;")
            .Append("            var __originalParameter = ").Append(controlAccess).AppendLine(".CommandParameter;");

    /// <summary>Appends the return that disposes the binding and puts the control back as it was found.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="controlAccess">The access chain to the bound control.</param>
    /// <param name="subscriptions">The expression producing the binding's own subscriptions.</param>
    /// <remarks>
    /// A binding that is disposed has to leave the control as it found it, or a view rebound to a second view
    /// model keeps executing the first one's command. The parameter is restored before the command, so the
    /// control never briefly holds the old command against the new parameter.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendRestoringReturn(StringBuilder sb, string controlAccess, string subscriptions) =>
        sb.Append("            return new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(")
            .AppendLine()
            .Append("                ").Append(subscriptions).AppendLine(",")
            .AppendLine("                new global::ReactiveUI.Primitives.Disposables.ActionDisposable(() =>")
            .AppendLine("                {")
            .Append("                    ").Append(controlAccess).AppendLine(".CommandParameter = __originalParameter;")
            .Append("                    ").Append(controlAccess).AppendLine(".Command = __originalCommand;")
            .AppendLine("                }));")
            .AppendLine("        }");

    /// <summary>
    /// Emits the binding for the variant that has both a CommandParameter property and an observable
    /// parameter, which must track the latest parameter value and re-subscribe per command.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The BindCommand invocation info.</param>
    /// <param name="controlAccess">The access chain to the bound control.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <remarks>
    /// The parameter is written before the command. Assigning the command is what makes a control ask whether
    /// it can execute, and it asks with whatever parameter it is holding at that moment, so setting the command
    /// first decides the control's enabled state from the parameter belonging to the command it just replaced.
    /// </remarks>
    private static void EmitObservableParameterBinding(
        StringBuilder sb,
        BindCommandInvocationInfo inv,
        string controlAccess,
        bool supportsNullable)
    {
        _ = sb.AppendLine();
        AppendCapturedOriginals(sb, controlAccess);

        var nullableSuffix = supportsNullable && inv.ParameterIsReferenceType ? "?" : string.Empty;
        var writeLatest = CommandBindingSyntax.WriteLatestParameter(inv, "p");

        _ = sb.Append("            ").Append(inv.ParameterTypeFullName).Append(nullableSuffix)
            .AppendLine(" __latestParam = default;")
            .AppendLine("            var __paramSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(")
            .Append("                withParameter, p => ").Append(writeLatest).AppendLine(");")
            .AppendLine()
            .AppendLine("            var serial = new global::ReactiveUI.Primitives.Disposables.SwapDisposable();")
            .AppendLine("            var __cmdSub = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(commandObs, cmd =>")
            .AppendLine("            {")
            .AppendLine("                serial.Disposable = global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;")
            .Append("                var param = ").Append(CommandBindingSyntax.ReadLatestParameter(inv)).AppendLine(";")
            .Append("                ").Append(controlAccess).AppendLine(".CommandParameter = param;")
            .Append("                ").Append(controlAccess).AppendLine(".Command = cmd;")
            .AppendLine("                if (cmd != null)")
            .AppendLine("                {")
            .AppendLine("                    serial.Disposable = global::ReactiveUI.Primitives.SubscribeExtensions.Subscribe(")
            .AppendLine("                        withParameter, p =>")
            .AppendLine("                        {")
            .Append("                            ").Append(writeLatest).AppendLine(";")
            .Append("                            ").Append(controlAccess).AppendLine(".CommandParameter = p;")
            .AppendLine("                        });")
            .AppendLine("                }")
            .AppendLine("            });")
            .AppendLine();

        AppendRestoringReturn(
            sb,
            controlAccess,
            "new global::ReactiveUI.Primitives.Disposables.MultipleDisposable("
            + "new global::ReactiveUI.Primitives.Disposables.MultipleDisposable(__cmdSub, __paramSub), serial)");
    }
}
