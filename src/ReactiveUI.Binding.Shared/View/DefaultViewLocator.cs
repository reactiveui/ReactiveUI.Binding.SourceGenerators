// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Default implementation of <see cref="IViewLocator"/> that resolves views for view models
/// using a three-tier resolution strategy: source-generated AOT-safe dispatch, explicit runtime
/// mappings, and service locator fallback.
/// </summary>
[DebuggerDisplay("Mappings = {_mappings.Count}")]
public sealed class DefaultViewLocator : IViewLocator
{
    /// <summary>
    /// The source-generated lookups registered by the assemblies that contain views, in registration order.
    /// Signature of each: (viewModelInstance, contract) returns IViewFor or null.
    /// The array is immutable; registration swaps in a new one.
    /// </summary>
    private static Func<object, string, IViewFor?>[] _generatedDispatches = [];

    /// <summary>Synchronization lock for thread-safe access to this instance's mappings.</summary>
    private readonly Lock _lock = new();

    /// <summary>
    /// Runtime explicit mappings from (viewModelType, contract) to view factory.
    /// Uses copy-on-write semantics for thread safety.
    /// </summary>
    private Dictionary<ViewMappingKey, Func<IViewFor>> _mappings = [];

    /// <summary>
    /// Adds a source-generated view dispatch function.
    /// Called by <c>__ReactiveUIGeneratedBindings</c>: from a module initializer in a C# 9 or newer project,
    /// and from its static constructor in an older one. Each assembly that contains views registers its own.
    /// </summary>
    /// <param name="dispatch">The dispatch function that resolves views by type-switching on the view model instance.</param>
    /// <remarks>
    /// Every registered function is consulted, the most recently registered first, and the first view it returns wins.
    /// A view model that two assemblies both have a view for therefore resolves to the view of the assembly that registered last.
    /// Registering a function that is already registered has no effect.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetGeneratedViewDispatch(Func<object, string, IViewFor?> dispatch)
    {
        ArgumentExceptionHelper.ThrowIfNull(dispatch);

        var current = Volatile.Read(ref _generatedDispatches);
        while (Array.IndexOf(current, dispatch) < 0)
        {
            var next = new Func<object, string, IViewFor?>[current.Length + 1];
            Array.Copy(current, next, current.Length);
            next[current.Length] = dispatch;

            var witnessed = Interlocked.CompareExchange(ref _generatedDispatches, next, current);
            if (ReferenceEquals(witnessed, current))
            {
                return;
            }

            current = witnessed;
        }
    }

    /// <summary>Registers an explicit view mapping for a view model type.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type. Must implement <see cref="IViewFor"/>.</typeparam>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Map<TViewModel, TView>()
        where TViewModel : class
        where TView : IViewFor, new() => Map<TViewModel, TView>(null);

    /// <summary>Registers an explicit view mapping for a view model type.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <typeparam name="TView">The view type. Must implement <see cref="IViewFor"/>.</typeparam>
    /// <param name="contract">A contract string for named registrations.</param>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    public void Map<TViewModel, TView>(string? contract)
        where TViewModel : class
        where TView : IViewFor, new()
    {
        var key = new ViewMappingKey(typeof(TViewModel), contract ?? string.Empty);
        lock (_lock)
        {
            _mappings = new(_mappings) { [key] = static () => new TView() };
        }
    }

    /// <summary>Registers an explicit view mapping with a custom factory for a view model type.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="factory">A factory function that creates the view.</param>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Map<TViewModel>(Func<IViewFor> factory)
        where TViewModel : class => Map<TViewModel>(factory, null);

    /// <summary>Registers an explicit view mapping with a custom factory for a view model type.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="factory">A factory function that creates the view.</param>
    /// <param name="contract">A contract string for named registrations.</param>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    public void Map<TViewModel>(Func<IViewFor> factory, string? contract)
        where TViewModel : class
    {
        ArgumentExceptionHelper.ThrowIfNull(factory);

        var key = new ViewMappingKey(typeof(TViewModel), contract ?? string.Empty);
        lock (_lock)
        {
            _mappings = new(_mappings) { [key] = factory };
        }
    }

    /// <summary>Removes an explicit view mapping for a view model type.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <returns><see langword="true"/> if the mapping was removed; otherwise, <see langword="false"/>.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Unmap<TViewModel>()
        where TViewModel : class => Unmap<TViewModel>(null);

    /// <summary>Removes an explicit view mapping for a view model type.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="contract">A contract string for named registrations.</param>
    /// <returns><see langword="true"/> if the mapping was removed; otherwise, <see langword="false"/>.</returns>
    [SuppressMessage("Design", "SST2307:Type parameters should be inferable", Justification = "Specified explicitly by the caller; it identifies the mapping.")]
    public bool Unmap<TViewModel>(string? contract)
        where TViewModel : class
    {
        var key = new ViewMappingKey(typeof(TViewModel), contract ?? string.Empty);
        lock (_lock)
        {
            var copy = new Dictionary<ViewMappingKey, Func<IViewFor>>(_mappings);
            var removed = copy.Remove(key);
            _mappings = copy;
            return removed;
        }
    }

    /// <inheritdoc/>
    public IViewFor? ResolveView<TViewModel>(TViewModel viewModel, string? contract)
        where TViewModel : class
    {
        if (viewModel is null)
        {
            return null;
        }

        var normalizedContract = contract ?? string.Empty;

        // 1. Source-generated dispatch (AOT-safe)
        var result = TryResolveFromGeneratedDispatches(viewModel, normalizedContract);
        if (result is not null)
        {
            SetViewModelOnView(result, viewModel);
            return result;
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
        if (view is null)
        {
            return null;
        }

        SetViewModelOnView(view, viewModel);
        return view;
    }

    /// <inheritdoc/>
    [RequiresDynamicCode("Resolving a view from an object closes IViewFor<> over its runtime type. Use the generic overload, or register the view, to stay ahead-of-time safe.")]
    public IViewFor? ResolveView(object? viewModel, string? contract)
    {
        if (viewModel is null)
        {
            return null;
        }

        var normalizedContract = contract ?? string.Empty;

        // 1. Source-generated dispatch (AOT-safe type-switch)
        var result = TryResolveFromGeneratedDispatches(viewModel, normalizedContract);
        if (result is not null)
        {
            SetViewModelOnView(result, viewModel);
            return result;
        }

        // 2. Explicit runtime mappings (AOT-safe)
        var view = TryResolveFromMappings(viewModel.GetType(), normalizedContract);
        if (view is not null)
        {
            SetViewModelOnView(view, viewModel);
            return view;
        }

        // 3. Closing IViewFor<> over the runtime type, which needs an instantiation the compiler never saw
        return TryResolveViaReflection(viewModel, normalizedContract);
    }

    /// <summary>Creates a new <see cref="ViewMappingBuilder"/> for fluent registration of view-to-view-model mappings.</summary>
    /// <returns>A new <see cref="ViewMappingBuilder"/> targeting this locator instance.</returns>
    public ViewMappingBuilder CreateMappingBuilder() => new(this);

    /// <summary>Removes every registered generated view dispatch for testing purposes.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ResetGeneratedViewDispatchForTesting() => Volatile.Write(ref _generatedDispatches, []);

    /// <summary>Asks each registered generated lookup for a view, the most recently registered first.</summary>
    /// <param name="viewModel">The view model instance.</param>
    /// <param name="contract">The normalized contract string.</param>
    /// <returns>The first view a lookup returns, or <see langword="null"/>.</returns>
    private static IViewFor? TryResolveFromGeneratedDispatches(object viewModel, string contract)
    {
        var dispatches = Volatile.Read(ref _generatedDispatches);
        for (var i = dispatches.Length - 1; i >= 0; i--)
        {
            var view = dispatches[i](viewModel, contract);
            if (view is not null)
            {
                return view;
            }
        }

        return null;
    }

    /// <summary>Sets the view model on the resolved view.</summary>
    /// <param name="view">The view to set the view model on.</param>
    /// <param name="viewModel">The view model instance.</param>
    private static void SetViewModelOnView(IViewFor view, object viewModel) => view.ViewModel = viewModel;

    /// <summary>Fallback resolution using MakeGenericType. Not AOT-safe but provides backward compatibility.</summary>
    /// <param name="viewModel">The view model instance.</param>
    /// <param name="contract">The normalized contract string.</param>
    /// <returns>The resolved view, or <see langword="null"/>.</returns>
    /// <remarks>
    /// Closing <c>IViewFor&lt;&gt;</c> over a runtime type needs the runtime to build an instantiation the
    /// compiler never saw, which an ahead-of-time build cannot do. Asking the runtime whether it can rather
    /// than attempting it and catching keeps this tier off the AOT analyser's books and off the exception path.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    [RequiresDynamicCode("Closes IViewFor<> over the view model's runtime type, which needs an instantiation the compiler never saw.")]
    private static IViewFor? TryResolveViaReflection(object viewModel, string contract)
    {
        try
        {
            var viewModelType = viewModel.GetType();
            var viewForType = typeof(IViewFor<>).MakeGenericType(viewModelType);
            var view = AppLocator.Current.GetService(viewForType, contract.Length == 0 ? null : contract) as IViewFor;
            if (view is not null)
            {
                SetViewModelOnView(view, viewModel);
                return view;
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException or TypeLoadException or ArgumentException)
        {
            // A closed IViewFor<> the compiler never saw cannot be built where its instantiation was trimmed,
            // so the caller falls back to the other resolution tiers
        }

        return null;
    }

    /// <summary>Tries to resolve a view from the explicit runtime mappings dictionary.</summary>
    /// <param name="viewModelType">The type of the view model.</param>
    /// <param name="contract">The normalized contract string.</param>
    /// <returns>The resolved view, or <see langword="null"/>.</returns>
    private IViewFor? TryResolveFromMappings(Type viewModelType, string contract) =>
        !_mappings.TryGetValue(new(viewModelType, contract), out var factory) ? null : factory();
}
