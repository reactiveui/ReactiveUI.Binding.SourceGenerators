// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;

namespace ReactiveUI.Binding.Tests.Builder;

/// <summary>
/// Tests for the <see cref="RxBindingBuilder"/> static factory class.
/// </summary>
public class RxBindingBuilderTests
{
    /// <summary>
    /// Verifies that CreateReactiveUIBindingBuilder returns a non-null builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateReactiveUIBindingBuilder_ReturnsBuilder()
    {
        RxBindingBuilder.ResetForTesting();

        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        await Assert.That(builder).IsNotNull();
    }

    /// <summary>
    /// Verifies that EnsureInitialized throws when not initialized.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EnsureInitialized_NotInitialized_ThrowsInvalidOperationException()
    {
        RxBindingBuilder.ResetForTesting();

        await Assert.That(RxBindingBuilder.EnsureInitialized)
            .ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that EnsureInitialized does not throw after BuildApp has been called.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EnsureInitialized_AfterBuildApp_DoesNotThrow()
    {
        RxBindingBuilder.ResetForTesting();

        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();
        builder.BuildApp();

        await Assert.That(RxBindingBuilder.EnsureInitialized).ThrowsNothing();
    }

    /// <summary>
    /// Verifies that ResetForTesting resets the initialization state.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ResetForTesting_ResetsInitializationState()
    {
        RxBindingBuilder.ResetForTesting();

        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();
        builder.BuildApp();

        // Should not throw
        RxBindingBuilder.EnsureInitialized();

        // Reset
        RxBindingBuilder.ResetForTesting();

        // Should throw again
        await Assert.That(RxBindingBuilder.EnsureInitialized)
            .ThrowsExactly<InvalidOperationException>();
    }
}
