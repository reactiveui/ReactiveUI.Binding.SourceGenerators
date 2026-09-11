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

You have a property. When it changes, something else needs to know: a label, a validation rule, another
property. This library lets you say that in one line, and writes the wiring for you while you compile.

> [!NOTE]
> This is the binding engine, not an introduction to reactive programming. The
> [WhenAny handbook](https://www.reactiveui.net/documentation/handbook/when-any/) teaches property
> observation and the [data binding guide](https://www.reactiveui.net/documentation/handbook/data-binding/)
> covers the binding verbs in depth. What follows gets you running, then explains what is specific to this
> engine.

## Table of Contents

- [The problem it solves](#the-problem-it-solves)
- [Your first binding](#your-first-binding)
- [How a property reports a change](#how-a-property-reports-a-change)
- [A chain is decided one link at a time](#a-chain-is-decided-one-link-at-a-time)
- [What the generator writes](#what-the-generator-writes)
- [How a call site reaches its generated code](#how-a-call-site-reaches-its-generated-code)
- [When nothing claims the call](#when-nothing-claims-the-call)
- [Installing](#installing)
- [Supported frameworks](#supported-frameworks)
- [Packages](#packages)
- [Supported APIs](#supported-apis)
- [Examples](#examples)
- [The view locator](#the-view-locator)
- [Which mechanism wins](#which-mechanism-wins)
- [Which thread a binding writes on](#which-thread-a-binding-writes-on)
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

## The problem it solves

C# already tells you when a property changes. A class raises `PropertyChanged`, you attach a handler, you
check which property the handler was told about, and you read the new value.

That is fine for one property. It goes badly as soon as you want a path through two of them:

```csharp
// Tell me when the city changes.
viewModel.PropertyChanged += (sender, args) =>
{
    if (args.PropertyName != nameof(viewModel.Address))
    {
        return;
    }

    // Address was replaced. Detach from the old Address, attach to the new one,
    // and start watching its City. Then undo all of it when you are finished.
};
```

You write the path instead:

```csharp
IObservable<string> city = viewModel.WhenChanged(x => x.Address.City);
```

That attaches to `Address` and to `City`, re-attaches further down when `Address` is replaced, and detaches
everything when you dispose the subscription.

The lambda is there to name the path. The generator reads the properties out of it, `Address` and then `City`,
and emits the code that fetches each one and attaches to whichever event reports it changing.

## Your first binding

**1. Install the package.**

```
dotnet add package ReactiveUI.Binding
```

**2. Raise `PropertyChanged` from your view model.** Any class that does this can be observed. There is no
base class to inherit.

```csharp
public class PersonViewModel : INotifyPropertyChanged
{
    private string _name = "";

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }
}
```

**3. Observe a property.** You get an `IObservable<T>`, which hands you the current value and then every
later one.

```csharp
var vm = new PersonViewModel { Name = "Ada" };

IDisposable subscription = vm.WhenChanged(x => x.Name)
    .Subscribe(name => Console.WriteLine(name));   // prints Ada

vm.Name = "Grace";                                // prints Grace
```

**4. Or bind it straight to a control.** This writes `Name` into the label and keeps writing it.

```csharp
IDisposable binding = vm.BindOneWay(view, x => x.Name, v => v.NameLabel.Text);
```

**5. Dispose when you are done.** Both calls return an `IDisposable`. Disposing detaches every handler the
binding attached.

> [!TIP]
> Keep your subscriptions in a `CompositeDisposable` and dispose that when the view goes away. A subscription
> you never dispose keeps the view model alive.

That is the whole surface you need to start. `WhenChanging`, `BindTwoWay`, `BindCommand` and the rest follow
the same shape.

## How a property reports a change

`PropertyChanged` is one way for a property to report a change. It is not the only one. A WPF control does
not raise it for `TextBox.Text`, and an iOS view does not raise it at all.

So the first thing the generator works out is which mechanism the declaring type offers. Each one is a
different event, attached a different way.

| Mechanism | What it is | How it is observed | Before the change |
|-----------|------------|--------------------|-------------------|
| `INotifyPropertyChanged` | The BCL interface. One event for the whole object, naming the property that changed. | Attach to `PropertyChanged`, keep the events naming your property, read the getter. | no |
| `INotifyPropertyChanging` | Its counterpart, raised before the value is replaced. | Attach to `PropertyChanging`, same shape. This is what `WhenChanging` needs. | yes |
| `IReactiveObject` | ReactiveUI's interface. Raises both of the above. | As above, and it gets both halves for free. | yes |
| WPF dependency property | A `TextBox.Text` is a `DependencyProperty`, not a CLR property, and raises no `PropertyChanged`. | `DependencyPropertyDescriptor.FromProperty(...)` gives a descriptor, then `AddValueChanged`. | see below |
| WinUI and MAUI bindable property | The same idea on WinUI and MAUI. | `RegisterPropertyChangedCallback`, released with the token it hands back. | no |
| WinForms component | WinForms has no single event. It has one per property, named by convention. | Find the `{PropertyName}Changed` event and attach to it. | no |
| Apple KVO | Key-value observing. How an `NSObject` reports a change on Apple platforms. | `NSObject.AddObserver` with `NSKeyValueObservingOptions`. | yes |
| Android view | An Android widget raises its own event, such as `TextView.TextChanged`. | Attach to that widget's event for that property. | no |

> [!NOTE]
> A type can offer more than one. A `ReactiveObject` in a WPF window implements `INotifyPropertyChanged` and
> may also carry dependency properties. The generator takes the most specific mechanism that can actually
> reach the property you named, so a plain CLR property on a `DependencyObject` still falls back to
> `PropertyChanged`.

Before-change observation needs the type to raise something before the value is replaced. Where it cannot,
`WhenChanging` reads the value once and then stays silent, and RXUIBIND004 tells you so while you build. A
WPF dependency property is the odd one out: it keeps a live subscription and delivers each new value.

## A chain is decided one link at a time

`x => x.Address.City` is two properties, and they need not use the same mechanism. `Address` might be a plain
property on a view model that raises `PropertyChanged`, while `City` sits on a WPF control.

The generator resolves each link separately, while you compile, and writes the right attach for each.

```csharp
// Address: INotifyPropertyChanged on the view model.
// City:    a dependency property on the control it holds.
vm.WhenChanged(x => x.Address.City);
```

When `Address` is replaced, the subscription below it is torn down and rebuilt against the new object. That
is the part you would otherwise hand-write, and the part most easily got wrong.

> [!WARNING]
> If a link in the path raises nothing at all, the value is read once and the path is followed no further.
> That is silent at run time, so the analyzer reports it as RXUIBIND010 while you build.

## What the generator writes

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

The last argument is the subscription it chose. Everything that subscription needs is fixed at compile time:
the declaring type, the property name, and a getter that is a direct call rather than a lookup.

`Choose` offers the link to any `ICreatesObservableForProperty` you registered yourself, and takes yours when
it scores higher than the mechanism the generator picked. That is how a platform plugin still wins, without
costing any reflection.

ReactiveUI does this same job at run time instead. It compiles the lambda into a delegate and then finds each
property by name. That costs time on every binding, and a trimmer cannot see which members the reflection
will ask for, so it may remove them.

> [!IMPORTANT]
> This is the reason the library exists. Doing the work at compile time is what makes a binding survive
> `PublishTrimmed` and `PublishAot`, because the emitted code names every type and member it touches.

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

| Target | Versions | Microsoft support ends |
|--------|----------|------------------------|
| .NET | 10.0, 11.0 | November 2028 for .NET 10 |
| .NET | 8.0, 9.0 | 10 November 2026 |
| .NET Framework | 4.7.2, 4.8.1 | tied to the Windows version |
| .NET Framework | 4.6.2 | 12 January 2027 |

> [!WARNING]
> .NET 8 and .NET 9 both leave Microsoft support on 10 November 2026, and this library drops them at the same
> time. .NET Framework 4.6.2 leaves support on 12 January 2027 and will be dropped then. Move to .NET 10 or
> later, or to .NET Framework 4.7.2 or later, before those dates. See the
> [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) and the
> [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework).

The WPF and WinForms packages target the .NET Framework versions and the Windows heads of .NET 8 to 11. The
MAUI packages start at .NET 10 and add Android, iOS, macOS, Mac Catalyst and tvOS heads; the Apple heads
build only on Windows and macOS.

NativeAOT works on .NET 8 and later. The generated path is the only one that runs there at all, because the
reflection engine compiles expressions at run time.

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

## Which mechanism wins

A type can offer several mechanisms, so each one carries a score. The highest score that can actually reach
the property you named wins.

| Mechanism | Type it keys on | Score |
|-----------|-----------------|------:|
| Apple KVO | `Foundation.NSObject` | 15 |
| IReactiveObject | `ReactiveUI.IReactiveObject` | 10 |
| WinForms component | `System.ComponentModel.Component` | 8 |
| WinUI bindable property | `Microsoft.UI.Xaml.DependencyObject` | 6 |
| INotifyPropertyChanged | `System.ComponentModel.INotifyPropertyChanged` | 5 |
| Android view | `Android.Views.View` | 5 |
| WPF dependency property | `System.Windows.DependencyObject` | 4 |

"Can reach" is the important half. A dependency object's plain CLR property is not a dependency property, and
a component with no `{PropertyName}Changed` event has nothing to attach to, so both fall through to the next
mechanism down. `INotifyPropertyChanged` and `Android.Views.View` share a score, and that tie goes to
`INotifyPropertyChanged`.

An `ICreatesObservableForProperty` you register yourself is scored against the same scale, and takes the link
when it scores higher. A tie goes to the generated code.

## Which thread a binding writes on

A view model raises its change notification on whichever thread did the work. A UI framework lets you touch a
view only from the thread that owns it. A binding therefore moves the write for you, so an update from a
background task lands where the view can take it.

Which thread that is belongs to the object being written, not to the process, so the platform package asks the
object.

| Package | What it asks |
|---------|--------------|
| `ReactiveUI.Binding.Wpf` | The dispatcher the `DependencyObject` was created on. |
| `ReactiveUI.Binding.WinForms` | The `Control`, which posts through its own window handle. |
| `ReactiveUI.Binding.Maui` | The `IDispatcher` the `BindableObject` carries. |

That distinction matters as soon as an application has more than one UI thread. WPF allows several, each owning
its own windows, and a write sent to the wrong one throws exactly as an unmarshalled write does.

A write that is already on the owning thread is applied inline, so setting a property on the UI thread and
reading the control back on the next line behaves as it reads. Only a write from another thread waits for a
turn of the message loop.

> [!TIP]
> Naming a scheduler on the binding wins outright - `vm.BindOneWay(view, x => x.Name, x => x.NameLabel,
> scheduler: someScheduler)`. Use it when you want the write somewhere specific.

An application can name one thread for everything else with
`BindingSchedulers.UseSynchronizationContext(context)`. It is consulted only for targets no platform package
claims. Where neither answers - a console host, a test, a platform with no thread affinity - writes are
delivered inline and cost nothing.

## Rx library compatibility

The observables and operators come from **ReactiveUI.Primitives**. That is the library this one is built on,
and it is the only Rx dependency the lean package has.

System.Reactive is reached through Primitives' own shim, **ReactiveUI.Primitives.Reactive**. Neither package
here references System.Reactive directly.

| Package | Depends on | Scheduler type |
|---------|------------|----------------|
| `ReactiveUI.Binding` | `ReactiveUI.Primitives` | `ISequencer` |
| `ReactiveUI.Binding.Reactive` | `ReactiveUI.Primitives.Reactive` | `IScheduler` |

Everything a binding hands back is an `IObservable<T>` from the BCL, so a consumer is not tied to either.

| Library | How it works |
|---------|--------------|
| ReactiveUI.Primitives | The default. Reference `ReactiveUI.Binding`. |
| System.Reactive | Reference `ReactiveUI.Binding.Reactive`, which takes it through the Primitives shim. |
| R3 | R3 exposes its own `Observable<T>` class, so convert with `.ToObservable()`. |
| Anything else | Any library that consumes `IObservable<T>` works as it is. |

> [!NOTE]
> Reference one runtime package or the other, never both. They share no type names, so referencing both puts
> two copies of every binding API in scope.

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
