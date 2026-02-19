// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;

using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Expression;

/// <summary>
/// Tests for the <see cref="Reflection"/> class.
/// </summary>
public class ReflectionTests
{
    /// <summary>
    /// Verifies that ExpressionToPropertyNames returns the correct name for a simple property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_SimpleProperty_ReturnsName()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var name = Reflection.ExpressionToPropertyNames(body);

        await Assert.That(name).IsEqualTo("Name");
    }

    /// <summary>
    /// Verifies that ExpressionToPropertyNames returns a dotted path for nested properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_NestedProperty_ReturnsDottedPath()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);

        var name = Reflection.ExpressionToPropertyNames(body);

        await Assert.That(name).IsEqualTo("Address.City");
    }

    /// <summary>
    /// Verifies that GetValueFetcherOrThrow returns a working getter for a property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueFetcherOrThrow_PropertyMember_ReturnsGetter()
    {
        var memberInfo = typeof(TestViewModel).GetProperty("Name")!;
        var getter = Reflection.GetValueFetcherOrThrow(memberInfo);

        var vm = new TestViewModel { Name = "Test" };
        var value = getter(vm, null);

        await Assert.That(value).IsEqualTo("Test");
    }

    /// <summary>
    /// Verifies that GetValueSetterOrThrow returns a working setter for a property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueSetterOrThrow_PropertyMember_ReturnsSetter()
    {
        var memberInfo = typeof(TestViewModel).GetProperty("Name")!;
        var setter = Reflection.GetValueSetterOrThrow(memberInfo);

        var vm = new TestViewModel();
        setter(vm, "Hello", null);

        await Assert.That(vm.Name).IsEqualTo("Hello");
    }

    /// <summary>
    /// Verifies that TryGetValueForPropertyChain returns the correct value for a simple chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_SimpleProperty_ReturnsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel { Name = "Alice" };
        var result = Reflection.TryGetValueForPropertyChain<string>(out var value, vm, chain);

        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo("Alice");
    }

    /// <summary>
    /// Verifies that TryGetValueForPropertyChain traverses nested properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_NestedProperty_TraversesChain()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel { Address = new TestAddress { City = "Seattle" } };
        var result = Reflection.TryGetValueForPropertyChain<string>(out var value, vm, chain);

        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo("Seattle");
    }

    /// <summary>
    /// Verifies that TryGetValueForPropertyChain returns false when a null is in the chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_NullInChain_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel(); // Address is null
        var result = Reflection.TryGetValueForPropertyChain<string>(out _, vm, chain);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain sets a value on a simple property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_SimpleProperty_SetsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel();
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "Bob");

        await Assert.That(result).IsTrue();
        await Assert.That(vm.Name).IsEqualTo("Bob");
    }

    /// <summary>
    /// Verifies that TryGetAllValuesForPropertyChain returns intermediate ObservedChange objects for a simple property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_SimpleProperty_ReturnsObservedChanges()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel { Name = "Alice" };
        var result = Reflection.TryGetAllValuesForPropertyChain(out var changeValues, vm, chain);

        await Assert.That(result).IsTrue();
        await Assert.That(changeValues.Length).IsEqualTo(1);
        await Assert.That(changeValues[0]).IsNotNull();
        await Assert.That(changeValues[0].Sender).IsEqualTo(vm);
        await Assert.That(changeValues[0].Value).IsEqualTo("Alice");
    }

    /// <summary>
    /// Verifies that TryGetAllValuesForPropertyChain returns multiple ObservedChange objects for a deep chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_DeepChain_ReturnsMultipleValues()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var address = new TestAddress { City = "Portland" };
        var vm = new TestViewModel { Address = address };
        var result = Reflection.TryGetAllValuesForPropertyChain(out var changeValues, vm, chain);

        await Assert.That(result).IsTrue();
        await Assert.That(changeValues.Length).IsEqualTo(2);
        await Assert.That(changeValues[0]).IsNotNull();
        await Assert.That(changeValues[0].Sender).IsEqualTo(vm);
        await Assert.That(changeValues[0].Value).IsEqualTo(address);
        await Assert.That(changeValues[1]).IsNotNull();
        await Assert.That(changeValues[1].Sender).IsEqualTo(address);
        await Assert.That(changeValues[1].Value).IsEqualTo("Portland");
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain sets a value through a deep property chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_DeepChain_SetsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var address = new TestAddress { City = "Old" };
        var vm = new TestViewModel { Address = address };
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "New");

        await Assert.That(result).IsTrue();
        await Assert.That(address.City).IsEqualTo("New");
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain returns false when a null intermediate is in the chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_NullIntermediate_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel(); // Address is null
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "New", shouldThrow: false);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that Rewrite strips Convert (boxing) expressions and returns the underlying member access.
    /// Covers ExpressionRewriter line 47: ExpressionType.Convert case.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Rewrite_ConvertExpression_StripsConvertAndReturnsMemberAccess()
    {
        // x => (object)x.Age creates a Convert expression wrapping MemberAccess
        Expression<Func<TestViewModel, object>> expr = x => x.Age;
        await Assert.That(expr.Body.NodeType).IsEqualTo(ExpressionType.Convert);

        var rewritten = Reflection.Rewrite(expr.Body);

        // After rewriting, the Convert should be stripped, leaving MemberAccess
        await Assert.That(rewritten.NodeType).IsEqualTo(ExpressionType.MemberAccess);
        var member = (MemberExpression)rewritten;
        await Assert.That(member.Member.Name).IsEqualTo("Age");
    }

    /// <summary>
    /// Verifies that ExpressionToPropertyNames returns a dotted path for a deeper chain using Address.City.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_DeeperChain_ReturnsDottedPath()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);

        var name = Reflection.ExpressionToPropertyNames(body);

        await Assert.That(name).IsEqualTo("Address.City");
    }

    /// <summary>
    /// Verifies that GetValueFetcherForProperty returns null for a member that is neither a field nor a property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueFetcherForProperty_MethodMember_ReturnsNull()
    {
        var methodInfo = typeof(TestViewModel).GetMethod("GetHashCode")!;
        var fetcher = Reflection.GetValueFetcherForProperty(methodInfo);

        await Assert.That(fetcher).IsNull();
    }

    /// <summary>
    /// Verifies that GetValueFetcherForProperty returns a working getter for a field member.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueFetcherForProperty_FieldMember_ReturnsGetter()
    {
        var fieldInfo = typeof(FieldTestModel).GetField("PublicField")!;
        var getter = Reflection.GetValueFetcherForProperty(fieldInfo);

        await Assert.That(getter).IsNotNull();

        var model = new FieldTestModel { PublicField = "FieldValue" };
        var value = getter!(model, null);

        await Assert.That(value).IsEqualTo("FieldValue");
    }

    /// <summary>
    /// Verifies that GetValueSetterForProperty returns a working setter for a field member.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueSetterForProperty_FieldMember_ReturnsSetter()
    {
        var fieldInfo = typeof(FieldTestModel).GetField("PublicField")!;
        var setter = Reflection.GetValueSetterForProperty(fieldInfo);

        await Assert.That(setter).IsNotNull();

        var model = new FieldTestModel();
        setter!(model, "NewValue", null);

        await Assert.That(model.PublicField).IsEqualTo("NewValue");
    }

    /// <summary>
    /// Verifies that GetValueSetterForProperty returns null for a method member.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueSetterForProperty_MethodMember_ReturnsNull()
    {
        var methodInfo = typeof(TestViewModel).GetMethod("GetHashCode")!;
        var setter = Reflection.GetValueSetterForProperty(methodInfo);

        await Assert.That(setter).IsNull();
    }

    /// <summary>
    /// Verifies that GetValueFetcherOrThrow throws for a method member.
    /// </summary>
    [Test]
    public void GetValueFetcherOrThrow_MethodMember_ThrowsArgumentException()
    {
        var methodInfo = typeof(TestViewModel).GetMethod("GetHashCode")!;

        Assert.Throws<ArgumentException>(() => Reflection.GetValueFetcherOrThrow(methodInfo));
    }

    /// <summary>
    /// Verifies that GetValueSetterOrThrow throws for a method member.
    /// </summary>
    [Test]
    public void GetValueSetterOrThrow_MethodMember_ThrowsArgumentException()
    {
        var methodInfo = typeof(TestViewModel).GetMethod("GetHashCode")!;

        Assert.Throws<ArgumentException>(() => Reflection.GetValueSetterOrThrow(methodInfo));
    }

    /// <summary>
    /// Verifies that TryGetAllValuesForPropertyChain returns false when null is in the chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_NullInChain_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel(); // Address is null
        var result = Reflection.TryGetAllValuesForPropertyChain(out _, vm, chain);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that ExpressionToPropertyNames handles index expression paths.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_IndexExpression_ReturnsPathWithIndex()
    {
        Expression<Func<IndexedModel, int>> expr = x => x.Items[0];
        var body = Reflection.Rewrite(expr.Body);

        var name = Reflection.ExpressionToPropertyNames(body);

        await Assert.That(name).Contains("Items");
        await Assert.That(name).Contains("[0]");
    }

    /// <summary>
    /// Verifies that GetValueSetterForProperty returns a working setter for a property member.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueSetterForProperty_PropertyMember_ReturnsSetter()
    {
        var propertyInfo = typeof(TestViewModel).GetProperty("Name")!;
        var setter = Reflection.GetValueSetterForProperty(propertyInfo);

        await Assert.That(setter).IsNotNull();

        var vm = new TestViewModel();
        setter!(vm, "Test", null);

        await Assert.That(vm.Name).IsEqualTo("Test");
    }

    /// <summary>
    /// Verifies that MaterializeExpressions returns the same array when given an array.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MaterializeExpressions_ArrayInput_ReturnsSameArray()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();
        var array = chain as System.Linq.Expressions.Expression[] ?? chain.ToArray();

        var result = Reflection.MaterializeExpressions(array);

        await Assert.That(ReferenceEquals(result, array)).IsTrue();
    }

    /// <summary>
    /// Verifies that MaterializeExpressions returns empty array for an empty ICollection.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MaterializeExpressions_EmptyCollection_ReturnsEmptyArray()
    {
        var emptyList = new List<System.Linq.Expressions.Expression>();

        var result = Reflection.MaterializeExpressions(emptyList);

        await Assert.That(result.Length).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that MaterializeExpressions copies from a non-empty ICollection.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MaterializeExpressions_NonEmptyCollection_CopiesElements()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();
        var list = new List<System.Linq.Expressions.Expression>(chain);

        var result = Reflection.MaterializeExpressions(list);

        await Assert.That(result.Length).IsEqualTo(list.Count);
    }

    /// <summary>
    /// Verifies that MaterializeExpressions handles a general IEnumerable (not array or ICollection).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MaterializeExpressions_GeneralEnumerable_MaterializesToArray()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        // Wrap in a pure IEnumerable using a generator method
        static IEnumerable<System.Linq.Expressions.Expression> AsEnumerable(IEnumerable<System.Linq.Expressions.Expression> src)
        {
            foreach (var item in src)
            {
                yield return item;
            }
        }

        var result = Reflection.MaterializeExpressions(AsEnumerable(chain));

        await Assert.That(result.Length).IsGreaterThan(0);
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain with shouldThrow=false
    /// and a setter returns null for an unsupported member returns false.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_ShouldThrowFalse_NullIntermediate_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel(); // Address is null
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "New", shouldThrow: false);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that GetValueFetcherForProperty for a field throws when the field value is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueFetcherForProperty_FieldMember_NullValue_Throws()
    {
        var fieldInfo = typeof(FieldTestModel).GetField("PublicField")!;
        var getter = Reflection.GetValueFetcherForProperty(fieldInfo);

        await Assert.That(getter).IsNotNull();

        var model = new FieldTestModel { PublicField = null! };
        var act = () => getter!(model, null);

        await Assert.That(act).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that TryGetValueForPropertyChain throws for empty expression chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_EmptyChain_ThrowsInvalidOperationException()
    {
        var act = () => Reflection.TryGetValueForPropertyChain<string>(out _, new TestModels.TestViewModel(), Array.Empty<System.Linq.Expressions.Expression>());

        await Assert.That(act).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that TryGetAllValuesForPropertyChain throws for empty expression chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_EmptyChain_ThrowsInvalidOperationException()
    {
        var act = () => Reflection.TryGetAllValuesForPropertyChain(out _, new TestModels.TestViewModel(), Array.Empty<System.Linq.Expressions.Expression>());

        await Assert.That(act).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain throws for empty expression chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_EmptyChain_ThrowsInvalidOperationException()
    {
        var act = () => Reflection.TrySetValueToPropertyChain(new TestModels.TestViewModel(), Array.Empty<System.Linq.Expressions.Expression>(), "value");

        await Assert.That(act).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that TryGetValueForPropertyChain returns false when the root object is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_NullRoot_ReturnsFalse()
    {
        Expression<Func<TestModels.TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        // For a single-property chain, null root hits the "if (current is null)" check at line 186
        var result = Reflection.TryGetValueForPropertyChain<string>(out _, null, chain);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that TryGetAllValuesForPropertyChain returns false when the last object in chain is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_NullAtLastStep_ReturnsFalse()
    {
        Expression<Func<TestModels.TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        // Null root with single-property chain hits the null check before last property
        var result = Reflection.TryGetAllValuesForPropertyChain(out var values, null, chain);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain returns false when target is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_NullTarget_ReturnsFalse()
    {
        Expression<Func<TestModels.TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var result = Reflection.TrySetValueToPropertyChain<string>(null, chain, "value");

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// A test model with a public field for testing field-based reflection.
    /// </summary>
    public class FieldTestModel
    {
#pragma warning disable SA1401 // Fields should be private
        /// <summary>
        /// A public field for testing field-based reflection.
        /// </summary>
        public string PublicField = string.Empty;
#pragma warning restore SA1401 // Fields should be private
    }

    /// <summary>
    /// A test model with an indexed property for testing index expressions.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used as type parameter in expression lambdas.")]
    private sealed class IndexedModel
    {
        public List<int> Items { get; } = [10, 20, 30];
    }
}
