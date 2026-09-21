// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.FallbackConverters;

/// <summary>Examples of fallback and set-method converters.</summary>
public static class FallbackConvertersExamples
{
    /// <summary>Demonstrates registering a fallback converter for runtime type conversions.</summary>
    public static void RegisterFallbackConverter()
    {
        var service = BindingConverters.Current;
        var fallbackConverter = new CustomFallbackConverter();

        service.FallbackConverters.Register(fallbackConverter);

        var count = 0;
        foreach (var _ in service.FallbackConverters.GetAllConverters())
        {
            count++;
        }

        SampleCheck.Equal(true, count > 0);
    }

    /// <summary>Demonstrates registering a set-method converter for specialized write operations.</summary>
    public static void RegisterSetMethodConverter()
    {
        var service = BindingConverters.Current;
        var setMethodConverter = new DemoSetMethodConverter();

        service.SetMethodConverters.Register(setMethodConverter);

        var count = 0;
        foreach (var _ in service.SetMethodConverters.GetAllConverters())
        {
            count++;
        }

        SampleCheck.Equal(true, count > 0);
    }

    /// <summary>Demonstrates that fallback converters are consulted after typed converters fail.</summary>
    public static void DemonstrateFallbackSelection()
    {
        var service = BindingConverters.Current;
        var fallbackConverter = new CustomFallbackConverter();

        service.FallbackConverters.Register(fallbackConverter);

        var affinity = fallbackConverter.GetAffinityForObjects(typeof(object), typeof(string));

        SampleCheck.Equal(true, affinity > 0);

        var success = fallbackConverter.TryConvert(
            typeof(object),
            new(),
            typeof(string),
            null,
            out var result);

        SampleCheck.Equal(true, success);
        SampleCheck.Equal(true, result is not null);
    }
}
