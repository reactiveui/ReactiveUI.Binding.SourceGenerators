// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.WhenAny;

/// <summary>
/// Tests for ObservableForProperty extension methods using the runtime fallback path.
/// </summary>
public class ObservableForPropertyTests
{
    /// <summary>
    /// Verifies that a single property emits on change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsOnChange()
    {
        EnsureInitialized();

        var fixture = new TestFixture();
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.IsNotNullString)
            .Subscribe(changes.Add);

        fixture.IsNotNullString = "Bar";
        fixture.IsNotNullString = "Baz";

        await Assert.That(changes.Count).IsEqualTo(2);
        await Assert.That(changes[0].Value).IsEqualTo("Bar");
        await Assert.That(changes[1].Value).IsEqualTo("Baz");
    }

    /// <summary>
    /// Verifies that ObservableForProperty skips the initial value by default.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_SkipsInitialByDefault()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Initial" };
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.IsNotNullString)
            .Subscribe(changes.Add);

        // Should not have emitted the initial value
        await Assert.That(changes.Count).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that ObservableForProperty emits the initial value when skipInitial is false.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsInitialWhenRequested()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "Initial" };
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.IsNotNullString, skipInitial: false)
            .Subscribe(changes.Add);

        await Assert.That(changes.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(changes[0].Value).IsEqualTo("Initial");
    }

    /// <summary>
    /// Verifies that a deep property chain emits when the leaf property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsOnLeafChange()
    {
        EnsureInitialized();

        var fixture = new HostTestFixture
        {
            Child = new TestFixture { IsOnlyOneWord = "Foo" }
        };
        var changes = new List<IObservedChange<HostTestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.Child!.IsOnlyOneWord, skipInitial: false)
            .Subscribe(changes.Add);

        await Assert.That(changes.Count).IsGreaterThanOrEqualTo(1);

        fixture.Child.IsOnlyOneWord = "Bar";

        await Assert.That(changes.Count).IsGreaterThanOrEqualTo(2);
    }

    /// <summary>
    /// Verifies that a deep chain resubscribes when an intermediate object changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_ResubscribesOnIntermediateChange()
    {
        EnsureInitialized();

        var fixture = new HostTestFixture
        {
            Child = new TestFixture { IsOnlyOneWord = "Foo" }
        };
        var changes = new List<IObservedChange<HostTestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.Child!.IsOnlyOneWord, skipInitial: false)
            .Subscribe(changes.Add);

        var initialCount = changes.Count;

        // Replace the intermediate object entirely
        fixture.Child = new TestFixture { IsOnlyOneWord = "Baz" };

        await Assert.That(changes.Count).IsGreaterThan(initialCount);
    }

    /// <summary>
    /// Verifies that a deep chain handles null intermediate objects gracefully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_ResubscribesAfterNullThenRestore()
    {
        EnsureInitialized();

        var fixture = new HostTestFixture
        {
            Child = new TestFixture { IsOnlyOneWord = "Foo" }
        };
        var changes = new List<IObservedChange<HostTestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.Child!.IsOnlyOneWord, skipInitial: false)
            .Subscribe(changes.Add);

        var countAfterInitial = changes.Count;

        // Set intermediate to null
        fixture.Child = null;

        // Restore with a new value
        fixture.Child = new TestFixture { IsOnlyOneWord = "Bar" };

        await Assert.That(changes.Count).IsGreaterThan(countAfterInitial);
    }

    /// <summary>
    /// Verifies that a four-level deep chain triggers updates at any level.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourLevelDeepChain_AnyLevelTriggersUpdate()
    {
        EnsureInitialized();

        var fixture = new ObjChain1
        {
            Chain2 = new ObjChain2
            {
                Chain3 = new ObjChain3
                {
                    Host = new HostTestFixture
                    {
                        SomeOtherParam = 42
                    }
                }
            }
        };
        var values = new List<IObservedChange<ObjChain1, int>>();

        using var sub = fixture.ObservableForProperty(
            x => x.Chain2!.Chain3!.Host!.SomeOtherParam,
            skipInitial: false)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);

        // Change the leaf value
        fixture.Chain2!.Chain3!.Host!.SomeOtherParam = 99;

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
    }

    /// <summary>
    /// Verifies that ObservableForProperty works with plain INPC objects.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WorksWithINPCObjects()
    {
        EnsureInitialized();

        var obj = new NonReactiveINPCObject();
        var changes = new List<IObservedChange<NonReactiveINPCObject, string>>();

        using var sub = obj.ObservableForProperty(x => x.InpcProperty)
            .Subscribe(changes.Add);

        obj.InpcProperty = "Hello";

        await Assert.That(changes.Count).IsEqualTo(1);
        await Assert.That(changes[0].Value).IsEqualTo("Hello");
    }

    /// <summary>
    /// Verifies that the string-based overload of ObservableForProperty works.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringOverload_ObservesProperty()
    {
        EnsureInitialized();

        var fixture = new TestFixture();
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty<TestFixture, string>("IsNotNullString")
            .Subscribe(changes.Add);

        fixture.IsNotNullString = "Test";

        await Assert.That(changes.Count).IsEqualTo(1);
        await Assert.That(changes[0].Value).IsEqualTo("Test");
    }

    /// <summary>
    /// Verifies that ObservableForProperty deduplicates same values by default.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IsDistinct_DeduplicatesSameValues()
    {
        EnsureInitialized();

        var fixture = new TestFixture { IsNotNullString = "A" };
        var changes = new List<IObservedChange<TestFixture, string>>();

        using var sub = fixture.ObservableForProperty(x => x.IsNotNullString, isDistinct: true)
            .Subscribe(changes.Add);

        fixture.IsNotNullString = "B";
        fixture.IsNotNullString = "B"; // Same value — should be deduped by equality check in setter

        await Assert.That(changes.Count).IsEqualTo(1);
    }

    internal static void EnsureInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        builder.WithCoreServices();
        builder.BuildApp();
    }
}
