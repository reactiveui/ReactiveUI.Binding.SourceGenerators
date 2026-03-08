// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Interactions;

/// <summary>
/// Tests for <see cref="InteractionContext{TInput, TOutput}"/>.
/// </summary>
public class InteractionContextTests
{
    /// <summary>
    /// Verifies that Input returns the value passed at construction.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Input_ReturnsConstructionValue()
    {
        var context = CreateContext("hello");
        await Assert.That(context.Input).IsEqualTo("hello");
    }

    /// <summary>
    /// Verifies that IsHandled is false before SetOutput is called.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task IsHandled_FalseBeforeSetOutput()
    {
        var context = CreateContext("input");
        await Assert.That(context.IsHandled).IsFalse();
    }

    /// <summary>
    /// Verifies that SetOutput sets IsHandled to true.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SetOutput_SetsIsHandledTrue()
    {
        var context = CreateContext("input");
        context.SetOutput(42);
        await Assert.That(context.IsHandled).IsTrue();
    }

    /// <summary>
    /// Verifies that GetOutput returns the value set by SetOutput.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetOutput_ReturnsSetValue()
    {
        var context = CreateContext("input");
        context.SetOutput(42);
        await Assert.That(context.GetOutput()).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies that calling SetOutput twice throws InvalidOperationException.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SetOutput_CalledTwice_Throws()
    {
        var context = CreateContext("input");
        context.SetOutput(1);
        await Assert.That(() => context.SetOutput(2)).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that GetOutput throws when output has not been set.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetOutput_BeforeSetOutput_Throws()
    {
        var context = CreateContext("input");
        await Assert.That(() => context.GetOutput()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Creates an <see cref="InteractionContext{TInput, TOutput}"/> using reflection
    /// since the constructor is internal.
    /// </summary>
    /// <param name="input">The input value for the interaction context.</param>
    /// <returns>A new <see cref="InteractionContext{TInput, TOutput}"/> instance.</returns>
    private static InteractionContext<string, int> CreateContext(string input) =>
        (InteractionContext<string, int>)Activator.CreateInstance(
            typeof(InteractionContext<string, int>),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null,
            [input],
            null)!;
}
