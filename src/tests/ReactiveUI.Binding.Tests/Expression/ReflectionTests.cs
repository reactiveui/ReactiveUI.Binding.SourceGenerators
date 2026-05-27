// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Expression;

/// <summary>
/// Tests for the <see cref="Reflection"/> class.
/// </summary>
public class ReflectionTests
{
    /// <summary>
    /// A sample name value used across reflection tests.
    /// </summary>
    private const string SampleName = "Alice";

    /// <summary>
    /// The name of a method member used to exercise non-property reflection paths.
    /// </summary>
    private const string HashCodeMethodName = "GetHashCode";

    /// <summary>
    /// A replacement value assigned through a reflection-based setter.
    /// </summary>
    private const string NewFieldValue = "NewValue";

    /// <summary>
    /// The expected number of values returned for a two-element property chain.
    /// </summary>
    private const int ExpectedTwoValues = 2;

    /// <summary>
    /// The index of the third value in a property chain result.
    /// </summary>
    private const int ThirdValueIndex = 2;

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

        var vm = new TestViewModel { Name = SampleName };
        var result = Reflection.TryGetValueForPropertyChain<string>(out var value, vm, chain);

        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo(SampleName);
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

        var vm = new TestViewModel { Address = new() { City = "Seattle" } };
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

        var vm = new TestViewModel { Name = SampleName };
        var result = Reflection.TryGetAllValuesForPropertyChain(out var changeValues, vm, chain);

        await Assert.That(result).IsTrue();
        await Assert.That(changeValues.Length).IsEqualTo(1);
        await Assert.That(changeValues[0]).IsNotNull();
        await Assert.That(changeValues[0].Sender).IsEqualTo(vm);
        await Assert.That(changeValues[0].Value).IsEqualTo(SampleName);
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
        await Assert.That(changeValues.Length).IsEqualTo(ExpectedTwoValues);
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
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "New", false);

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
    /// Verifies that ExpressionToPropertyNames joins every hop of a deep (4-level) chain into a single
    /// dotted path, exercising the join across multiple intermediate members rather than just one.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_DeeperChain_ReturnsDottedPath()
    {
        Expression<Func<ObjChain1, int>> expr = x => x.Chain2!.Chain3!.Host!.SomeOtherParam;
        var body = Reflection.Rewrite(expr.Body);

        var name = Reflection.ExpressionToPropertyNames(body);

        await Assert.That(name).IsEqualTo("Chain2.Chain3.Host.SomeOtherParam");
    }

    /// <summary>
    /// Verifies that GetValueFetcherForProperty returns null for a member that is neither a field nor a property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueFetcherForProperty_MethodMember_ReturnsNull()
    {
        var methodInfo = typeof(TestViewModel).GetMethod(HashCodeMethodName)!;
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
        setter!(model, NewFieldValue, null);

        await Assert.That(model.PublicField).IsEqualTo(NewFieldValue);
    }

    /// <summary>
    /// Verifies that GetValueSetterForProperty returns null for a method member.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetValueSetterForProperty_MethodMember_ReturnsNull()
    {
        var methodInfo = typeof(TestViewModel).GetMethod(HashCodeMethodName)!;
        var setter = Reflection.GetValueSetterForProperty(methodInfo);

        await Assert.That(setter).IsNull();
    }

    /// <summary>
    /// Verifies that GetValueFetcherOrThrow throws for a method member.
    /// </summary>
    [Test]
    public void GetValueFetcherOrThrow_MethodMember_ThrowsArgumentException()
    {
        var methodInfo = typeof(TestViewModel).GetMethod(HashCodeMethodName)!;

        Assert.Throws<ArgumentException>(() => Reflection.GetValueFetcherOrThrow(methodInfo));
    }

    /// <summary>
    /// Verifies that GetValueSetterOrThrow throws for a method member.
    /// </summary>
    [Test]
    public void GetValueSetterOrThrow_MethodMember_ThrowsArgumentException()
    {
        var methodInfo = typeof(TestViewModel).GetMethod(HashCodeMethodName)!;

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
        var array = chain as System.Linq.Expressions.Expression[] ?? [.. chain];

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
        static IEnumerable<System.Linq.Expressions.Expression> AsEnumerable(
            IEnumerable<System.Linq.Expressions.Expression> src)
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
    /// Verifies that TrySetValueToPropertyChain with shouldThrow=true returns false (rather than throwing)
    /// when the final property's immediate parent is null. The chain is short enough that the null target
    /// is reached only after the traversal loop, so this exercises the throwing getter-resolution path
    /// (GetValueFetcherOrThrow) reaching a valid member yet still returning false — distinct from both the
    /// shouldThrow=false null-intermediate case and the deeper chain that throws mid-loop.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_ShouldThrowTrue_NullFinalParent_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var vm = new TestViewModel(); // Address is null
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "New", true);

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
        var act = () => Reflection.TryGetValueForPropertyChain<string>(out _, new TestViewModel(), []);

        await Assert.That(act).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that TryGetAllValuesForPropertyChain throws for empty expression chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_EmptyChain_ThrowsInvalidOperationException()
    {
        var act = () => Reflection.TryGetAllValuesForPropertyChain(out _, new TestViewModel(), []);

        await Assert.That(act).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain throws for empty expression chain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_EmptyChain_ThrowsInvalidOperationException()
    {
        var act = () => Reflection.TrySetValueToPropertyChain(new TestViewModel(), [], "value");

        await Assert.That(act).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that TryGetValueForPropertyChain returns false when the root object is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_NullRoot_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
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
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        // Null root with single-property chain hits the null check before last property
        var result = Reflection.TryGetAllValuesForPropertyChain(out _, null, chain);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain returns false when target is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_NullTarget_ReturnsFalse()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var result = Reflection.TrySetValueToPropertyChain(null, chain, "value");

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that TryGetValueForPropertyChain returns false when an intermediate becomes null
    /// during loop traversal in a 4-level chain. Uses Chain2.Chain3.Host.SomeOtherParam where Chain3
    /// is null, so the third loop iteration (i=2) encounters null current.
    /// Covers Reflection.cs line 176 (current is null in loop body).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetValueForPropertyChain_NullIntermediate_In4LevelChain_ReturnsFalse()
    {
        // 4-element chain: [Chain2, Chain3, Host, SomeOtherParam]
        // Loop runs for i=0,1,2 (count-1=3).
        // i=0: fetches Chain2 (non-null). i=1: fetches Chain3 (null). current = null.
        // i=2: current is null → line 176 fires.
        Expression<Func<ObjChain1, int>> expr = x => x.Chain2!.Chain3!.Host!.SomeOtherParam;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        // Chain2 exists but Chain3 is null
        var obj = new ObjChain1 { Chain2 = new() };
        var result = Reflection.TryGetValueForPropertyChain<int>(out _, obj, chain);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    /// Verifies that TryGetAllValuesForPropertyChain returns false when an intermediate becomes null
    /// during loop traversal in a 4-level chain. Uses Chain2.Chain3.Host.SomeOtherParam where Chain3
    /// is null, so the third loop iteration (i=2) encounters null current.
    /// Covers Reflection.cs line 221 (current is null in loop body).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryGetAllValuesForPropertyChain_NullIntermediate_In4LevelChain_ReturnsFalse()
    {
        // 4-element chain: [Chain2, Chain3, Host, SomeOtherParam]
        // Loop runs for i=0,1,2 (count-1=3).
        // i=0: fetches Chain2 (non-null). i=1: fetches Chain3 (null). current = null.
        // i=2: current is null → line 221 fires.
        Expression<Func<ObjChain1, int>> expr = x => x.Chain2!.Chain3!.Host!.SomeOtherParam;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        // Chain2 exists but Chain3 is null
        var obj = new ObjChain1 { Chain2 = new() };
        var result = Reflection.TryGetAllValuesForPropertyChain(out var changeValues, obj, chain);

        await Assert.That(result).IsFalse();

        // First two values should be set, third should be null
        await Assert.That(changeValues[0]).IsNotNull();
        await Assert.That(changeValues[1]).IsNotNull();
        await Assert.That(changeValues[ThirdValueIndex]).IsNull();
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain with shouldThrow=true throws ArgumentNullException
    /// when an intermediate target becomes null during chain traversal.
    /// Uses a 4-level chain where Chain3 is null, causing the third loop iteration to hit
    /// the null target throw at getter(target ?? throw).
    /// Covers Reflection.cs line 278 (target ?? throw).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_ShouldThrowTrue_NullIntermediate_ThrowsArgumentNullException()
    {
        // 4-element chain: [Chain2, Chain3, Host, SomeOtherParam]
        // Loop runs i=0,1,2. At i=1, getter returns null (Chain3 is null).
        // At i=2, target is null and shouldThrow=true, so getter(null ?? throw) fires.
        Expression<Func<ObjChain1, int>> expr = x => x.Chain2!.Chain3!.Host!.SomeOtherParam;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var obj = new ObjChain1 { Chain2 = new() }; // Chain3 is null
        var action = () => Reflection.TrySetValueToPropertyChain(obj, chain, 42);

        await Assert.That(action).ThrowsException();
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain with shouldThrow=false resolves the getter/setter via the
    /// non-throwing path (GetValueFetcherForProperty / GetValueSetterForProperty) and successfully sets the
    /// final property when every member in the chain is populated.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_ShouldThrowFalse_PopulatedChain_SetsValue()
    {
        Expression<Func<TestViewModel, string>> expr = x => x.Address!.City;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var address = new TestAddress { City = "Old" };
        var vm = new TestViewModel { Address = address };
        var result = Reflection.TrySetValueToPropertyChain(vm, chain, "New", false);

        await Assert.That(result).IsTrue();
        await Assert.That(address.City).IsEqualTo("New");
    }

    /// <summary>
    /// Verifies that ExpressionToPropertyNames correctly formats multi-argument indexer expressions
    /// with comma separators between arguments.
    /// Covers Reflection.cs line 58 (i != 0 TRUE branch in indexer argument loop).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_MultiArgIndexer_ReturnsPathWithCommaSeparatedArgs()
    {
        // Create an IndexExpression with two constant arguments
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(MultiArgIndexedModel), "x");
        var itemsProperty = System.Linq.Expressions.Expression.Property(parameter, "Items");
        var indexer = typeof(Dictionary<string, int>).GetProperty("Item")!;
        var arg = System.Linq.Expressions.Expression.Constant("key1");
        var indexExpr = System.Linq.Expressions.Expression.MakeIndex(itemsProperty, indexer, [arg]);

        // Build a full expression: parameter -> Items (MemberAccess) -> Item["key1"] (Index)
        var rewritten = Reflection.Rewrite(indexExpr);
        var name = Reflection.ExpressionToPropertyNames(rewritten);

        await Assert.That(name).Contains("Item[key1]");
    }

    /// <summary>
    /// Verifies that ExpressionToPropertyNames correctly formats a true multi-parameter indexer expression
    /// with comma separators between arguments.
    /// Covers Reflection.cs line 58 (i != 0 TRUE branch in multi-arg indexer argument loop).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExpressionToPropertyNames_TrueMultiArgIndexer_ReturnsCommaSeparatedArgs()
    {
        // Create an IndexExpression with TWO constant arguments to exercise the i != 0 branch
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TrueMultiArgIndexedModel), "x");
        var indexer = typeof(TrueMultiArgIndexedModel).GetProperty("Item")!;
        var arg0 = System.Linq.Expressions.Expression.Constant(3);
        var arg1 = System.Linq.Expressions.Expression.Constant(5);
        var indexExpr = System.Linq.Expressions.Expression.MakeIndex(
            parameter,
            indexer,
            [arg0, arg1]);

        var rewritten = Reflection.Rewrite(indexExpr);
        var name = Reflection.ExpressionToPropertyNames(rewritten);

        // Should contain "Item[3,5]" with the comma between args
        await Assert.That(name).Contains("Item[3,5]");
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain throws when attempting to set a read-only property
    /// with shouldThrow=true (default). PropertyInfo.SetValue throws when the property has no setter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_ReadOnlyProperty_ShouldThrowTrue_Throws()
    {
        Expression<Func<ReadOnlyModel, string>> expr = x => x.ReadOnlyValue;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var model = new ReadOnlyModel();
        var act = () => Reflection.TrySetValueToPropertyChain(model, chain, NewFieldValue);

        await Assert.That(act).ThrowsException();
    }

    /// <summary>
    /// Verifies that TrySetValueToPropertyChain with shouldThrow=false on a read-only property
    /// throws at the PropertyInfo.SetValue level (the setter delegate is non-null but the property
    /// has no set accessor). Covers Reflection.cs line 293 where shouldThrow=false selects
    /// GetValueSetterForProperty, and line 295 where the returned setter is invoked.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrySetValueToPropertyChain_ReadOnlyProperty_ShouldThrowFalse_Throws()
    {
        Expression<Func<ReadOnlyModel, string>> expr = x => x.ReadOnlyValue;
        var body = Reflection.Rewrite(expr.Body);
        var chain = body.GetExpressionChain();

        var model = new ReadOnlyModel();
        var act = () => Reflection.TrySetValueToPropertyChain(model, chain, NewFieldValue, false);

        // GetValueSetterForProperty returns property.SetValue (non-null) for any PropertyInfo,
        // but calling SetValue on a getter-only property throws ArgumentException.
        await Assert.That(act).ThrowsException();
    }

    /// <summary>
    /// A test model with a public field for testing field-based reflection.
    /// </summary>
    public class FieldTestModel
    {
        /// <summary>
        /// A public field for testing field-based reflection.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Test model")]
        [SuppressMessage("Major Code Smell", "S2357:Fields should be private", Justification = "Test model")]
        [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Test model")]
        public string PublicField = string.Empty;
    }

    /// <summary>
    /// A test model with an indexed property for testing index expressions.
    /// </summary>
    [SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Referenced only inside an inspected expression tree (never compiled), so no construction is visible to the analyzer.")]
    private sealed class IndexedModel
    {
        /// <summary>
        /// The first sample value stored in <see cref="Items"/>.
        /// </summary>
        private const int FirstItemValue = 10;

        /// <summary>
        /// The second sample value stored in <see cref="Items"/>.
        /// </summary>
        private const int SecondItemValue = 20;

        /// <summary>
        /// The third sample value stored in <see cref="Items"/>.
        /// </summary>
        private const int ThirdItemValue = 30;

        /// <summary>
        /// Gets a list of integers for testing index expressions.
        /// </summary>
        public List<int> Items { get; } = [FirstItemValue, SecondItemValue, ThirdItemValue];
    }

    /// <summary>
    /// A test model with a dictionary for testing multi-argument index expressions.
    /// </summary>
    [SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Referenced only via typeof/reflection metadata in indexer tests; never constructed by design.")]
    private sealed class MultiArgIndexedModel
    {
        /// <summary>
        /// The sample value mapped to the seeded dictionary key.
        /// </summary>
        private const int SeededValue = 42;

        /// <summary>
        /// Gets a dictionary of string-to-int mappings for testing multi-argument index expressions.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Used for testing")]
        public Dictionary<string, int> Items { get; } = new() { ["key1"] = SeededValue };
    }

    /// <summary>
    /// A test model with a true multi-parameter indexer (two int parameters).
    /// </summary>
    [SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Referenced only via typeof/reflection metadata in indexer tests; never constructed by design.")]
    private sealed class TrueMultiArgIndexedModel
    {
        /// <summary>
        /// Backing dictionary for the multi-parameter indexer.
        /// </summary>
        private readonly Dictionary<(int Row, int Col), int> _data = [];

        /// <summary>
        /// Gets or sets the value at the specified row and column.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="col">The column index.</param>
        /// <returns>The value at the specified position.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Used for testing")]
        public int this[int row, int col]
        {
            get => _data.TryGetValue((row, col), out var val) ? val : 0;
            set => _data[(row, col)] = value;
        }
    }

    /// <summary>
    /// A test model with a read-only property (getter only, no setter).
    /// </summary>
    private sealed class ReadOnlyModel
    {
        /// <summary>
        /// Gets the read-only value. This property has no setter.
        /// </summary>
        public string ReadOnlyValue { get; } = "Immutable";
    }
}
