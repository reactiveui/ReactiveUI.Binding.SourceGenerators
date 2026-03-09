// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding;

/// <summary>
/// Default implementation of <see cref="IViewLocator"/> that resolves views for view models
/// using a three-tier resolution strategy: source-generated AOT-safe dispatch, explicit runtime
/// mappings, and service locator fallback.
/// </summary>
public sealed class DefaultViewLocator : IViewLocator
{
    /// <summary>
    /// Synchronization lock for thread-safe access to mappings.
    /// </summary>
#if NET9_0_OR_GREATER
    private static readonly Lock _lock = new();
#else
    private static readonly object _lock = new();
#endif

    /// <summary>
    /// Source-generated dispatch function set by the generated code.
    /// Signature: (viewModelInstance, contract) returns IViewFor or null.
    /// </summary>
    private static Func<object, string, IViewFor?>? _generatedDispatch;

    /// <summary>
    /// Runtime explicit mappings from (viewModelType, contract) to view factory.
    /// Uses copy-on-write semantics for thread safety.
    /// </summary>
    private Dictionary<(Type ViewModelType, string Contract), Func<IViewFor>> _mappings = new();

    /// <summary>
    /// Registers the source-generated view dispatch function.
    /// Called by the source generator's static field initializer on <c>__ReactiveUIGeneratedBindings</c>.
    /// </summary>
    /// <param name="dispatch">The dispatch function that resolves views by type-switching on the view model instance.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetGeneratedViewDispatch(Func<object, string, IViewFor?> dispatch)
    {
        ArgumentExceptionHelper.ThrowIfNull(dispatch);
        _generatedDispatch = dispatch;
    }

    /// <summary>
    /// Registers an explicit view mapping for a view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type. Must implement <see cref="IViewFor"/>.</typeparam>
    /// <param name="contract">An optional contract string for named registrations.</param>
    public void Map<TViewModel, TView>(string? contract = null)
        where TViewModel : class
        where TView : IViewFor, new()
    {
        var key = (typeof(TViewModel), contract ?? string.Empty);
        lock (_lock)
        {
            var copy = new Dictionary<(Type ViewModelType, string Contract), Func<IViewFor>>(_mappings)
            {
                [key] = static () => new TView(),
            };
            _mappings = copy;
        }
    }

    /// <summary>
    /// Registers an explicit view mapping with a custom factory for a view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="factory">A factory function that creates the view.</param>
    /// <param name="contract">An optional contract string for named registrations.</param>
    public void Map<TViewModel>(Func<IViewFor> factory, string? contract = null)
        where TViewModel : class
    {
        ArgumentExceptionHelper.ThrowIfNull(factory);

        var key = (typeof(TViewModel), contract ?? string.Empty);
        lock (_lock)
        {
            var copy = new Dictionary<(Type ViewModelType, string Contract), Func<IViewFor>>(_mappings)
            {
                [key] = factory,
            };
            _mappings = copy;
        }
    }

    /// <summary>
    /// Removes an explicit view mapping for a view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="contract">An optional contract string for named registrations.</param>
    /// <returns><see langword="true"/> if the mapping was removed; otherwise, <see langword="false"/>.</returns>
    public bool Unmap<TViewModel>(string? contract = null)
        where TViewModel : class
    {
        var key = (typeof(TViewModel), contract ?? string.Empty);
        lock (_lock)
        {
            var copy = new Dictionary<(Type ViewModelType, string Contract), Func<IViewFor>>(_mappings);
            var removed = copy.Remove(key);
            _mappings = copy;
            return removed;
        }
    }

    /// <inheritdoc/>
    public IViewFor? ResolveView<TViewModel>(TViewModel viewModel, string? contract = null)
        where TViewModel : class
    {
        if (viewModel is null)
        {
            return null;
        }

        var normalizedContract = contract ?? string.Empty;

        // 1. Source-generated dispatch (AOT-safe)
        var dispatch = _generatedDispatch;
        if (dispatch is not null)
        {
            var result = dispatch(viewModel, normalizedContract);
            if (result is not null)
            {
                SetViewModelOnView(result, viewModel);
                return result;
            }
        }

        // 2. Explicit runtime mappings (AOT-safe)
        var view = TryResolveFromMappings(typeof(TViewModel), normalizedContract);
        if (view is not null)
        {
            SetViewModelOnView(view, viewModel);
            return view;
        }

        // 3. Service locator lookup (AOT-safe for registered types)
        view = AppLocator.Current.GetService<IViewFor<TViewModel>>(
            normalizedContract.Length == 0 ? null : normalizedContract);
        if (view is not null)
        {
            SetViewModelOnView(view, viewModel);
            return view;
        }

        return null;
    }

    /// <inheritdoc/>
    public IViewFor? ResolveView(object? viewModel, string? contract = null)
    {
        if (viewModel is null)
        {
            return null;
        }

        var normalizedContract = contract ?? string.Empty;

        // 1. Source-generated dispatch (AOT-safe type-switch)
        var dispatch = _generatedDispatch;
        if (dispatch is not null)
        {
            var result = dispatch(viewModel, normalizedContract);
            if (result is not null)
            {
                SetViewModelOnView(result, viewModel);
                return result;
            }
        }

        // 2. Explicit runtime mappings (AOT-safe)
        var view = TryResolveFromMappings(viewModel.GetType(), normalizedContract);
        if (view is not null)
        {
            SetViewModelOnView(view, viewModel);
            return view;
        }

        // 3. MakeGenericType fallback (non-AOT, for compatibility)
        return TryResolveViaReflection(viewModel, normalizedContract);
    }

    /// <summary>
    /// Resets the generated view dispatch for testing purposes.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static void ResetGeneratedViewDispatchForTesting() => _generatedDispatch = null;

    /// <summary>
    /// Sets the view model on the resolved view.
    /// </summary>
    /// <param name="view">The view to set the view model on.</param>
    /// <param name="viewModel">The view model instance.</param>
    private static void SetViewModelOnView(IViewFor view, object viewModel)
    {
        view.ViewModel = viewModel;
    }

    /// <summary>
    /// Fallback resolution using MakeGenericType. Not AOT-safe but provides backward compatibility.
    /// </summary>
    /// <param name="viewModel">The view model instance.</param>
    /// <param name="contract">The normalized contract string.</param>
    /// <returns>The resolved view, or <see langword="null"/>.</returns>
    [ExcludeFromCodeCoverage]
    private static IViewFor? TryResolveViaReflection(object viewModel, string contract)
    {
        try
        {
            var vmType = viewModel.GetType();
            var viewForType = typeof(IViewFor<>).MakeGenericType(vmType);
            var svcContract = contract.Length == 0 ? null : contract;
            var view = AppLocator.Current.GetService(viewForType, svcContract) as IViewFor;
            if (view is not null)
            {
                SetViewModelOnView(view, viewModel);
                return view;
            }
        }
        catch
        {
            // MakeGenericType can fail on AOT platforms — this is expected
        }

        return null;
    }

    /// <summary>
    /// Tries to resolve a view from the explicit runtime mappings dictionary.
    /// </summary>
    /// <param name="viewModelType">The type of the view model.</param>
    /// <param name="contract">The normalized contract string.</param>
    /// <returns>The resolved view, or <see langword="null"/>.</returns>
    private IViewFor? TryResolveFromMappings(Type viewModelType, string contract)
    {
        var mappings = _mappings;
        if (mappings.TryGetValue((viewModelType, contract), out var factory))
        {
            return factory();
        }

        return null;
    }
}
