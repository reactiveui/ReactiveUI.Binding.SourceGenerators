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

This library binds properties without reflection. A source generator reads each binding call site while you
compile and writes the observation code for it. Nothing is looked up by name at run time, so a trimmed or
ahead-of-time published application keeps working.

## Table of Contents

- [What it does](#what-it-does)
- [How it works](#how-it-works)
- [How a call site reaches its generated code](#how-a-call-site-reaches-its-generated-code)
- [When nothing claims the call](#when-nothing-claims-the-call)
- [Installing](#installing)
- [Supported frameworks](#supported-frameworks)
- [Packages](#packages)
- [Supported APIs](#supported-apis)
- [Examples](#examples)
- [The view locator](#the-view-locator)
- [Notification mechanisms](#notification-mechanisms)
- [Rx library compatibility](#rx-library-compatibility)
- [Performance](#performance)
- [Diagnostics](#diagnostics)
- [Where this differs from ReactiveUI](#where-this-differs-from-reactiveui)
- [Layout](#layout)
- [Core team](#core-team)
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

## What it does

A property raises an event when it changes. Which event depends on the type that declares it.

The generator looks at that type while you compile, works out which event to listen to, and writes the code
that subscribes. Your call site then just subscribes to the chain.

ReactiveUI does the same job at run time instead. It compiles the expression tree, then finds each property by
name. That costs time on every binding, and a trimmer cannot see which members the reflection will ask for.

Doing it at compile time removes both problems. The emitted code names every type and every member, so
trimming and ahead-of-time publishing keep working. The base package also has no System.Reactive dependency,
because generated code returns `IObservable<T>` from the BCL.

## How it works

The generator needs two things from your code.

First, which event each type raises. A class raising `PropertyChanged` is listened to one way, a WPF
`DependencyObject` another, an `NSObject` another again. The generator records which mechanism reaches each
property.

Second, which properties each call site names. It reads the path out of the lambda and emits one method per
call site, subscribing to the events those properties raise. A chain such as `x => x.Address.City` subscribes
to each link, and re-subscribes further down when an intermediate object is replaced.

It also scans for `IViewFor<T>` and writes a type switch that resolves a view without reflection.

Here is what it emits for `vm.WhenChanged(x => x.Name)` on a class that raises `PropertyChanged`:

```csharp
private static global::System.IObservable<string> __WhenChanged_7FFFD2E8D6FC818E(MyViewModel obj)
{
    return global::ReactiveUI.Binding.Observables.PluginObservationSource.Choose<string>(
        obj,
        ((Expression<Func<MyViewModel, string>>)(__e => __e.Name)).Body,
        "Name",
        false,
        5,
        (object __o) => ((MyViewModel)__o).Name,
        new global::ReactiveUI.Binding.Observables.PropertyObservable<string>(
            obj, "Name", (INotifyPropertyChanged __o) => ((MyViewModel)__o).Name, true));
}
```

The last argument is the subscription the generator wrote. Everything it needs is fixed at compile time: the
declaring type, the property name and the getter.

`Choose` offers the link to any `ICreatesObservableForProperty` you registered, and takes yours when it scores
higher than the mechanism the generator picked. That is how a platform plugin still wins without costing any
reflection.

## How a call site reaches its generated code

Your compiler decides this, and it matters only when something goes wrong.

Roslyn 4.13 and newer intercept the call, which means the compiler runs the generated method in place of
yours. Nothing goes through name lookup, so it works from any file and any language version, including
`<LangVersion>7.3</LangVersion>` on .NET Framework 4.6.2.

Roslyn 4.8 to 4.12 emit an overload that competes for the call instead. It wins wherever
extension-method lookup finds it, and RXUIBIND009 tells you where it will not.

Both routes run the same method, so a binding behaves the same either way.

Set `ReactiveUIBindingUseInterceptors` to `false` to take the overloads on a compiler that could intercept.
Set `ReactiveUIBindingEmitGeneratedCodeMarkers` to `false` to drop the `// <auto-generated/>` header from
emitted files, which lets analyzer and compiler diagnostics inside them surface.

The generator and analyzer are packed once per compiler generation:

```
analyzers/dotnet/roslyn4.8/cs/    <- Roslyn 4.8 to 4.12
analyzers/dotnet/roslyn4.13/cs/   <- Roslyn 4.13 and newer
```

The .NET SDK picks the highest folder your compiler supports. A legacy non-SDK project is handed both, so the
package's targets remove the one you are not being served by. Loading the generator twice would emit every
dispatch file twice and fail your build.

## When nothing claims the call

Some call sites cannot be read while you compile. An expression held in a variable, one assembled at run
time, or a receiver typed as a type parameter all name no path the generator can see.

Those call sites get the `Unsafe` overload, which is ReactiveUI's reflection pipeline. It walks the chain at
run time and finds each property by name, so it is not guaranteed to survive trimming or ahead-of-time
publishing. Every other overload is.

Asking for the plain name where nothing was generated throws, and the message names the overload that
resolves it:

```csharp
Expression<Func<MyViewModel, string>> selector = x => x.Name;

vm.WhenChanged(selector);        // throws, and names WhenChangedUnsafe
vm.WhenChangedUnsafe(selector);  // walks the chain by reflection
```

Thirteen APIs have an `Unsafe` twin.

| Resolved while you compile | Resolved by reflection |
|----------------------------|------------------------|
| `WhenChanged` | `WhenChangedUnsafe` |
| `WhenChanging` | `WhenChangingUnsafe` |
| `WhenAnyValue` | `WhenAnyValueUnsafe` |
| `WhenAny` | `WhenAnyUnsafe` |
| `WhenAnyObservable` | `WhenAnyObservableUnsafe` |
| `BindOneWay` | `BindOneWayUnsafe` |
| `BindTwoWay` | `BindTwoWayUnsafe` |
| `OneWayBind` | `OneWayBindUnsafe` |
| `Bind` | `BindUnsafe` |
| `BindTo` | `BindToUnsafe` |
| `BindCommand` | `BindCommandUnsafe` |
| `BindInteraction` | `BindInteractionUnsafe` |
| `InvokeCommand` | `InvokeCommandUnsafe` |

Every `Unsafe` overload carries `[RequiresUnreferencedCode]`. The overloads a generated dispatch displaces
carry none. A `PublishTrimmed` or `PublishAot` build therefore reports the call sites that asked for
reflection and says nothing about the rest. RXUIBIND001, RXUIBIND006 and RXUIBIND009 name those call sites
while you build, before anything throws.

The scheduler overloads split the same way, and both halves live on `ReactiveSchedulerExtensions`.

Two members have no compile-time half, so their unsuffixed names carry the annotation themselves.

- `WhenAnyDynamic` takes the chain as an `Expression` you built. There is no lambda to read.
- `ViewLocator`'s object-typed `ResolveView` closes `IViewFor<>` over a runtime type, so it carries
  `[RequiresDynamicCode]`. The generic overload does not.

## Installing

```
dotnet add package ReactiveUI.Binding
```

The generator and the analyzer ship inside that package. There is nothing else to reference.

Install `ReactiveUI.Binding.Reactive` instead if your application speaks System.Reactive. It is the same
library compiled against `System.Reactive.Concurrency.IScheduler`, under the `ReactiveUI.Binding.Reactive.*`
namespaces. Reference one or the other, not both.

## Supported frameworks

| Target | Versions |
|--------|----------|
| .NET | 8.0, 9.0, 10.0, 11.0 |
| .NET Framework | 4.6.2, 4.7.2, 4.8.1 |

The WPF and WinForms packages target the three .NET Framework versions and the Windows heads of .NET 8 to 11.
The MAUI packages start at .NET 10 and add Android, iOS, macOS, Mac Catalyst and tvOS heads; the Apple heads
build only on Windows and macOS.

NativeAOT is supported on .NET 8 and later. The generated path is the only one that runs there at all.

## Packages

Ten packages ship. Each runtime package carries the generator and the analyzer; the platform packages inherit
them.

| Package | What it is |
|---------|------------|
| `ReactiveUI.Binding` | The runtime library. Lightweight observables, no System.Reactive dependency. |
| `ReactiveUI.Binding.Reactive` | The same library against System.Reactive's `IScheduler`. |
| `ReactiveUI.Binding.Wpf` | WPF dependency-property observation. |
| `ReactiveUI.Binding.Wpf.Reactive` | The same, for a System.Reactive application. |
| `ReactiveUI.Binding.WinForms` | WinForms component observation. |
| `ReactiveUI.Binding.WinForms.Reactive` | The same, for a System.Reactive application. |
| `ReactiveUI.Binding.Maui` | MAUI bindable-property observation. |
| `ReactiveUI.Binding.Maui.Reactive` | The same, for a System.Reactive application. |
| `ReactiveUI.Binding.SourceGenerators` | MSBuild props and targets only. A compatibility package. |
| `ReactiveUI.Binding.Analyzer` | The analyzer project. Its assets ship inside the runtime packages. |

## Supported APIs

| API | What it does | Properties per call |
|-----|--------------|--------------------:|
| `WhenChanged` | Observes a property after it changes. | 16 |
| `WhenChanging` | Observes a property before it changes. | 16 |
| `WhenAnyValue` | ReactiveUI's name for `WhenChanged`. | 16 |
| `WhenAny` | Observes properties and hands each change to a selector. | 12 |
| `WhenAnyObservable` | Observes properties that hold observables, and switches between them. | 12 |
| `WhenAnyDynamic` | Observes a chain you built as an `Expression`. | 12 |
| `BindOneWay` | Writes a source property to a target property. | 1 each side |
| `BindTwoWay` | Carries a property both ways. | 1 each side |
| `OneWayBind` | ReactiveUI's name for a one-way binding, written view first. | 1 each side |
| `Bind` | ReactiveUI's name for a two-way binding, written view first. | 1 each side |
| `BindTo` | Writes an observable's values to a target property. | 1 |
| `BindCommand` | Binds a command to a control's event. | 1 |
| `BindInteraction` | Registers a handler against an interaction a property holds. | 1 |
| `InvokeCommand` | Executes a command with each value an observable produces. | 1 |

Every one of them reads a single property, a deep chain such as `x => x.Address.City`, or several properties
at once. A deep chain re-subscribes when an intermediate object is replaced.

`BindOneWay`, `BindTwoWay`, `OneWayBind` and `Bind` also take a scheduler. No observation API does.

## Examples

### Observing a property

```csharp
// One property.
IObservable<string> name = vm.WhenChanged(x => x.Name);

// A chain. Replacing Address re-subscribes.
IObservable<string> city = vm.WhenChanged(x => x.Address.City);

// Several properties, combined by a selector.
IObservable<string> fullName = vm.WhenChanged(
    x => x.FirstName,
    x => x.LastName,
    (first, last) => $"{first} {last}");

// Before the change. The type has to raise PropertyChanging.
IObservable<string> before = vm.WhenChanging(x => x.Name);
```

### Binding

```csharp
// One way.
IDisposable binding = vm.BindOneWay(view, x => x.Name, x => x.NameLabel);

// One way, converting on the way through.
IDisposable converted = vm.BindOneWay(view, x => x.Age, x => x.AgeLabel, age => $"Age: {age}");

// Both ways.
IDisposable twoWay = vm.BindTwoWay(view, x => x.Name, x => x.NameTextBox);

// Both ways, with a converter per direction.
IDisposable pair = vm.BindTwoWay(
    view,
    x => x.Age,
    x => x.AgeTextBox,
    age => age.ToString(),
    text => int.TryParse(text, out var n) ? n : 0);

// On a scheduler.
IDisposable scheduled = vm.BindOneWay(
    view, x => x.Name, x => x.NameLabel, scheduler: RxApp.MainThreadScheduler);
```

`OneWayBind` and `Bind` are the same bindings written view first, for code moving across from ReactiveUI:

```csharp
IDisposable oneWay = view.OneWayBind(vm, x => x.Name, x => x.NameLabel);
IDisposable twoWay = view.Bind(vm, x => x.Name, x => x.NameTextBox);
```

### Running a command

```csharp
// The command lives on a view model property. Each value becomes the command parameter,
// and a value CanExecute refuses is dropped.
IDisposable invocation = searchText.InvokeCommand(vm, x => x.Search);

// The caller holds the command, so there is no property to observe.
IDisposable direct = searchText.InvokeCommand(vm.Search);
```

A `ReactiveCommand` is reached like any other `ICommand`. The parameter arrives as `object` rather than the
command's declared input type.

### Observing a chain built at run time

`WhenAnyDynamic` takes the chain as an `Expression` rather than a lambda, so it can be assembled from a
property name or a configuration entry. Arities 1 to 12 are available, each with and without a distinct gate.

```csharp
Expression chain = ((Expression<Func<MyViewModel, string>>)(x => x.Address.City)).Body;

IObservable<string?> city = vm.WhenAnyDynamic(chain, static c => (string?)c.Value);
```

There is nothing for a generator to read in an expression built at run time, so the chain is walked by
reflection. Every overload carries `[RequiresUnreferencedCode]` and each call site is reported in a
`PublishAot` build. Where the chain is known while you compile, `WhenChanged` and `WhenAny` observe the same
thing with no reflection at all.

## The view locator

Implement `IViewFor<T>` and the generator registers the mapping for you.

```csharp
public class LoginView : IViewFor<LoginViewModel>
{
    public LoginViewModel ViewModel { get; set; }

    object IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (LoginViewModel)value;
    }
}

IViewFor? view = ViewLocator.GetCurrent().ResolveView(myViewModel);
```

Three attributes change what is registered.

| Attribute | Effect |
|-----------|--------|
| `[ViewContract("name")]` | Registers the view under a contract, so one view model can have several views. Pass the contract to `ResolveView`. |
| `[SingleInstanceView]` | Caches one instance instead of constructing a view per resolution. Unsuitable for a view used more than once in the tree. |
| `[ExcludeFromViewRegistration]` | Leaves the view out of the generated registration. |

```csharp
[ViewContract("compact")]
public class CompactDashboardView : IViewFor<DashboardViewModel> { }

public class FullDashboardView : IViewFor<DashboardViewModel> { }

var compact = ViewLocator.GetCurrent().ResolveView(vm, "compact");
var full = ViewLocator.GetCurrent().ResolveView(vm);
```

A resolution is tried in three places, in order.

1. The generated dispatch, which is a type switch with no reflection in it.
2. A mapping you added at run time with `Map<TViewModel, TView>()`.
3. The service locator, `Splat.AppLocator.Current`.

Inside the generated dispatch, a contract is matched before the default view. The view then comes from the
service locator, and failing that from a constructor call, or from a cache when the view is marked
`[SingleInstanceView]`. The cache is filled with `Interlocked.CompareExchange`, so two threads resolving at
once share one instance.

Resolve through the generic overload where you can. The object-typed overload closes `IViewFor<>` over a
runtime type, so it carries `[RequiresDynamicCode]` and is not safe to publish ahead of time.

## Notification mechanisms

A type advertises a mechanism. A property either participates in it or does not, and every link of a chain is
declared by its own type. So the generator picks a mechanism per property rather than per class.

| Mechanism | Keyed on | Before-change | Affinity |
|-----------|----------|---------------|---------:|
| Apple KVO | `Foundation.NSObject` | yes | 15 |
| IReactiveObject | `ReactiveUI.IReactiveObject` | yes | 10 |
| WinForms component | `System.ComponentModel.Component` | no | 8 |
| WinUI dependency object | `Microsoft.UI.Xaml.DependencyObject` | no | 6 |
| INotifyPropertyChanged | `System.ComponentModel.INotifyPropertyChanged` | yes | 5 |
| Android view | `Android.Views.View` | no | 5 |
| WPF dependency object | `System.Windows.DependencyObject` | no | 4 |

The highest affinity that can reach the property wins. A dependency object's plain CLR property, or a
component with no matching `Changed` event, falls through to the next mechanism. `INotifyPropertyChanged` and
`Android.Views.View` share an affinity, and that tie is settled by the order the plugins are registered in,
with `INotifyPropertyChanged` first.

Before-change observation needs the type to raise `PropertyChanging`. Where it cannot, `WhenChanging` reads
the value once and then stays silent, and RXUIBIND004 says so while you build. A WPF dependency property is
the exception among the after-change mechanisms: it keeps a live subscription and delivers each new value.

A plugin you register at run time outranks the generated observation when it scores higher. A tie goes to the
generated code.

## Rx library compatibility

`ReactiveUI.Binding` does not depend on System.Reactive. Generated code returns `IObservable<T>` from the
BCL, so any Rx implementation can consume it.

| Library | How it works |
|---------|--------------|
| System.Reactive | Use `ReactiveUI.Binding.Reactive`, which types its schedulers as `IScheduler`. |
| R3 | R3 exposes its own `Observable<T>` class, so convert with `.ToObservable()`. |
| Anything else | Any library that consumes `IObservable<T>` works as it is. |

## Performance

Removing the expression tree and the name lookups makes a binding several times faster and allocates several
times less than the reflection engine, and it is the only path that runs under NativeAOT at all.

The suite that measures this, what each benchmark covers, and how to run it are described in
[src/benchmarks/README.md](src/benchmarks/README.md). Figures are deliberately not published here, because a
number written into a document goes stale without anyone noticing.

## Diagnostics

The analyzer ships inside the runtime packages and reports these.

| ID | Severity | What it means |
|----|----------|---------------|
| RXUIBIND001 | Info | The expression is not an inline lambda, so nothing is generated. Name the `Unsafe` overload or the call will throw. |
| RXUIBIND002 | Warning | The type has no observable property and raises no notification. |
| RXUIBIND003 | Warning | The expression reads a private or protected member, which a generated extension method cannot. |
| RXUIBIND004 | Warning | The type raises no before-change notification, so `WhenChanging` reads the value once and then stays silent. |
| RXUIBIND005 | Info | The source implements `INotifyDataErrorInfo`. Validation state is not generated. |
| RXUIBIND006 | Warning | The path contains an indexer, a field or a method call. Only property access is generated. |
| RXUIBIND007 | Warning | The control named by `BindCommand` has no default bindable event. Pass `toEvent`. |
| RXUIBIND008 | Warning | The property named by `BindInteraction` does not implement `IInteraction<TInput, TOutput>`. |
| RXUIBIND009 | Warning | The generated dispatch is out of reach from this file, so the call throws. Not reported where an interceptor claims the call. |
| RXUIBIND010 | Warning | The path passes through a type that raises no notification, so it is read once and followed no further. |
| RXUIBIND011 | Warning | The call resolved to ReactiveUI's own mixin, so nothing is generated and it takes the runtime engine. |

The package's targets report one error of their own.

| ID | What it means |
|----|---------------|
| RXUIBIND100 | The compiler is older than Roslyn 4.8, so no generator would load. Upgrade to Visual Studio 2022 17.8 or the .NET 8.0.100 SDK. |

## Where this differs from ReactiveUI

Each difference below is deliberate. Generating the equivalent would put reflection back on the path that
exists to remove it. Nothing else in the binding surface behaves differently.

### `TriggerUpdate` and `signalViewUpdate` are not offered

ReactiveUI's `Bind` accepts both. The first chooses which side wins the first write. The second replaces the
view's change stream with one you supply.

Neither has a generated overload, so asking for one does not compile. A generated binding is resolved from
the two lambdas alone, and an overload taking a caller-supplied stream has nothing to resolve while you
compile. It would have to hand the call to the runtime engine, which carries `[RequiresUnreferencedCode]` and
breaks a `PublishAot` build for everyone who calls it.

The first write follows ReactiveUI's default. The view model side is applied first. The view's own first value
is then weighed against what was just written, and dropped when the two are equal.

### A binding made through a type parameter is not generated

A generated overload has to name the bound types. A call made through a type parameter names none, because
the type is only known once something closes it. Those call sites are left to the stub, so a generic binding
helper compiles and throws when it runs. Emitting an overload that named a type parameter would fail your
build outright. Call the `Unsafe` twin from the helper to bind it by reflection, which also marks the helper
as the reflection boundary it is.

### A silent link is reported while you build

ReactiveUI logs a warning the first time it observes a property on a type that raises no notification.
RXUIBIND010 reports the same thing while you compile, which is the only place a generator can say it. The
observation behaves the same either way: the value is read once, and the path is followed no further.

### A write that faults behaves identically

A write that throws is logged against the bound expression, and rethrown as a `TargetInvocationException`
only when it carries an inner exception. This is parity, not divergence, and it is not optional. A setter
that throws on the notifying thread has no caller stack to surface on, so swallowing it would lose the
failure entirely.

## Layout

```
src/
  ReactiveUI.Binding/                     Runtime package, lightweight observables
  ReactiveUI.Binding.Reactive/            Runtime package, System.Reactive schedulers
  ReactiveUI.Binding.Shared/              The runtime source, compiled by both packages above
  ReactiveUI.Binding.SourceGenerators/    The generator, and the shipped props and targets
  ReactiveUI.Binding.Analyzer/            The RXUIBIND analyzers
  ReactiveUI.Binding.*.Roslyn413/         The same generator and analyzer against Roslyn 4.13
  ReactiveUI.Binding.Wpf*/                WPF integration, one package per runtime flavour
  ReactiveUI.Binding.WinForms*/           WinForms integration, one package per runtime flavour
  ReactiveUI.Binding.Maui*/               MAUI integration, one package per runtime flavour
  benchmarks/                             BenchmarkDotNet projects
  tests/                                  Test projects and the shared scenario sources
```

A `*.Shared` folder is source rather than a project. Each leaf compiles its own copy, which is how one body
of code serves both runtime flavours: `ReactiveShim.props` keys on the `.Reactive` suffix, defines
`REACTIVE_SHIM`, and aliases the scheduler type.

The generator and the analyzer target netstandard2.0, because Roslyn requires it. Generated output is C# 7.3
compatible, so the oldest supported consumer can compile it.

## Contribute

ReactiveUI.Binding.SourceGenerators is developed under an OSI-approved open source license, making it freely
usable and distributable, even for commercial use. We value the people who are involved in this project, and
we'd love to have you on board, especially if you are just getting started or have never contributed to
open-source before.

So here's to you, lovely person who wants to join us -- this is how you can support us:

* [Responding to questions on GitHub Discussions](https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/discussions)
* [Passing on knowledge and teaching the next generation of developers](http://ericsink.com/entries/dont_use_rxui.html)
* Submitting documentation updates where you see fit or lacking.
* Making contributions to the code base.
