// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Controls;
using ReactiveUI.Primitives.Signals;
using Splat;

namespace ReactiveUI.Binding.Documentation.BindingsErrors;

/// <summary>Shows what a binding does when a write to the target faults.</summary>
public static class BindingsErrorsExamples
{
    /// <summary>A reference that fits on a statement.</summary>
    private const string ShortReference = "Rent March";

    /// <summary>A reference that is longer than the bank prints.</summary>
    private const string LongReference = "Rent for the flat on Harbour Street";

    /// <summary>A reference the conversion function cannot show.</summary>
    private const string ForbiddenReference = "forbidden";

    /// <summary>The expression a binding names when it reports a fault.</summary>
    private const string BoundExpression = "x => x.Reference";

    /// <summary>The message of a fault a conversion function raises.</summary>
    private const string ConversionMessage = "The reference is not allowed.";

    /// <summary>The message of the exception a conversion function wraps.</summary>
    private const string CauseMessage = "The reference list is unavailable.";

    /// <summary>The number of entries a binding writes to the log for one fault.</summary>
    private const int OneLogEntry = 1;

    /// <summary>Registers the core services, and a logger that records what the application writes.</summary>
    /// <returns>The logger that records what bindings report.</returns>
    public static RecordingLogger BuildApplication()
    {
        RecordingLogger logger = new();
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices().BuildApp();
        AppLocator.CurrentMutable.RegisterConstant<ILogger>(logger);
        return logger;
    }

    /// <summary>
    /// Raises a fault with no inner exception, as the sequence feeding a write ending in error does. The binding logs
    /// the fault against its expression and rethrows nothing, because the caller can observe the sequence itself.
    /// </summary>
    /// <param name="logger">The logger that records what bindings report.</param>
    public static void FaultWithoutInnerExceptionIsLogged(RecordingLogger logger)
    {
        logger.Clear();
        TransferDraft draft = new() { Reference = ShortReference };
        LabelControl label = new();

        using (draft.BindOneWay(label, x => x.Reference, v => v.Text, static reference => reference == ForbiddenReference ? throw new InvalidOperationException(ConversionMessage) : reference))
        {
            SampleCheck.Equal(ShortReference, label.Text);

            draft.Reference = ForbiddenReference;

            SampleCheck.Equal(OneLogEntry, logger.Exceptions.Count);
            SampleCheck.Equal(ConversionMessage, logger.Exceptions[0].Message);
            SampleCheck.Equal(true, logger.Messages[0].Contains("v => v.Text", StringComparison.Ordinal));

            // The faulted sequence has ended, so the binding writes nothing more.
            draft.Reference = ShortReference;
            SampleCheck.Equal(ShortReference, label.Text);
            draft.Reference = "Rent April";
            SampleCheck.Equal(ShortReference, label.Text);
        }
    }

    /// <summary>
    /// Raises a fault that wraps a cause, as a setter or converter that threw does. The binding logs it and rethrows it
    /// as a <see cref="TargetInvocationException"/> that names the bound expression and carries the cause.
    /// </summary>
    /// <param name="logger">The logger that records what bindings report.</param>
    public static void FaultWithInnerExceptionIsRethrown(RecordingLogger logger)
    {
        logger.Clear();
        TransferDraft draft = new() { Reference = ShortReference };
        LabelControl label = new();
        InvalidOperationException cause = new(CauseMessage);
        TargetInvocationException? rethrown = null;

        using (draft.BindOneWay(label, x => x.Reference, v => v.Text, reference => reference == ForbiddenReference ? throw new InvalidOperationException(ConversionMessage, cause) : reference))
        {
            try
            {
                draft.Reference = ForbiddenReference;
            }
            catch (TargetInvocationException ex)
            {
                rethrown = ex;
            }

            SampleCheck.Equal(true, rethrown is not null);
            SampleCheck.Equal(true, ReferenceEquals(cause, rethrown!.InnerException));
            SampleCheck.Equal(true, rethrown.Message.Contains("v => v.Text", StringComparison.Ordinal));
            SampleCheck.Equal(OneLogEntry, logger.Exceptions.Count);
        }
    }

    /// <summary>
    /// Writes to a target whose setter throws. The setter runs on the thread that raised the change, so the exception
    /// reaches whoever changed the view model property, and a binding created over a bad value throws while it is created.
    /// </summary>
    public static void SetterThatThrowsReachesTheCaller()
    {
        TransferDraft draft = new() { Reference = ShortReference };
        StatementReferenceBox box = new();
        ArgumentException? fromChange = null;
        ArgumentException? fromCreate = null;

        using (draft.BindOneWay(box, x => x.Reference, v => v.Text))
        {
            SampleCheck.Equal(ShortReference, box.Text);

            try
            {
                draft.Reference = LongReference;
            }
            catch (ArgumentException ex)
            {
                fromChange = ex;
            }
        }

        SampleCheck.Equal(true, fromChange is not null);
        SampleCheck.Equal(ShortReference, box.Text);

        try
        {
            using var second = draft.BindOneWay(box, x => x.Reference, v => v.Text);
        }
        catch (ArgumentException ex)
        {
            fromCreate = ex;
        }

        SampleCheck.Equal(true, fromCreate is not null);
    }

    /// <summary>Applies the fault contract directly with <see cref="BindingErrors.Subscribe{T}"/>: values reach the write, and each kind of fault is handled as a binding handles it.</summary>
    /// <param name="logger">The logger that records what bindings report.</param>
    public static void SubscribeAppliesTheFaultContract(RecordingLogger logger)
    {
        logger.Clear();
        List<string> written = [];
        Signal<string> values = new();
        Signal<string> withoutCause = new();
        Signal<string> withCause = new();
        InvalidOperationException cause = new(CauseMessage);
        TargetInvocationException? rethrown = null;

        using (BindingErrors.Subscribe(values, written.Add, BoundExpression))
        using (BindingErrors.Subscribe(withoutCause, written.Add, BoundExpression))
        using (BindingErrors.Subscribe(withCause, written.Add, BoundExpression))
        {
            values.OnNext(ShortReference);
            values.OnCompleted();

            SampleCheck.SequenceEqual([ShortReference], written);
            SampleCheck.Equal(0, logger.Exceptions.Count);

            withoutCause.OnError(new InvalidOperationException(ConversionMessage));

            SampleCheck.Equal(OneLogEntry, logger.Exceptions.Count);

            try
            {
                withCause.OnError(new InvalidOperationException(ConversionMessage, cause));
            }
            catch (TargetInvocationException ex)
            {
                rethrown = ex;
            }

            SampleCheck.Equal(true, rethrown is not null);
            SampleCheck.Equal(true, ReferenceEquals(cause, rethrown!.InnerException));
            SampleCheck.Equal(true, rethrown.Message.Contains(BoundExpression, StringComparison.Ordinal));
        }
    }
}
