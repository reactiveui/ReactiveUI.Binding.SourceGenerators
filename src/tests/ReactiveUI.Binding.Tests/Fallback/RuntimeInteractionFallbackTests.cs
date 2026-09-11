// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>Covers the interaction registration a call site the generator could not read falls back to.</summary>
public class RuntimeInteractionFallbackTests
{
    /// <summary>The expression text reported when the observation faults.</summary>
    private const string BindingExpression = "x => x.Confirm";

    /// <summary>A null view model holds no interaction, so the handler is never registered.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindInteraction_WithNoViewModel_RegistersNothing()
    {
        RuntimeObservationFallbackTests.EnsureInitialized();
        var registrations = 0;

        using var binding = RuntimeInteractionFallback.BindInteraction<DispatchStubViewModel, string, bool>(
            null,
            x => x.Confirm,
            _ =>
            {
                registrations++;
                return new UnregisteredHandler();
            },
            BindingExpression);

        await Assert.That(registrations).IsEqualTo(0);
    }

    /// <summary>Stands in for the registration a handler would hand back.</summary>
    private sealed class UnregisteredHandler : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
