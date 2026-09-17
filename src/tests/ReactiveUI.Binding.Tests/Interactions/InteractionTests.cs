// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.Interactions;

/// <summary>Tests for <see cref="Interaction{TInput, TOutput}"/>.</summary>
public class InteractionTests
{
    /// <summary>The result the first registered handler produces.</summary>
    private const string FirstHandlerResult = "first";

    /// <summary>The result the second registered handler produces.</summary>
    private const string SecondHandlerResult = "second";

    /// <summary>The expected output of the synchronous handler (the length of "hello").</summary>
    private const int HelloLength = 5;

    /// <summary>A sample output value set by handlers under test.</summary>
    private const int SampleOutput = 42;

    /// <summary>An alternative output value used by the Action-overload test.</summary>
    private const int ActionOutput = 99;

    /// <summary>The input handed to an interaction whose handlers ignore it.</summary>
    private const string HandleInput = "input";

    /// <summary>The number of handlers the handler-list test registers.</summary>
    private const int RegisteredHandlerCount = 2;

    /// <summary>Verifies that Handle returns the output set by a synchronous handler.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_SyncHandler_ReturnsOutput()
    {
        var interaction = new Interaction<string, int>();
        using var registration = interaction.RegisterHandler(static ctx => ctx.SetOutput(ctx.Input.Length));

        var result = await interaction.Handle("hello");
        await Assert.That(result).IsEqualTo(HelloLength);
    }

    /// <summary>Verifies that Handle returns the output set by an async task handler.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_TaskHandler_ReturnsOutput()
    {
        var interaction = new Interaction<string, bool>();
        using var registration = interaction.RegisterHandler(static async ctx =>
        {
            await Task.Yield();
            ctx.SetOutput(true);
        });

        var result = await interaction.Handle("question");
        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that Handle returns the output set by an observable handler.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_ObservableHandler_ReturnsOutput()
    {
        var interaction = new Interaction<string, int>();
        using var registration = interaction.RegisterHandler(static ctx =>
        {
            ctx.SetOutput(SampleOutput);
            return new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<int>(0);
        });

        var result = await interaction.Handle("test");
        await Assert.That(result).IsEqualTo(SampleOutput);
    }

    /// <summary>Verifies that handlers are invoked in LIFO order.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_MultipleHandlers_LIFOOrder()
    {
        var interaction = new Interaction<string, string>();
        using var first = interaction.RegisterHandler(static ctx => ctx.SetOutput(FirstHandlerResult));
        using var second = interaction.RegisterHandler(static ctx => ctx.SetOutput(SecondHandlerResult));

        var result = await interaction.Handle(HandleInput);
        await Assert.That(result).IsEqualTo(SecondHandlerResult);
    }

    /// <summary>Verifies that GetHandlers hands back a copy, so changing it leaves the registered handlers alone.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetHandlers_ReturnsACopyOfTheRegisteredHandlers()
    {
        var interaction = new HandlerListingInteraction();
        using var first = interaction.RegisterHandler(static ctx => ctx.SetOutput(FirstHandlerResult));
        using var second = interaction.RegisterHandler(static ctx => ctx.SetOutput(SecondHandlerResult));

        var handlers = interaction.ListHandlers();
        Array.Clear(handlers);

        await Assert.That(interaction.ListHandlers().Length).IsEqualTo(RegisteredHandlerCount);
        await Assert.That(interaction.ListHandlers()[1]).IsNotNull();
        await Assert.That(await interaction.Handle(HandleInput)).IsEqualTo(SecondHandlerResult);
    }

    /// <summary>Verifies that Handle throws UnhandledInteractionException when no handler calls SetOutput.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_NoHandlers_ThrowsUnhandledInteractionException()
    {
        var interaction = new Interaction<string, int>();
        await Assert.That(() => interaction.Handle("test"))
            .ThrowsExactly<UnhandledInteractionException<string, int>>();
    }

    /// <summary>Verifies that disposing the registration unregisters the handler.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_UnregistersHandler()
    {
        var interaction = new Interaction<string, int>();
        var registration = interaction.RegisterHandler(static ctx => ctx.SetOutput(SampleOutput));
        registration.Dispose();

        await Assert.That(() => interaction.Handle("test"))
            .ThrowsExactly<UnhandledInteractionException<string, int>>();
    }

    /// <summary>Verifies that a handler that doesn't call SetOutput is skipped, and the next handler is tried.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_HandlerSkips_FallsToNext()
    {
        var interaction = new Interaction<string, string>();
        using var first = interaction.RegisterHandler(static ctx => ctx.SetOutput(FirstHandlerResult));
        using var second = interaction.RegisterHandler(static ctx =>
        {
            // Intentionally don't call SetOutput — skip
        });

        var result = await interaction.Handle(HandleInput);
        await Assert.That(result).IsEqualTo(FirstHandlerResult);
    }

    /// <summary>Verifies that RegisterHandler(Action) throws ArgumentNullException when handler is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegisterHandler_Action_NullHandler_Throws()
    {
        var interaction = new Interaction<string, bool>();
        await Assert.That(() => interaction.RegisterHandler((Action<IInteractionContext<string, bool>>)null!))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that RegisterHandler(Func&lt;Task&gt;) throws ArgumentNullException when handler is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegisterHandler_TaskHandler_NullHandler_Throws()
    {
        var interaction = new Interaction<string, bool>();
        await Assert.That(() => interaction.RegisterHandler((Func<IInteractionContext<string, bool>, Task>)null!))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that RegisterHandler(observable) throws ArgumentNullException when handler is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegisterHandler_ObservableHandler_NullHandler_Throws()
    {
        var interaction = new Interaction<string, bool>();
        await Assert.That(() =>
                interaction.RegisterHandler((Func<IInteractionContext<string, bool>, IObservable<int>>)null!))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that an observable handler that errors propagates the exception through Handle.
    /// Covers ObservableToTaskObserver.OnError.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_ObservableHandler_OnError_PropagatesException()
    {
        var interaction = new Interaction<string, bool>();
        using var registration = interaction.RegisterHandler(static ctx =>
        {
            ctx.SetOutput(true);
            return new ErrorObservable<int>(new InvalidOperationException("test error"));
        });

        await Assert.That(() => interaction.Handle("test"))
            .ThrowsExactly<InvalidOperationException>()
            .WithMessage("test error", StringComparison.Ordinal);
    }

    /// <summary>Verifies that the Action overload of RegisterHandler sets the output correctly.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegisterHandler_Action_SetsOutput()
    {
        var interaction = new Interaction<string, int>();
        using var registration = interaction.RegisterHandler(static ctx => ctx.SetOutput(ActionOutput));

        var result = await interaction.Handle("test");
        await Assert.That(result).IsEqualTo(ActionOutput);
    }

    /// <summary>An observable that immediately errors on subscribe. Used to test OnError path.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="error">The exception this observable faults with on subscription.</param>
    private sealed class ErrorObservable<T>(Exception error) : IObservable<T>
    {
        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnError(error);
            return EmptyDisposable.Instance;
        }
    }

    /// <summary>An interaction that exposes its protected handler list to the tests.</summary>
    private sealed class HandlerListingInteraction : Interaction<string, string>
    {
        /// <summary>Lists the registered handlers.</summary>
        /// <returns>The registered handlers, in registration order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Func<IInteractionContext<string, string>, Task>[] ListHandlers() => GetHandlers();
    }
}
