// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.CodeGeneration;

/// <summary>
/// Tests for <see cref="ObservationCodeGenerator"/> — multi-property observation.
/// </summary>
public partial class ObservationCodeGeneratorHelperTests
{
    /// <summary>
    /// Verifies GenerateMultiPropertyObservation produces CombineLatest with multiple paths.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_MultiplePaths_GeneratesCombineLatest()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Name", "global::System.String")
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Age", "global::System.Int32")
            ])
        ]);

        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "global::System.String",
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Age"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("CombineLatest");
        await Assert.That(result).Contains("selector");
    }

    /// <summary>
    /// Verifies GenerateMultiPropertyObservation with a mix of shallow and deep chain paths generates both variable styles.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_WithDeepChainPath_GeneratesDeepChainVariables()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Name", "global::System.String")
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
                ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
            ])
        ]);

        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "global::System.String",
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Address.City"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("__propObs0");
        await Assert.That(result).Contains("__propObs1");
        await Assert.That(result).Contains("RxBindingExtensions.Switch(");
        await Assert.That(result).Contains("CombineLatest");
    }

    /// <summary>
    /// Verifies GenerateMultiPropertyObservation with isBeforeChange=true generates PropertyChanging code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_BeforeChange_GeneratesPropertyChangingCode()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Name", "global::System.String")
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Age", "global::System.Int32")
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "global::System.String",
            isBeforeChange: true,
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Age"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).Contains("CombineLatest");
    }

    /// <summary>
    /// Verifies GenerateMultiPropertyObservation before-change with deep chain generates PropertyChanging variable declarations.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_BeforeChange_WithDeepChain_GeneratesPropertyChangingVariables()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Name", "global::System.String")
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Address", "global::TestApp.Address"),
                ModelFactory.CreatePropertyPathSegment("City", "global::System.String", "global::TestApp.Address")
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "global::System.String",
            isBeforeChange: true,
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Address.City"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPChanging: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, true);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyChanging");
        await Assert.That(result).Contains("__propObs1_s0");
        await Assert.That(result).Contains("CombineLatest");
    }

    /// <summary>
    /// Verifies GenerateMultiPropertyObservation with IReactiveObject after-change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_ReactiveObjectAfterChange_GeneratesPropertyObservable()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Name", "global::System.String")
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Age", "global::System.Int32")
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "global::System.String",
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Age"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsIReactiveObject: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("PropertyObservable");
        await Assert.That(result).Contains("CombineLatest");
    }

    /// <summary>
    /// Verifies GenerateMultiPropertyObservation with null classInfo generates code.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_NullClassInfo_GeneratesReturnObservable()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Name", "global::System.String")
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Age", "global::System.Int32")
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "global::System.String",
            hasSelector: true,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Age"]));

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, null, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("CombineLatest");
    }

    /// <summary>
    /// Verifies GenerateMultiPropertyObservation with no selector generates tuple lambda.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GenerateMultiPropertyObservation_NoSelector_GeneratesTupleResult()
    {
        var sb = new StringBuilder();
        var paths = new EquatableArray<EquatableArray<PropertyPathSegment>>([
            new([
                ModelFactory.CreatePropertyPathSegment("Name", "global::System.String")
            ]),
            new([
                ModelFactory.CreatePropertyPathSegment("Age", "global::System.Int32")
            ])
        ]);
        var inv = ModelFactory.CreateInvocationInfo(
            propertyPaths: paths,
            returnTypeFullName: "(global::System.String property1, global::System.Int32 property2)",
            hasSelector: false,
            expressionTexts: new EquatableArray<string>(["x => x.Name", "x => x.Age"]));
        var classInfo = ModelFactory.CreateClassBindingInfo(implementsINPC: true);

        ObservationCodeGenerator.GenerateMultiPropertyObservation(sb, inv, classInfo, false);

        var result = sb.ToString();
        await Assert.That(result).Contains("CombineLatest");
        await Assert.That(result).Contains("property1: p1");
        await Assert.That(result).Contains("property2: p2");
    }
}
