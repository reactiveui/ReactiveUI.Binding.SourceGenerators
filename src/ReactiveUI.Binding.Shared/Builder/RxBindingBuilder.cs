// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Builder;
#else
namespace ReactiveUI.Binding.Builder;
#endif

/// <summary>Static factory for creating <see cref="ReactiveUIBindingBuilder"/> instances.</summary>
/// <example>
/// <code>
/// RxBindingBuilder.CreateReactiveUIBindingBuilder()
///     .WithCoreServices()
///     .BuildApp();
/// </code>
/// </example>
public static class RxBindingBuilder
{
    /// <summary>Whether ReactiveUI.Binding has been initialized: 0 until <see cref="MarkAsInitialized"/> runs, 1 after.</summary>
    private static int _hasBeenInitialized;

    /// <summary>Creates a new <see cref="ReactiveUIBindingBuilder"/> using the current Splat locator.</summary>
    /// <returns>A new builder instance.</returns>
    public static ReactiveUIBindingBuilder CreateReactiveUIBindingBuilder() =>
        new(AppLocator.CurrentMutable, AppLocator.Current);

    /// <summary>Ensures ReactiveUI.Binding has been initialized via the builder pattern.</summary>
    /// <exception cref="InvalidOperationException">Thrown if <c>BuildApp()</c> has not been called.</exception>
    public static void EnsureInitialized()
    {
        if (Volatile.Read(ref _hasBeenInitialized) != 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "ReactiveUI.Binding has not been initialized. You must initialize using the builder pattern.\n\n"
            + "Example:\n"
            + "RxBindingBuilder.CreateReactiveUIBindingBuilder()\n"
            + "    .WithCoreServices()\n"
            + "    .BuildApp();");
    }

    /// <summary>Resets the initialization state for testing purposes only.</summary>
    /// <remarks>
    /// WARNING: This method should ONLY be used in unit tests. Never call in production code.
    /// </remarks>
    internal static void ResetForTesting()
    {
        AppBuilder.ResetBuilderStateForTests();
        AppLocator.SetLocator(new ModernDependencyResolver());
        Volatile.Write(ref _hasBeenInitialized, 0);
    }

    /// <summary>Marks ReactiveUI.Binding as initialized. Called by <see cref="ReactiveUIBindingBuilder.BuildApp"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void MarkAsInitialized() => Volatile.Write(ref _hasBeenInitialized, 1);
}
