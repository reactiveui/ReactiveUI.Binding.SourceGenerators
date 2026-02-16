// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.ObservableForProperty;

using Splat;

namespace ReactiveUI.Binding.Tests.Builder;

/// <summary>
/// Tests for the <see cref="ReactiveUIBindingModule"/> class.
/// </summary>
public class ReactiveUIBindingModuleTests
{
    /// <summary>
    /// Verifies that Configure registers INPC and POCO observable for property implementations.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_RegistersINPCAndPOCO()
    {
        var resolver = new ModernDependencyResolver();
        var module = new ReactiveUIBindingModule();

        module.Configure(resolver);

        var services = resolver.GetServices<ICreatesObservableForProperty>().ToList();

        await Assert.That(services.Count).IsEqualTo(2);

        var hasINPC = services.Any(s => s is INPCObservableForProperty);
        var hasPOCO = services.Any(s => s is POCOObservableForProperty);

        await Assert.That(hasINPC).IsTrue();
        await Assert.That(hasPOCO).IsTrue();
    }

    /// <summary>
    /// Verifies that Configure throws for null resolver.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_NullResolver_ThrowsArgumentNullException()
    {
        var module = new ReactiveUIBindingModule();

        await Assert.That(() => module.Configure(null!))
            .ThrowsExactly<ArgumentNullException>();
    }
}
