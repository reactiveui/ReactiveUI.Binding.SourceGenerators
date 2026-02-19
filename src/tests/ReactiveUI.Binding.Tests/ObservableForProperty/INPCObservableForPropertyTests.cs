// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reactive.Linq;

using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

using LinqExpression = System.Linq.Expressions.Expression;

namespace ReactiveUI.Binding.Tests.ObservableForProperty;

/// <summary>
/// Tests for the <see cref="INPCObservableForProperty"/> class.
/// </summary>
public class INPCObservableForPropertyTests
{
    /// <summary>
    /// Verifies affinity is 5 for types implementing INotifyPropertyChanged.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_INPCType_Returns5()
    {
        var sut = new INPCObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(TestViewModel), "Name", beforeChanged: false);

        await Assert.That(affinity).IsEqualTo(5);
    }

    /// <summary>
    /// Verifies affinity is 5 for INotifyPropertyChanging when beforeChanged is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_INPCChangingType_Returns5ForBeforeChanged()
    {
        var sut = new INPCObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(TestViewModel), "Name", beforeChanged: true);

        await Assert.That(affinity).IsEqualTo(5);
    }

    /// <summary>
    /// Verifies affinity is 0 for POCO types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_PocoType_Returns0()
    {
        var sut = new INPCObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(PocoModel), "Value", beforeChanged: false);

        await Assert.That(affinity).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that GetNotificationForProperty emits when a property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_PropertyChanged_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new TestViewModel();

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, "Name")
            .Subscribe(_ => emitted = true);

        vm.Name = "Alice";

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>
    /// Verifies that GetNotificationForProperty emits before-change notifications.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_BeforeChanged_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new TestViewModel();

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, "Name", beforeChanged: true)
            .Subscribe(_ => emitted = true);

        vm.Name = "Alice";

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>
    /// Verifies that GetNotificationForProperty filters by property name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_DifferentProperty_DoesNotEmit()
    {
        var sut = new INPCObservableForProperty();
        var vm = new TestViewModel();

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, "Name")
            .Subscribe(_ => emitted = true);

        // Change Age, not Name
        vm.Age = 30;

        await Assert.That(emitted).IsFalse();
    }

    /// <summary>
    /// Verifies that GetNotificationForProperty returns Observable.Never for POCO types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_PocoType_ReturnsNever()
    {
        var sut = new INPCObservableForProperty();
        var model = new PocoModel { Value = "test" };

        Expression<Func<PocoModel, string>> expr = x => x.Value;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(model, body, "Value")
            .Subscribe(_ => emitted = true);

        model.Value = "changed";

        await Assert.That(emitted).IsFalse();
    }

    /// <summary>
    /// Verifies that null or empty PropertyName in PropertyChanged emits for all listeners.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_NullPropertyName_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new NullPropertyNameViewModel();

        Expression<Func<NullPropertyNameViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, "Name")
            .Subscribe(_ => emitted = true);

        vm.RaiseAllPropertiesChanged();

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>
    /// Verifies affinity is 0 for before-change on types without INotifyPropertyChanging.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_NonChangingType_Returns0ForBeforeChanged()
    {
        var sut = new INPCObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(NonChangingViewModel), "Name", beforeChanged: true);

        await Assert.That(affinity).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that GetNotificationForProperty emits for indexer property changes (PropertyChanged path).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_IndexExpression_PropertyChanged_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        // Create an IndexExpression for vm["key"] — NodeType == ExpressionType.Index
        var param = LinqExpression.Parameter(typeof(IndexableViewModel), "x");
        var indexer = typeof(IndexableViewModel).GetProperty("Item")!;
        var indexExpr = LinqExpression.MakeIndex(
            param,
            indexer,
            new[] { LinqExpression.Constant("key") });

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, indexExpr, "Item")
            .Subscribe(_ => emitted = true);

        vm["key"] = "value";

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>
    /// Verifies that GetNotificationForProperty emits for indexer property changes (PropertyChanging/beforeChanged path).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_IndexExpression_BeforeChanged_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        var param = LinqExpression.Parameter(typeof(IndexableViewModel), "x");
        var indexer = typeof(IndexableViewModel).GetProperty("Item")!;
        var indexExpr = LinqExpression.MakeIndex(
            param,
            indexer,
            new[] { LinqExpression.Constant("key") });

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, indexExpr, "Item", beforeChanged: true)
            .Subscribe(_ => emitted = true);

        vm["key"] = "value";

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>
    /// Verifies that indexer property notification does not emit for unrelated property name changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_IndexExpression_DifferentPropertyName_DoesNotEmit()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        var param = LinqExpression.Parameter(typeof(IndexableViewModel), "x");
        var indexer = typeof(IndexableViewModel).GetProperty("Item")!;
        var indexExpr = LinqExpression.MakeIndex(
            param,
            indexer,
            new[] { LinqExpression.Constant("key") });

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, indexExpr, "SomeOtherProperty")
            .Subscribe(_ => emitted = true);

        vm["key"] = "value";

        await Assert.That(emitted).IsFalse();
    }

    /// <summary>
    /// Verifies that indexer property notification emits when PropertyChanged fires with null/empty name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_IndexExpression_NullPropertyName_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new IndexableViewModel();

        var param = LinqExpression.Parameter(typeof(IndexableViewModel), "x");
        var indexer = typeof(IndexableViewModel).GetProperty("Item")!;
        var indexExpr = LinqExpression.MakeIndex(
            param,
            indexer,
            new[] { LinqExpression.Constant("key") });

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, indexExpr, "Item")
            .Subscribe(_ => emitted = true);

        vm.RaiseAllPropertiesChanged();

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>
    /// Verifies that ObservableForProperty by name handles an invalid property name gracefully
    /// by catching the Expression.Property exception and using the parameter expression.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_InvalidProperty_StillCreatesObservable()
    {
        var vm = new TestViewModel();

        var emitted = false;
        using var sub = vm.ObservableForProperty<TestViewModel, string>("NonExistentProperty")
            .Subscribe(_ => emitted = true);

        await Assert.That(emitted).IsFalse();
    }

    /// <summary>
    /// Verifies that ObservableForProperty by name with skipInitial=false emits the initial value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_SkipInitialFalse_EmitsInitialValue()
    {
        var vm = new TestViewModel { Name = "Initial" };

        string? receivedValue = null;
        using var sub = vm.ObservableForProperty<TestViewModel, string>("Name", skipInitial: false)
            .Subscribe(x => receivedValue = x.Value);

        await Assert.That(receivedValue).IsEqualTo("Initial");
    }

    /// <summary>
    /// Verifies that ObservableForProperty by name with isDistinct=false allows duplicate values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_IsDistinctFalse_AllowsDuplicates()
    {
        var vm = new TestViewModel();

        var values = new List<string>();
        using var sub = vm.ObservableForProperty<TestViewModel, string>("Name", isDistinct: false)
            .Subscribe(x => values.Add(x.Value));

        vm.Name = "Alice";
        vm.Name = string.Empty;
        vm.Name = "Alice";

        await Assert.That(values.Count).IsEqualTo(3);
    }

    /// <summary>
    /// Verifies that ObservableForProperty by name handles a nullable property returning null
    /// by returning default in GetCurrentValue.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableForProperty_ByName_NullPropertyValue_ReturnsDefault()
    {
        var vm = new NullablePropertyViewModel();

        string? receivedValue = null;
        var received = false;
        using var sub = vm.ObservableForProperty<NullablePropertyViewModel, string?>("NullableName", skipInitial: false)
            .Subscribe(x =>
            {
                receivedValue = x.Value;
                received = true;
            });

        await Assert.That(received).IsTrue();
        await Assert.That(receivedValue).IsNull();
    }

    /// <summary>
    /// A test model with a nullable property for testing null GetCurrentValue path.
    /// </summary>
    private sealed class NullablePropertyViewModel : INotifyPropertyChanged
    {
        private string? _nullableName;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the nullable name.
        /// </summary>
        public string? NullableName
        {
            get => _nullableName;
            set
            {
                _nullableName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NullableName)));
            }
        }
    }

    /// <summary>
    /// A test model that implements INotifyPropertyChanged but NOT INotifyPropertyChanging.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used as type parameter in typeof() expression.")]
    private sealed class NonChangingViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
#pragma warning disable CS0067 // Event is never used
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// A test model that raises PropertyChanged with null PropertyName.
    /// </summary>
    private sealed class NullPropertyNameViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Raises PropertyChanged with null property name (all properties changed).
        /// </summary>
        public void RaiseAllPropertiesChanged() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    /// <summary>
    /// A test model with an indexer that implements both INotifyPropertyChanged and INotifyPropertyChanging.
    /// </summary>
    private sealed class IndexableViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private readonly Dictionary<string, string> _items = new();

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        public string this[string key]
        {
            get => _items.TryGetValue(key, out var val) ? val : string.Empty;
            set
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs("Item[]"));
                _items[key] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            }
        }

        /// <summary>
        /// Raises PropertyChanged with null property name (all properties changed).
        /// </summary>
        public void RaiseAllPropertiesChanged() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }
}
