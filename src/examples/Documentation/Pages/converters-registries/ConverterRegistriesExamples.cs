// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.ConverterRegistries;

/// <summary>Examples of working with converter registries and affinity.</summary>
public static class ConverterRegistriesExamples
{
    /// <summary>The affinity score for low-priority converters.</summary>
    private const int LowAffinityScore = 1;

    /// <summary>The affinity score for high-priority converters.</summary>
    private const int HighAffinityScore = 100;

    /// <summary>Demonstrates static access to the converter service.</summary>
    public static void AccessConverterService()
    {
        var service = BindingConverters.Current;

        if (service is null)
        {
            return;
        }

        SampleCheck.Equal(true, service.TypedConverters is not null);
        SampleCheck.Equal(true, service.FallbackConverters is not null);
        SampleCheck.Equal(true, service.SetMethodConverters is not null);
    }

    /// <summary>Demonstrates registering and listing converters.</summary>
    public static void RegisterAndListConverters()
    {
        var service = BindingConverters.Current;
        var initialCount = 0;

        foreach (var _ in service.TypedConverters.GetAllConverters())
        {
            initialCount++;
        }

        var customConverter = new DemoIntToStringConverter(LowAffinityScore);

        service.TypedConverters.Register(customConverter);

        var afterRegisterCount = 0;
        foreach (var _ in service.TypedConverters.GetAllConverters())
        {
            afterRegisterCount++;
        }

        SampleCheck.Equal(true, afterRegisterCount > initialCount);
    }

    /// <summary>Demonstrates how converter affinity determines which converter is selected.</summary>
    public static void DemonstrateAffinitySelection()
    {
        var service = BindingConverters.Current;

        var lowAffinityConverter = new DemoIntToStringConverter(LowAffinityScore);
        var highAffinityConverter = new DemoIntToStringConverter(HighAffinityScore);

        service.TypedConverters.Register(lowAffinityConverter);
        service.TypedConverters.Register(highAffinityConverter);

        var resolved = service.TypedConverters.TryGetConverter(typeof(int), typeof(string));

        if (resolved is null)
        {
            return;
        }

        var affinity = resolved.GetAffinityForObjects();

        SampleCheck.Equal(HighAffinityScore, affinity);
    }

    /// <summary>Demonstrates overriding a built-in converter with a custom one.</summary>
    public static void OverrideBuiltInConverter()
    {
        var service = BindingConverters.Current;

        var customBoolToStringConverter = new CustomBoolToStringConverter();

        service.TypedConverters.Register(customBoolToStringConverter);

        var resolved = service.TypedConverters.TryGetConverter(typeof(bool), typeof(string));

        if (resolved is null)
        {
            return;
        }

        var success = resolved.TryConvertTyped(true, null, out var result);

        SampleCheck.Equal(true, success);
        SampleCheck.Equal("AFFIRMATIVE", result);
    }
}
