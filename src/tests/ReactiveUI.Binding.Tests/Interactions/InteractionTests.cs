// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Interactions;

/// <summary>
/// Tests for <see cref="Interaction{TInput, TOutput}"/>.
/// </summary>
public class InteractionTests
{
    /// <summary>
    /// Verifies that Handle returns the output set by a synchronous handler.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_SyncHandler_ReturnsOutput()
    {
        var interaction = new Interaction<string, int>();
        using var registration = interaction.RegisterHandler(ctx => ctx.SetOutput(ctx.Input.Length));

        var result = await interaction.Handle("hello");
        await Assert.That(result).IsEqualTo(5);
    }

    /// <summary>
    /// Verifies that Handle returns the output set by an async task handler.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_TaskHandler_ReturnsOutput()
    {
        var interaction = new Interaction<string, bool>();
        using var registration = interaction.RegisterHandler(async ctx =>
        {
            await Task.Yield();
            ctx.SetOutput(true);
        });

        var result = await interaction.Handle("question");
        await Assert.That(result).IsTrue();
    }

    /// <summary>
    /// Verifies that Handle returns the output set by an observable handler.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_ObservableHandler_ReturnsOutput()
    {
        var interaction = new Interaction<string, int>();
        using var registration = interaction.RegisterHandler<int>(ctx =>
        {
            ctx.SetOutput(42);
            return new ReactiveUI.Binding.Observables.ReturnObservable<int>(0);
        });

        var result = await interaction.Handle("test");
        await Assert.That(result).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies that handlers are invoked in LIFO order.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_MultipleHandlers_LIFOOrder()
    {
        var interaction = new Interaction<string, string>();
        using var first = interaction.RegisterHandler(ctx => ctx.SetOutput("first"));
        using var second = interaction.RegisterHandler(ctx => ctx.SetOutput("second"));

        var result = await interaction.Handle("input");
        await Assert.That(result).IsEqualTo("second");
    }

    /// <summary>
    /// Verifies that Handle throws UnhandledInteractionException when no handler calls SetOutput.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_NoHandlers_ThrowsUnhandledInteractionException()
    {
        var interaction = new Interaction<string, int>();
        await Assert.That(async () => await interaction.Handle("test"))
            .ThrowsExactly<UnhandledInteractionException<string, int>>();
    }

    /// <summary>
    /// Verifies that disposing the registration unregisters the handler.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_UnregistersHandler()
    {
        var interaction = new Interaction<string, int>();
        var registration = interaction.RegisterHandler(ctx => ctx.SetOutput(42));
        registration.Dispose();

        await Assert.That(async () => await interaction.Handle("test"))
            .ThrowsExactly<UnhandledInteractionException<string, int>>();
    }

    /// <summary>
    /// Verifies that a handler that doesn't call SetOutput is skipped, and the next handler is tried.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Handle_HandlerSkips_FallsToNext()
    {
        var interaction = new Interaction<string, string>();
        using var first = interaction.RegisterHandler(ctx => ctx.SetOutput("first"));
        using var second = interaction.RegisterHandler(ctx =>
        {
            // Intentionally don't call SetOutput — skip
        });

        var result = await interaction.Handle("input");
        await Assert.That(result).IsEqualTo("first");
    }

    /// <summary>
    /// Verifies that RegisterHandler(Action) throws ArgumentNullException when handler is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegisterHandler_Action_NullHandler_Throws()
    {
        var interaction = new Interaction<string, bool>();
        await Assert.That(() => interaction.RegisterHandler((Action<IInteractionContext<string, bool>>)null!))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that RegisterHandler(Func&lt;Task&gt;) throws ArgumentNullException when handler is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegisterHandler_TaskHandler_NullHandler_Throws()
    {
        var interaction = new Interaction<string, bool>();
        await Assert.That(() => interaction.RegisterHandler((Func<IInteractionContext<string, bool>, Task>)null!))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that RegisterHandler(observable) throws ArgumentNullException when handler is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegisterHandler_ObservableHandler_NullHandler_Throws()
    {
        var interaction = new Interaction<string, bool>();
        await Assert.That(() => interaction.RegisterHandler<int>((Func<IInteractionContext<string, bool>, IObservable<int>>)null!))
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
        using var registration = interaction.RegisterHandler<int>(ctx =>
        {
            ctx.SetOutput(true);
            return new ErrorObservable<int>(new InvalidOperationException("test error"));
        });

        await Assert.That(async () => await interaction.Handle("test"))
            .ThrowsExactly<InvalidOperationException>()
            .WithMessage("test error");
    }

    /// <summary>
    /// Verifies that the Action overload of RegisterHandler sets the output correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RegisterHandler_Action_SetsOutput()
    {
        var interaction = new Interaction<string, int>();
        using var registration = interaction.RegisterHandler(ctx => ctx.SetOutput(99));

        var result = await interaction.Handle("test");
        await Assert.That(result).IsEqualTo(99);
    }

    /// <summary>
    /// An observable that immediately errors on subscribe. Used to test OnError path.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    private sealed class ErrorObservable<T>(Exception error) : IObservable<T>
    {
        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnError(error);
            return ReactiveUI.Binding.Observables.EmptyDisposable.Instance;
        }
    }
}
