[![NuGet Stats](https://img.shields.io/nuget/v/ReactiveUI.Binding.svg)](https://www.nuget.org/packages/ReactiveUI.Binding) [![Build](https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/actions/workflows/ci-build.yml/badge.svg)](https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/actions/workflows/ci-build.yml) [![Code Coverage](https://codecov.io/gh/reactiveui/ReactiveUI.Binding.SourceGenerators/branch/main/graph/badge.svg)](https://codecov.io/gh/reactiveui/ReactiveUI.Binding.SourceGenerators) [![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
<br>
<a href="https://www.nuget.org/packages/ReactiveUI.Binding">
<img src="https://img.shields.io/nuget/dt/ReactiveUI.Binding.svg">
</a>
<a href="https://reactiveui.net/slack">
<img src="https://img.shields.io/badge/chat-slack-blue.svg">
</a>
<a href="https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/labels/good%20first%20issue">
<img src="https://img.shields.io/badge/first--timers--only-friendly-blue.svg">
</a>
<a href="https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/stargazers">
<img src="https://img.shields.io/github/stars/reactiveui/ReactiveUI.Binding.SourceGenerators.svg?style=social">
</a>

<img src="images/logo.png" width="200">

# ReactiveUI.Binding.SourceGenerators

A C# source generator that replaces ReactiveUI's runtime expression-tree binding engine with compile-time code
generation. Zero reflection, fully AOT/trimming safe, 3-7x faster than the legacy engine.

## Table of Contents

- [What does it do?](#what-does-it-do)
- [How does it work?](#how-does-it-work)
- [How do I install?](#how-do-i-install)
- [Supported APIs](#supported-apis)
- [Usage Examples](#usage-examples)
- [View Locator](#view-locator)
- [Supported Notification Mechanisms](#supported-notification-mechanisms)
- [Packages](#packages)
- [Rx Library Compatibility](#rx-library-compatibility)
- [Performance](#performance)
- [Diagnostics](#diagnostics)
- [Architecture](#architecture)
- [Core Team](#core-team)
- [Contribute](#contribute)

### Core Team

<table>
  <tbody>
    <tr>
      <td align="center" valign="top">
        <img width="100" height="100" src="https://github.com/ChrisPulman.png?s=150">
        <br>
        <a href="https://github.com/ChrisPulman">Chris Pulman</a>
        <p>London, UK</p>
      </td>
      <td align="center" valign="top">
        <img width="100" height="100" src="https://github.com/glennawatson.png?s=150">
        <br>
        <a href="https://github.com/glennawatson">Glenn Watson</a>
        <p>Melbourne, Australia</p>
      </td>
    </tr>
  </tbody>
</table>

## What does it do?

ReactiveUI.Binding.SourceGenerators is an incremental source generator that analyses your `WhenChanged`, `WhenChanging`,
`WhenAnyValue`, `WhenAny`, `WhenAnyObservable`, `BindOneWay`, `BindTwoWay`, `OneWayBind`, and `Bind` call sites at
compile time and emits optimised, strongly-typed observation and binding code. It eliminates:

- **Runtime expression-tree compilation** -- no `Expression<Func<T>>` evaluation at runtime
- **Reflection** -- all property access is generated as direct member access
- **System.Reactive dependency in the base package** -- generated code uses lightweight built-in observables

The result is binding code that is AOT-safe, trimming-safe, and significantly faster than the legacy ReactiveUI binding
engine.

## How does it work?

The generator runs two pipelines during compilation:

**Pipeline A (Type Detection)** scans your types for notification mechanisms (INotifyPropertyChanged, IReactiveObject,
WPF DependencyObject, WinUI DependencyObject, Apple KVO, WinForms Component, Android View) and registers per-type
observation factories via a `[ModuleInitializer]`.

**Pipeline B (Invocation Detection)** scans method invocations and extracts lambda property paths at compile time. Each
call site is identified by `[CallerFilePath]` + `[CallerLineNumber]`, and the generator emits a per-call-site optimised
method that is dispatched to at runtime via a generated lookup table.

```csharp
// You write:
var obs = vm.WhenChanged(x => x.Name);

// The generator emits a dispatch stub that captures caller info:
public static IObservable<TReturn> WhenChanged<TObj, TReturn>(
    this TObj obj, Expression<Func<TObj, TReturn>> property,
    [CallerFilePath] string callerFilePath = "",
    [CallerLineNumber] int callerLineNumber = 0) where TObj : class
{
    if (__GeneratedBindingDispatcher.TryGetWhenChanged(callerFilePath, callerLineNumber, obj, out var result))
        return (IObservable<TReturn>)result!;
    throw new InvalidOperationException("No generated binding found.");
}

// And a per-call-site method with direct property access:
private static IObservable<string> __WhenChanged_0(MyViewModel obj)
{
    return new PropertyObservable<string>(
        obj, "Name", static o => ((MyViewModel)o).Name, true);
}
```

## How do I install?

Install the `ReactiveUI.Binding` NuGet package. The source generator is automatically included.

```
dotnet add package ReactiveUI.Binding
```

If you need `IScheduler` overloads (e.g. `ObserveOn`), also install the adapter package:

```
dotnet add package ReactiveUI.Binding.Reactive
```

### Supported Frameworks

| Target         | Versions            |
|----------------|---------------------|
| .NET           | 8.0, 9.0, 10.0      |
| .NET Framework | 4.6.2, 4.7.2, 4.8.1 |
| NativeAOT      | .NET 10.0+          |

### Platform Packages

Platform-specific packages provide DependencyProperty observation and other platform integrations:

| Platform | Package                       |
|----------|-------------------------------|
| WPF      | `ReactiveUI.Binding.Wpf`      |
| WinForms | `ReactiveUI.Binding.WinForms` |
| MAUI     | `ReactiveUI.Binding.Maui`     |

## Supported APIs

| API                 | Description                                                                         |
|---------------------|-------------------------------------------------------------------------------------|
| `WhenChanged`       | Observe property changes (after value has changed)                                  |
| `WhenChanging`      | Observe property changes (before value changes, requires `INotifyPropertyChanging`) |
| `WhenAnyValue`      | ReactiveUI compatibility shim -- same semantics as `WhenChanged`                    |
| `WhenAny`           | Multi-property observation with selector                                            |
| `WhenAnyDynamic`    | Observe a chain given as an `Expression` built at run time (reflection, not AOT-safe) |
| `WhenAnyObservable` | Observe and switch between observable properties                                    |
| `BindOneWay`        | One-way binding from source to target                                               |
| `BindTwoWay`        | Two-way binding between source and target                                           |
| `OneWayBind`        | ReactiveUI compatibility shim for one-way binding                                   |
| `Bind`              | ReactiveUI compatibility shim for two-way binding                                   |
| `BindCommand`       | Bind a command property to a UI element                                             |
| `BindInteraction`   | Bind an interaction to a handler                                                    |

All APIs support single properties, deep property chains (e.g. `x => x.Address.City`), and multi-property observation (
up to 12 properties for `WhenAnyValue`/`WhenChanged`).

## Usage Examples

### Property Observation

```csharp
// Single property
IObservable<string> nameObs = vm.WhenChanged(x => x.Name);

// Deep chain -- re-subscribes when intermediate objects change
IObservable<string> cityObs = vm.WhenChanged(x => x.Address.City);

// Multiple properties with selector
IObservable<string> fullName = vm.WhenChanged(
    x => x.FirstName,
    x => x.LastName,
    (first, last) => $"{first} {last}");

// Before-change observation (requires INotifyPropertyChanging)
IObservable<string> nameChanging = vm.WhenChanging(x => x.Name);
```

### Observing a Chain Named at Run Time

`WhenAnyDynamic` takes the chain as a `System.Linq.Expressions.Expression` the caller built, rather than as a
lambda the compiler could read. Arities 1 to 12 are available, each with and without the distinct gate.

```csharp
// The chain is a value, so it can be assembled from a property name, a configuration entry, or a caller.
Expression chain = ((Expression<Func<MyViewModel, string>>)(x => x.Address.City)).Body;

IObservable<string?> cityObs = vm.WhenAnyDynamic(chain, static c => (string?)c.Value);

IObservable<string> fullName = vm.WhenAnyDynamic(
    firstNameChain,
    lastNameChain,
    static (first, last) => $"{first.Value} {last.Value}");
```

There is nothing for a generator to resolve in an expression built at run time, so the chain is walked by
reflection. Every overload carries `[RequiresUnreferencedCode]`, which makes this the one part of the observation
surface that is not trimming- or AOT-safe and reports each call site in a `PublishAot` build. Where the chain is
known at compile time, `WhenChanged` and `WhenAny` observe the same thing with no reflection.

### One-Way Binding

```csharp
// Bind source property to target property
IDisposable binding = vm.BindOneWay(view, x => x.Name, x => x.NameLabel);

// With converter
IDisposable binding = vm.BindOneWay(view,
    x => x.Age,
    x => x.AgeLabel,
    age => $"Age: {age}");
```

### Two-Way Binding

```csharp
// Bind source and target properties bidirectionally
IDisposable binding = vm.BindTwoWay(view, x => x.Name, x => x.NameTextBox);

// With converters
IDisposable binding = vm.BindTwoWay(view,
    x => x.Age,
    x => x.AgeTextBox,
    age => age.ToString(),
    text => int.TryParse(text, out var n) ? n : 0);
```

### ReactiveUI Compatibility

```csharp
// WhenAnyValue is a drop-in replacement for ReactiveUI.WhenAnyValue
IObservable<string> obs = vm.WhenAnyValue(x => x.Name);

// OneWayBind / Bind match ReactiveUI's binding API
IDisposable binding = view.OneWayBind(vm, x => x.Name, x => x.NameLabel);
IDisposable binding = view.Bind(vm, x => x.Name, x => x.NameTextBox);
```

### Scheduler Overloads

Scheduler overloads require the `ReactiveUI.Binding.Reactive` package:

```csharp
using ReactiveUI.Binding.Reactive;
using System.Reactive.Concurrency;

// Observe on a specific scheduler
IObservable<string> obs = vm.WhenChanged(x => x.Name, RxApp.MainThreadScheduler);

// Bind with scheduler
IDisposable binding = vm.BindOneWay(view, x => x.Name, x => x.NameLabel,
    scheduler: RxApp.MainThreadScheduler);
```

### View Locator

The source generator automatically detects classes implementing `IViewFor<T>` and generates an AOT-safe view dispatch
table. Views are resolved without reflection via a compile-time type-switch.

```csharp
// Implement IViewFor<T> and the generator registers the mapping automatically
public class LoginView : ReactiveUI.Binding.IViewFor<LoginViewModel>
{
    public LoginViewModel ViewModel { get; set; }
    object ReactiveUI.Binding.IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (LoginViewModel)value;
    }
}

// Resolve a view at runtime
IViewFor view = ViewLocator.Current.ResolveView(myViewModel);
```

#### View Attributes

| Attribute                       | Description                                                                                                                                            |
|---------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|
| `[ViewContract("name")]`        | Registers the view under a named contract, enabling multiple views per ViewModel. Pass the contract string to `ResolveView` to select a specific view. |
| `[SingleInstanceView]`          | Caches a singleton instance of the view instead of creating a new one per resolution. Not suitable for views reused multiple times in the visual tree. |
| `[ExcludeFromViewRegistration]` | Excludes the view from automatic source-generated registration (e.g. for base classes or test doubles).                                                |

```csharp
// Contract-based resolution: multiple views for one ViewModel
[ViewContract("compact")]
public class CompactDashboardView : IViewFor<DashboardViewModel> { /* ... */ }

public class FullDashboardView : IViewFor<DashboardViewModel> { /* ... */ }

// Resolve the compact view
var compactView = ViewLocator.Current.ResolveView(vm, "compact");

// Resolve the default view
var defaultView = ViewLocator.Current.ResolveView(vm);
```

#### Resolution Strategy

The generated dispatch follows a 3-tier resolution strategy per view:

1. **Service locator** -- checks `Splat.AppLocator.Current` for DI-registered views
2. **Direct construction** -- falls back to `new View()` if a parameterless constructor exists
3. **Singleton cache** -- for `[SingleInstanceView]`, caches and reuses a single instance (thread-safe via
   `Interlocked.CompareExchange`)

## Supported Notification Mechanisms

The source generator detects and generates optimised code for each platform's notification mechanism:

| Mechanism               | Interface / Base Type                           | WhenChanged | WhenChanging | Affinity |
|-------------------------|-------------------------------------------------|:-----------:|:------------:|:--------:|
| INotifyPropertyChanged  | `System.ComponentModel.INotifyPropertyChanged`  |     Yes     |      --      |    21    |
| INotifyPropertyChanging | `System.ComponentModel.INotifyPropertyChanging` |     --      |     Yes      |    21    |
| IReactiveObject         | `ReactiveUI.IReactiveObject`                    |     Yes     |     Yes      |    24    |
| WPF DependencyObject    | `System.Windows.DependencyObject`               |     Yes     |      --      |    20    |
| WinUI DependencyObject  | `Microsoft.UI.Xaml.DependencyObject`            |     Yes     |      --      |    22    |
| Apple KVO (NSObject)    | `Foundation.NSObject`                           |     Yes     |      --      |    25    |
| WinForms Component      | `System.ComponentModel.Component`               |     Yes     |      --      |    23    |
| Android View            | `Android.Views.View`                            |     Yes     |      --      |    19    |

Higher affinity values take priority when a type implements multiple mechanisms.

## Packages

| Package                               | Description                                                                  | NuGet                                                                                                                                                  |
|---------------------------------------|------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|
| `ReactiveUI.Binding`                  | Runtime library with lightweight observables. No System.Reactive dependency. | [![NuGet](https://img.shields.io/nuget/v/ReactiveUI.Binding.svg)](https://www.nuget.org/packages/ReactiveUI.Binding)                                   |
| `ReactiveUI.Binding.SourceGenerators` | Compatibility package; the generator ships inside the runtime packages.      | [![NuGet](https://img.shields.io/nuget/v/ReactiveUI.Binding.SourceGenerators.svg)](https://www.nuget.org/packages/ReactiveUI.Binding.SourceGenerators) |
| `ReactiveUI.Binding.Reactive`         | System.Reactive adapter for IScheduler overloads.                            | [![NuGet](https://img.shields.io/nuget/v/ReactiveUI.Binding.Reactive.svg)](https://www.nuget.org/packages/ReactiveUI.Binding.Reactive)                 |
| `ReactiveUI.Binding.Wpf`              | WPF DependencyProperty support.                                              | [![NuGet](https://img.shields.io/nuget/v/ReactiveUI.Binding.Wpf.svg)](https://www.nuget.org/packages/ReactiveUI.Binding.Wpf)                           |
| `ReactiveUI.Binding.WinForms`         | WinForms Component support.                                                  | [![NuGet](https://img.shields.io/nuget/v/ReactiveUI.Binding.WinForms.svg)](https://www.nuget.org/packages/ReactiveUI.Binding.WinForms)                 |
| `ReactiveUI.Binding.Maui`             | MAUI BindableProperty support.                                               | [![NuGet](https://img.shields.io/nuget/v/ReactiveUI.Binding.Maui.svg)](https://www.nuget.org/packages/ReactiveUI.Binding.Maui)                         |

## Rx Library Compatibility

The base `ReactiveUI.Binding` package has **no dependency on System.Reactive**. All generated code returns
`IObservable<T>` (the BCL interface), making it compatible with any Rx implementation.

| Library                      | Compatibility                                          | Notes                                                                                                                                                                                                                                   |
|------------------------------|--------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **System.Reactive**          | Full support via `ReactiveUI.Binding.Reactive` adapter | IScheduler overloads, ObserveOn                                                                                                                                                                                                         |
| **R3**                       | Compatible via `IObservable<T>`                        | Generated code returns `IObservable<T>` which R3 can consume via `.ToObservable()` conversion. R3 uses its own `Observable<T>` abstract class rather than `IObservable<T>`, so native R3 types are not used directly in generated code. |
| **Other Rx implementations** | Works out of the box                                   | Any library that consumes `IObservable<T>` is compatible                                                                                                                                                                                |

## Performance

### Source-Generated vs ReactiveUI Expression-Tree Engine

All benchmarks use 1,000 property changes per iteration. Measured on AMD Ryzen 7 5800X, .NET SDK 10.0.102.

#### Property Observation (WhenChanged vs WhenAnyValue)

**Source-Generated (this project):**

| Method            | Runtime        |    Mean | Allocated |
|-------------------|----------------|--------:|----------:|
| Single Property   | .NET 10.0      |  220 us |     63 KB |
| Deep Chain        | .NET 10.0      |  304 us |     64 KB |
| Two Properties    | .NET 10.0      |  315 us |     88 KB |
| First Observation | .NET 10.0      |  9.9 us |    0.9 KB |
| Single Property   | .NET 8.0       |  238 us |     63 KB |
| Deep Chain        | .NET 8.0       |  310 us |     64 KB |
| Two Properties    | .NET 8.0       |  328 us |     88 KB |
| First Observation | .NET 8.0       | 11.1 us |    0.9 KB |
| Single Property   | NativeAOT 10.0 |   36 us |     64 KB |
| Deep Chain        | NativeAOT 10.0 |   42 us |     65 KB |
| Two Properties    | NativeAOT 10.0 |   61 us |     89 KB |
| First Observation | NativeAOT 10.0 |  4.9 us |    1.1 KB |

**ReactiveUI Expression-Tree Engine (baseline):**

| Method            | Runtime   |     Mean | Allocated |
|-------------------|-----------|---------:|----------:|
| Single Property   | .NET 10.0 | 1,032 us |    391 KB |
| Deep Chain        | .NET 10.0 | 1,101 us |    395 KB |
| Two Properties    | .NET 10.0 | 2,216 us |    759 KB |
| First Observation | .NET 10.0 |  29.4 us |    4.9 KB |
| Single Property   | .NET 8.0  | 1,086 us |    388 KB |
| Deep Chain        | .NET 8.0  | 1,163 us |    391 KB |
| Two Properties    | .NET 8.0  | 2,225 us |    760 KB |
| First Observation | .NET 8.0  |  33.4 us |    4.9 KB |

**Summary:**

| Scenario                      |     Speedup | Allocation Reduction |
|-------------------------------|------------:|---------------------:|
| Single Property (.NET 10.0)   | 4.7x faster |            6.2x less |
| Deep Chain (.NET 10.0)        | 3.6x faster |            6.2x less |
| Two Properties (.NET 10.0)    | 7.0x faster |            8.6x less |
| First Observation (.NET 10.0) | 3.0x faster |            5.4x less |

The ReactiveUI expression-tree engine cannot run under NativeAOT due to its use of runtime reflection and expression
compilation. The source-generated engine runs under NativeAOT with an additional 6x speedup over JIT.

#### Binding (BindOneWay / BindTwoWay / Bind)

**Source-Generated:**

| Method        | Runtime        |    Mean | Allocated |
|---------------|----------------|--------:|----------:|
| BindOneWay    | .NET 10.0      |  286 us |     64 KB |
| BindTwoWay    | .NET 10.0      |  237 us |   87.8 KB |
| Bind          | .NET 10.0      |  124 us |   42.2 KB |
| First Binding | .NET 10.0      | 13.4 us |    1.3 KB |
| BindOneWay    | NativeAOT 10.0 |   47 us |     65 KB |
| BindTwoWay    | NativeAOT 10.0 |   58 us |   88.4 KB |
| Bind          | NativeAOT 10.0 |   25 us |   42.7 KB |

Each figure is one binding created and then driven through 1,000 property changes.

Watching a two-way binding costs almost nothing, because a subscriber shares the binding's own upstream
subscription rather than re-projecting the two observed sides:

| Method                  | Runtime   |    Mean | Allocated |
|-------------------------|-----------|--------:|----------:|
| Bind                    | .NET 10.0 |  124 us |   42.2 KB |
| Bind + observed changes | .NET 10.0 |  125 us |   42.3 KB |

**ReactiveUI Expression-Tree Engine:**

| Method           | Runtime   |     Mean | Allocated |
|------------------|-----------|---------:|----------:|
| OneWayBind       | .NET 10.0 | 2,543 us |    522 KB |
| Bind             | .NET 10.0 | 3,128 us |    746 KB |
| First OneWayBind | .NET 10.0 |  55.6 us |   12.5 KB |

**Summary:**

| Scenario                    |     Speedup | Allocation Reduction |
|-----------------------------|------------:|---------------------:|
| One-Way Binding (.NET 10.0) | 8.9x faster |            8.2x less |
| Two-Way Binding (.NET 10.0) | 8.1x faster |            8.5x less |
| First Binding (.NET 10.0)   | 4.1x faster |            9.6x less |

#### Chains Named at Run Time (WhenAnyDynamic)

| Method            | Runtime   |   Mean | Allocated |
|-------------------|-----------|-------:|----------:|
| Single Chain      | .NET 10.0 | 278 us |  102.7 KB |
| Two Chains        | .NET 10.0 | 595 us |  190.2 KB |
| Deep Chain        | .NET 10.0 | 354 us |  103.3 KB |
| First Observation | .NET 10.0 | 7.3 us |    1.1 KB |
| Single Chain      | .NET 8.0  | 347 us |  102.7 KB |
| Two Chains        | .NET 8.0  | 691 us |  190.1 KB |
| Deep Chain        | .NET 8.0  | 419 us |  103.3 KB |
| First Observation | .NET 8.0  | 7.9 us |    1.1 KB |

This is the one part of the surface where the two engines are level. Both walk the chain by reflection and
allocate the same objects doing it, so the numbers land on top of each other; the gain is in resolving the chain
at compile time instead:

| Single chain, .NET 10.0     |   Mean | Allocated |
|-----------------------------|-------:|----------:|
| Generated (`WhenChanged`)   | 175 us |   63.4 KB |
| `WhenAnyDynamic`            | 278 us |  102.7 KB |
| ReactiveUI `WhenAnyDynamic` | 299 us |  102.6 KB |

Most of the gap to the generated path is the `IObservedChange` handed to the selector - 40 bytes per
notification, which the signature has to produce, against a generated observation that emits the value itself.

Two chains fire twice the notifications of one, so their 190 KB is 95 bytes per notification against a single
chain's 103: combining a higher arity costs one subscription, not a per-notification charge. Every arity observes
each of its chains through the same walk and differs only in how many it combines.

## Diagnostics

The separate analyzer package reports the following diagnostics:

| ID          | Severity | Description                                                                                                                                                              |
|-------------|----------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| RXUIBIND001 | Info     | Expression must be an inline lambda for compile-time optimisation. Variable or method references fall back to runtime.                                                   |
| RXUIBIND002 | Warning  | Type has no observable properties and does not implement any observable notification mechanism.                                                                          |
| RXUIBIND003 | Warning  | Expression accesses a private or protected member which cannot be observed by a generated extension method.                                                              |
| RXUIBIND004 | Warning  | Type does not support before-change notifications (WhenChanging). WPF DependencyObjects, WinForms Components, and Android Views only support after-change notifications. |
| RXUIBIND005 | Info     | Source type implements INotifyDataErrorInfo; validation state propagation is not generated and requires runtime engine or manual ErrorsChanged subscription.             |
| RXUIBIND006 | Warning  | Expression contains an unsupported path segment (indexer, field, or method call). Only simple property access chains can be observed by the source generator.            |
| RXUIBIND007 | Warning  | BindCommand control has no bindable event. Specify the `toEvent` parameter explicitly.                                                                                   |
| RXUIBIND008 | Warning  | The property selected in a BindInteraction expression does not implement `IInteraction<TInput, TOutput>`.                                                                |
| RXUIBIND009 | Warning  | The generated binding dispatch is out of reach from this file, so the call falls back to the runtime stub.                                                               |
| RXUIBIND010 | Warning  | The observed path passes through a type that raises no notification, so it is read once and the observation stops following the path there.                              |

## Where this differs from ReactiveUI

Everything below is a deliberate difference from the reflection binding engine, and each one is here because
generating the equivalent would put reflection back on the path that exists to remove it. Nothing else in the
binding surface behaves differently.

### `TriggerUpdate` and `signalViewUpdate` are not offered

ReactiveUI's `Bind` accepts both: the first chooses which side wins the first write, the second replaces the
view's own change stream with one the caller supplies. Neither has a generated overload, so asking for one does
not compile rather than failing at run time.

A generated binding is resolved from the two lambdas alone. An overload taking a caller-supplied stream has
nothing to resolve at compile time and would have to hand the call to the runtime expression engine - which
carries `[RequiresUnreferencedCode]` and breaks a `PublishAot` build for every consumer of that call site.

The first write is instead decided the way ReactiveUI decides it by default: the view model side is applied
first, and the view's own first value is then weighed against what was just written and dropped when equal.

### A binding call made through a type parameter is not generated

A generated overload has to name the bound types, and a call made through a type parameter names none - the
type is only known once something closes it. Those call sites are left to the runtime stub, so a generic
binding helper compiles and throws when it runs rather than emitting an overload naming a type parameter,
which would fail the consumer's build outright.

### A silent link in a path is reported at compile time, not at run time

ReactiveUI logs a warning the first time it observes a property on a type that raises no notification.
RXUIBIND010 reports the same thing while the consumer is building, which is the only place a generator can
say it. The observation itself behaves the same either way: the value is read once and the path stops being
followed at that link.

## Architecture

```
src/
  ReactiveUI.Binding/                        Runtime library, lightweight observables, IObservable<T>
  ReactiveUI.Binding.SourceGenerators/       Incremental source generator (netstandard2.0)
  ReactiveUI.Binding.Analyzer/              Roslyn analyzer for RXUIBIND diagnostics
  ReactiveUI.Binding.Reactive/              System.Reactive adapter (IScheduler overloads)
  ReactiveUI.Binding.Wpf/                   WPF DependencyProperty integration
  ReactiveUI.Binding.WinForms/              WinForms Component integration
  ReactiveUI.Binding.Maui/                  MAUI BindableProperty integration
```

The generator and analyzer both target netstandard2.0 (Roslyn requirement). The runtime library targets .NET 8.0, 9.0,
10.0, and .NET Framework 4.6.2-4.8.1. Generated output is C# 7.3 compatible to support the widest range of consumer
projects.

A third pipeline scans for `IViewFor<T>` implementations and generates an AOT-safe view dispatch table (
`ViewDispatch.g.cs`) with type-switch resolution, singleton caching, and contract-based selection.

## Contribute

ReactiveUI.Binding.SourceGenerators is developed under an OSI-approved open source license, making it freely usable and
distributable, even for commercial use. We value the people who are involved in this project, and we'd love to have you
on board, especially if you are just getting started or have never contributed to open-source before.

So here's to you, lovely person who wants to join us -- this is how you can support us:

* [Responding to questions on GitHub Discussions](https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/discussions)
* [Passing on knowledge and teaching the next generation of developers](http://ericsink.com/entries/dont_use_rxui.html)
* Submitting documentation updates where you see fit or lacking.
* Making contributions to the code base.
