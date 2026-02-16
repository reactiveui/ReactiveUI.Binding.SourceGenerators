// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

namespace ReactiveUI.Binding.Builder;

/// <summary>
/// Static factory for creating <see cref="ReactiveUIBindingBuilder"/> instances.
/// </summary>
/// <example>
/// <code>
/// RxBindingBuilder.CreateReactiveUIBindingBuilder()
///     .WithCoreServices()
///     .BuildApp();
/// </code>
/// </example>
public static class RxBindingBuilder
{
#if NET9_0_OR_GREATER
    private static readonly System.Threading.Lock _resetLock = new();
#else
    private static readonly object _resetLock = new();
#endif

    private static int _hasBeenInitialized; // 0 = false, 1 = true

    /// <summary>
    /// Creates a new <see cref="ReactiveUIBindingBuilder"/> using the current Splat locator.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static ReactiveUIBindingBuilder CreateReactiveUIBindingBuilder() =>
        new(AppLocator.CurrentMutable, AppLocator.Current);

    /// <summary>
    /// Creates a new <see cref="ReactiveUIBindingBuilder"/> using the specified dependency resolver.
    /// </summary>
    /// <param name="resolver">The dependency resolver to use.</param>
    /// <returns>A new builder instance.</returns>
    public static ReactiveUIBindingBuilder CreateReactiveUIBindingBuilder(this IMutableDependencyResolver resolver)
    {
        ArgumentExceptionHelper.ThrowIfNull(resolver);

        var readonlyResolver = resolver as IReadonlyDependencyResolver ?? AppLocator.Current;
        return new(resolver, readonlyResolver);
    }

    /// <summary>
    /// Ensures ReactiveUI.Binding has been initialized via the builder pattern.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <c>BuildApp()</c> has not been called.</exception>
    public static void EnsureInitialized()
    {
        lock (_resetLock)
        {
            if (_hasBeenInitialized == 0)
            {
                throw new InvalidOperationException(
                    "ReactiveUI.Binding has not been initialized. You must initialize using the builder pattern.\n\n" +
                    "Example:\n" +
                    "RxBindingBuilder.CreateReactiveUIBindingBuilder()\n" +
                    "    .WithCoreServices()\n" +
                    "    .BuildApp();");
            }
        }
    }

    /// <summary>
    /// Resets the initialization state for testing purposes only.
    /// </summary>
    /// <remarks>
    /// WARNING: This method should ONLY be used in unit tests. Never call in production code.
    /// </remarks>
    internal static void ResetForTesting()
    {
        lock (_resetLock)
        {
            AppBuilder.ResetBuilderStateForTests();
            AppLocator.SetLocator(new ModernDependencyResolver());
            _hasBeenInitialized = 0;
        }
    }

    /// <summary>
    /// Marks ReactiveUI.Binding as initialized. Called by <see cref="ReactiveUIBindingBuilder.BuildApp"/>.
    /// </summary>
    internal static void MarkAsInitialized()
    {
        lock (_resetLock)
        {
            _hasBeenInitialized = 1;
        }
    }
}
