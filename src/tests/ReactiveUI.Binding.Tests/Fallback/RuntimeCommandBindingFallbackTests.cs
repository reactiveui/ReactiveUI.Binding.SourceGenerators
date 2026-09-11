// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>Covers the command binding a call site the generator could not read falls back to.</summary>
public class RuntimeCommandBindingFallbackTests
{
    /// <summary>The expression text reported when the observation faults.</summary>
    private const string BindingExpression = "x => x.Run";

    /// <summary>A null view model holds no command to bind, so the parameter stream stays unsubscribed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WithNoViewModel_BindsNothing()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        Locator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new ClickCommandBinder());
        var parameters = new ManualObservable<object?>();

        using var binding = RuntimeCommandBindingFallback.BindCommand<
            DispatchStubView,
            DispatchStubViewModel,
            ICommand,
            DispatchStubControl>(
            new(),
            null,
            x => x.Run,
            x => x.Control,
            parameters,
            null,
            BindingExpression);

        await Assert.That(parameters.Observer).IsNull();
    }

    /// <summary>A control no registered binder reaches leaves the command unbound rather than faulting.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WithAControlNoBinderReaches_BindsNothing()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        Locator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new ClickCommandBinder());
        var command = new RecordingStubCommand();
        var viewModel = new DispatchStubViewModel { Run = command };

        using var binding = RuntimeCommandBindingFallback.BindCommand<
            DispatchStubView,
            DispatchStubViewModel,
            ICommand,
            UnclaimedStubControl>(
            new(),
            viewModel,
            x => x.Run,
            x => x.Surface,
            new ManualObservable<object?>(),
            null,
            BindingExpression);

        await Assert.That(command.LastParameter).IsNull();
    }

    /// <summary>Naming an event does not find a binder for a control no binder reaches either.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_WithANamedEventAndAControlNoBinderReaches_BindsNothing()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        Locator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new ClickCommandBinder());
        var command = new RecordingStubCommand();
        var viewModel = new DispatchStubViewModel { Run = command };

        using var binding = RuntimeCommandBindingFallback.BindCommand<
            DispatchStubView,
            DispatchStubViewModel,
            ICommand,
            UnclaimedStubControl>(
            new(),
            viewModel,
            x => x.Run,
            x => x.Surface,
            new ManualObservable<object?>(),
            nameof(DispatchStubControl.Click),
            BindingExpression);

        await Assert.That(command.LastParameter).IsNull();
    }
}
