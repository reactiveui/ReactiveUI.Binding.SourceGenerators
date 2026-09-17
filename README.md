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

You have a property. When it changes, something else needs to know. That might be a label, a validation rule
or another property. This library lets you say that in one line. It writes the code for you when you build.

> [!NOTE]
> This page covers this binding engine. It does not teach reactive programming. The
> [WhenAny handbook](https://www.reactiveui.net/documentation/handbook/when-any/) teaches property
> observation. The [data binding guide](https://www.reactiveui.net/documentation/handbook/data-binding/)
> covers the binding methods in depth.

## Table of Contents

- [The problem it solves](#the-problem-it-solves)
- [Your first binding](#your-first-binding)
- [How a property reports a change](#how-a-property-reports-a-change)
- [The generator checks each property in a path](#the-generator-checks-each-property-in-a-path)
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

C# already tells you when a property changes. A class raises `PropertyChanged`. You attach a handler, check
which property changed, and read the new value.

That works for one property. It gets hard when you follow a path through two properties:

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

With this library, you write the path as a lambda instead:

```csharp
IObservable<string> city = viewModel.WhenChanged(x => x.Address.City);
```

This attaches to `Address` and to `City`. When `Address` is replaced, it moves to the new `Address`. When you
dispose the subscription, it detaches from everything.

The lambda only names the path. A source generator is a compiler add-on that writes C# code while your project
builds. This library's generator reads the properties in the lambda, `Address` and then `City`. It writes code
that reads each one and attaches to the event that reports its change.

## Your first binding

**1. Install the package.**

```
dotnet add package ReactiveUI.Binding
```

**2. Raise `PropertyChanged` from your view model.** Any class that does this can be observed. You do not need
a base class.

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

**3. Observe a property.** You get an `IObservable<T>`. It gives you the current value, then every later value.

```csharp
var vm = new PersonViewModel { Name = "Ada" };

IDisposable subscription = vm.WhenChanged(x => x.Name)
    .Subscribe(name => Console.WriteLine(name));   // prints Ada

vm.Name = "Grace";                                // prints Grace
```

**4. Or bind it straight to a control.** This writes `Name` into the label now, and again on every change.

```csharp
IDisposable binding = vm.BindOneWay(view, x => x.Name, v => v.NameLabel.Text);
```

**5. Dispose when you are done.** Both calls return an `IDisposable`. Disposing it detaches every handler the
binding attached.

> [!TIP]
> Keep your subscriptions in a `CompositeDisposable`. Dispose it when the view goes away. A subscription you
> never dispose keeps the view model alive.

`WhenChanging`, `BindTwoWay`, `BindCommand` and the other methods work the same way.

## How a property reports a change

`PropertyChanged` is one way for a property to report a change. There are others. A WPF `TextBox.Text` does
not raise `PropertyChanged`. An iOS view does not raise it at all.

Each way of reporting a change is a mechanism. The generator first finds the mechanisms a type offers. Each
mechanism uses a different event, and attaches to it a different way.

| Mechanism | What it is | How it is observed | Before the change |
|-----------|------------|--------------------|-------------------|
| `INotifyPropertyChanged` | The .NET interface. It has one event for the whole object. The event names the property that changed. | Attach to `PropertyChanged`, keep the events for your property, and read the getter. | no |
| `INotifyPropertyChanging` | The matching interface, raised before the value is replaced. | Attach to `PropertyChanging` the same way. `WhenChanging` needs this. | yes |
| `IReactiveObject` | ReactiveUI's interface. It raises both events above. | As above, with both events. | yes |
| WPF dependency property | `TextBox.Text` is a `DependencyProperty`, not a plain C# property. It raises no `PropertyChanged`. | Get a descriptor from `DependencyPropertyDescriptor.FromProperty(...)`, then call `AddValueChanged`. | see below |
| WinUI and MAUI bindable property | The same idea on WinUI and MAUI. | Call `RegisterPropertyChangedCallback`. Release it with the token it returns. | no |
| WinForms component | WinForms has one event per property, named by convention. | Find the `{PropertyName}Changed` event and attach to it. | no |
| Apple KVO | Key-value observing. An `NSObject` reports changes this way on Apple platforms. | Call `NSObject.AddObserver` with `NSKeyValueObservingOptions`. | yes |
| Android view | An Android widget raises its own event, such as `TextView.TextChanged`. | Attach to that widget's event for the property. | no |

> [!NOTE]
> A type can offer more than one mechanism. A `ReactiveObject` in a WPF window implements
> `INotifyPropertyChanged`. It may also have dependency properties. The generator uses the most specific
> mechanism that can reach the property you named. So a plain C# property on a `DependencyObject` still uses
> `PropertyChanged`. [Which mechanism wins](#which-mechanism-wins) lists the order.

Observing before a change needs the type to raise an event before the value is replaced. Some types do not. For
those, `WhenChanging` reads the value once and then stays silent. RXUIBIND004 warns you about this when you
build. A WPF dependency property is the exception. It keeps a live subscription and delivers each new value.

## The generator checks each property in a path

`x => x.Address.City` names two properties. They can use different mechanisms. `Address` might be a plain
property on a view model that raises `PropertyChanged`. `City` might be a dependency property on a WPF control.

The generator checks each property on its own when you build. It writes the code to attach to each one.

```csharp
// Address: INotifyPropertyChanged on the view model.
// City:    a dependency property on the control it holds.
vm.WhenChanged(x => x.Address.City);
```

When `Address` is replaced, the subscription detaches from the old `Address` and attaches to the new one. You
would otherwise write this part by hand, and it is easy to get wrong.

> [!WARNING]
> If a property in the path raises no change event, its value is read once. The path is followed no further.
> Nothing tells you when the app runs. So the analyzer reports RXUIBIND010 when you build.

## What the generator writes

Here is the code it writes for `vm.WhenChanged(x => x.Name)` on a class that raises `PropertyChanged`:

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

The last argument is the subscription the generator chose. Everything it needs is fixed when you build: the
declaring type, the property name, and a getter. The getter is a direct call, not a lookup by name.

`Choose` also checks for an `ICreatesObservableForProperty` you registered yourself. It uses yours when yours
scores higher. See [Which mechanism wins](#which-mechanism-wins).

> [!IMPORTANT]
> The generated code names every type and member it touches. So a trimmer keeps them, and a binding keeps
> working with `PublishTrimmed` and `PublishAot`.

## How a call site reaches its generated code

A call site is a line where you call a method such as `WhenChanged`. How a call site reaches its generated code
depends on the C# compiler that builds your project.

### Which compiler you have

You do not choose the compiler directly. It comes with your build tools.

- Building in Visual Studio, or with Visual Studio's `msbuild`, uses the compiler that ships with that Visual
  Studio version.
- Building with `dotnet build` uses the compiler that ships with that .NET SDK version.

| Your build tools | How a call reaches the generated code |
|------------------|---------------------------------------|
| Visual Studio 2022 17.13 or later, Visual Studio 2026, or .NET SDK 9.0.200 or later | Interception |
| Visual Studio 2022 17.8 to 17.12, or .NET SDK 8.0.100 to 9.0.1xx | A generated overload |
| Anything older | Not supported. The build fails with RXUIBIND100. |

Microsoft's [Roslyn version table](https://learn.microsoft.com/en-us/visualstudio/extensibility/roslyn-version-support)
lists the compiler in each Visual Studio version.

**Interception.** The compiler replaces your call with a call to the generated method. This works from any file
and any C# language version.

**A generated overload.** The generator adds an overload that has to win C#'s normal method lookup. RXUIBIND009
tells you where it cannot.

Both ways run the same generated method. A binding behaves the same either way.

### .NET Framework projects

A .NET Framework project gets the same features as a .NET project. It needs the SDK-style project format and new
enough build tools. An SDK-style project file names the SDK on its first line and sets a target framework:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
  </PropertyGroup>
</Project>
```

Build that project with Visual Studio 2022 17.13 or later, or with `dotnet build` on .NET SDK 9.0.200 or later.
It then uses interception, even at the C# 7.3 language version .NET Framework projects default to.

An old-style project file has no `Sdk` attribute and lists its source files one by one. It still works. It uses the
compiler from the Visual Studio that builds it.

### Build properties

Two build properties change this.

- Set `ReactiveUIBindingUseInterceptors` to `false` to use the overloads on a compiler that can intercept.
- Set `ReactiveUIBindingEmitGeneratedCodeMarkers` to `false` to drop the `// <auto-generated/>` header from
  generated files. Analyzer and compiler warnings inside those files then show up.

The package holds one copy of the generator and the analyzer per compiler generation:

```
analyzers/dotnet/roslyn4.8/cs/    <- Roslyn 4.8 to 4.12
analyzers/dotnet/roslyn4.13/cs/   <- Roslyn 4.13 and newer
```

The .NET SDK picks the highest folder your compiler supports. An old-style project that does not use the .NET
SDK gets both folders. The package's build targets remove the folder your compiler does not use. Loading the
generator twice would write every generated file twice and fail your build.

## When nothing claims the call

Some call sites cannot be read when you build. Examples are a lambda stored in a variable, an expression built
at run time, and a call on a generic type parameter `T`. None of them name a path the generator can see.

For those call sites, call the `Unsafe` overload. It uses reflection. Reflection looks up each property by name
while the app runs. A trimmer cannot see those lookups, so an `Unsafe` overload may break after trimming or
ahead-of-time publishing. The other overloads keep working.

When nothing was generated for a call site, the plain method throws. The error message names the `Unsafe`
overload to use:

```csharp
Expression<Func<MyViewModel, string>> selector = x => x.Name;

vm.WhenChanged(selector);        // throws, and names WhenChangedUnsafe
vm.WhenChangedUnsafe(selector);  // walks the path by reflection
```

Thirteen methods have an `Unsafe` twin.

| Resolved when you build | Resolved by reflection |
|-------------------------|------------------------|
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

Every `Unsafe` overload carries `[RequiresUnreferencedCode]`. The plain overloads carry none. So a
`PublishTrimmed` or `PublishAot` build warns about each call that uses reflection, and about nothing else.
RXUIBIND001, RXUIBIND006 and RXUIBIND009 point out those call sites when you build, before anything throws.

The scheduler overloads split the same way. Both kinds live on `ReactiveSchedulerExtensions`.

Two members always use reflection, so their plain names carry the attribute.

- `WhenAnyDynamic`. See [Observing a path built at run time](#observing-a-path-built-at-run-time).
- The object-typed `ResolveView` on the view locator. See [The view locator](#the-view-locator).

## Installing

```
dotnet add package ReactiveUI.Binding
```

The generator and the analyzer ship inside that package. You do not need to reference anything else.

If your app uses System.Reactive, install `ReactiveUI.Binding.Reactive` instead. It is the same library, built
against `System.Reactive.Concurrency.IScheduler`. Its namespaces start with `ReactiveUI.Binding.Reactive`.

Reference one package or the other, never both. They share no type names. So both together put two copies of
every method in scope.

## Supported frameworks

| Target | Versions | Microsoft support ends |
|--------|----------|------------------------|
| .NET | 10.0, 11.0 | November 2028 for .NET 10 |
| .NET | 8.0, 9.0 | 10 November 2026 |
| .NET Framework | 4.7, 4.7.1, 4.7.2, 4.8, 4.8.1 | tied to the Windows version |
| .NET Framework | 4.6.2 | 12 January 2027 |

> [!WARNING]
> .NET 8 and .NET 9 leave Microsoft support on 10 November 2026. This library drops them on that date. .NET
> Framework 4.6.2 leaves support on 12 January 2027, and this library drops it then. Before those dates, move to
> .NET 10 or later, or to .NET Framework 4.7 or later. See the
> [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) and the
> [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework).

The WPF and WinForms packages target the .NET Framework versions and the Windows targets of .NET 8 to 11.

The MAUI packages start at .NET 10. They add Android, iOS, macOS, Mac Catalyst and tvOS targets. The Apple
targets build only on Windows and macOS.

NativeAOT works on .NET 8 and later. Only the generated code runs there. The `Unsafe` overloads compile
expressions at run time, and NativeAOT cannot do that.

## Packages

Ten packages ship. Each runtime package carries the generator and the analyzer. The platform packages get them
through the runtime package.

| Package | What it is |
|---------|------------|
| `ReactiveUI.Binding` | The runtime library. It has lightweight observables and no System.Reactive dependency. |
| `ReactiveUI.Binding.Reactive` | The same library, built against System.Reactive's `IScheduler`. |
| `ReactiveUI.Binding.Wpf` | WPF dependency-property observation. |
| `ReactiveUI.Binding.Wpf.Reactive` | The same, for a System.Reactive app. |
| `ReactiveUI.Binding.WinForms` | WinForms component observation. |
| `ReactiveUI.Binding.WinForms.Reactive` | The same, for a System.Reactive app. |
| `ReactiveUI.Binding.Maui` | MAUI bindable-property observation. |
| `ReactiveUI.Binding.Maui.Reactive` | The same, for a System.Reactive app. |
| `ReactiveUI.Binding.SourceGenerators` | MSBuild props and targets only. A compatibility package. |
| `ReactiveUI.Binding.Analyzer` | The analyzer project. Its files ship inside the runtime packages. |

## Supported APIs

| API | What it does | Properties per call |
|-----|--------------|--------------------:|
| `WhenChanged` | Observes a property after it changes. | 16 |
| `WhenChanging` | Observes a property before it changes. | 16 |
| `WhenAnyValue` | Observes a property after it changes, like `WhenChanged`. | 16 |
| `WhenAny` | Observes properties and hands each change to a selector. | 12 |
| `WhenAnyObservable` | Observes properties that hold observables, and switches between them. | 12 |
| `WhenAnyDynamic` | Observes a path you built as an `Expression`. | 12 |
| `BindOneWay` | Writes a source property to a target property. | 1 each side |
| `BindTwoWay` | Carries a property both ways. | 1 each side |
| `OneWayBind` | A one-way binding, written view first. | 1 each side |
| `Bind` | A two-way binding, written view first. | 1 each side |
| `BindTo` | Writes an observable's values to a target property. | 1 |
| `BindCommand` | Binds a command to a control's event. | 1 |
| `BindInteraction` | Registers a handler for an interaction a property holds. | 1 |
| `InvokeCommand` | Runs a command with each value an observable produces. | 1 |

Each of them reads a single property, a path such as `x => x.Address.City`, or several properties at once. When
an object in the middle of a path is replaced, the subscription moves to the new object.

`BindOneWay`, `BindTwoWay`, `OneWayBind` and `Bind` also accept a scheduler. The observation methods do not.

## Examples

### Observing a property

```csharp
// One property.
IObservable<string> name = vm.WhenChanged(x => x.Name);

// A path. Replacing Address moves the subscription to the new Address.
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

`OneWayBind` and `Bind` are the same bindings, written with the view first:

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

A `ReactiveCommand` works like any other `ICommand`. The parameter arrives as `object`, not as the command's
declared input type.

### Observing a path built at run time

`WhenAnyDynamic` takes the path as an `Expression`, not a lambda. So you can build the path from a property name
or a setting. It observes 1 to 12 paths at once. Each count has an overload that reports only changed values,
and one that reports every value.

```csharp
Expression chain = ((Expression<Func<MyViewModel, string>>)(x => x.Address.City)).Body;

IObservable<string?> city = vm.WhenAnyDynamic(chain, static c => (string?)c.Value);
```

The generator cannot read an expression built at run time. So `WhenAnyDynamic` walks the path by reflection.
Every overload carries `[RequiresUnreferencedCode]`, so a `PublishAot` build reports each call. When you know the
path before you build, `WhenChanged` and `WhenAny` observe the same thing without reflection.

## The view locator

A view locator finds the view for a view model. Implement `IViewFor<T>`, and the generator registers the view for
you.

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
| `[SingleInstanceView]` | Keeps one instance instead of creating a view each time. Do not use it for a view that appears more than once in the tree. |
| `[ExcludeFromViewRegistration]` | Leaves the view out of the generated registration. |

```csharp
[ViewContract("compact")]
public class CompactDashboardView : IViewFor<DashboardViewModel> { }

public class FullDashboardView : IViewFor<DashboardViewModel> { }

var compact = ViewLocator.GetCurrent().ResolveView(vm, "compact");
var full = ViewLocator.GetCurrent().ResolveView(vm);
```

`ResolveView` looks in three places, in order.

1. The generated lookup. It is a type switch with no reflection.
2. A mapping you added at run time with `Map<TViewModel, TView>()`.
3. The service locator, `Splat.AppLocator.Current`.

In the generated lookup, a view registered under the requested contract comes before the default view. The view
instance comes from the first of these that has one.

1. The service locator.
2. The cached instance, when the view is marked `[SingleInstanceView]`. The first resolution creates it.
3. A new instance from the view's parameterless constructor.

The cache is set with `Interlocked.CompareExchange`. So two threads resolving at once share one instance.

Use the generic `ResolveView` overload where you can. The object-typed overload closes `IViewFor<>` over a type
known only at run time. So it carries `[RequiresDynamicCode]` and is not safe for ahead-of-time publishing.

## Which mechanism wins

Each mechanism has a score. The highest-scoring mechanism that can reach the property you named wins.

| Mechanism | Type it keys on | Score |
|-----------|-----------------|------:|
| Apple KVO | `Foundation.NSObject` | 15 |
| IReactiveObject | `ReactiveUI.IReactiveObject` | 10 |
| WinForms component | `System.ComponentModel.Component` | 8 |
| WinUI bindable property | `Microsoft.UI.Xaml.DependencyObject` | 6 |
| INotifyPropertyChanged | `System.ComponentModel.INotifyPropertyChanged` | 5 |
| Android view | `Android.Views.View` | 5 |
| WPF dependency property | `System.Windows.DependencyObject` | 4 |

A mechanism has to reach the property. A plain C# property on a dependency object is not a dependency property. A
component property with no `{PropertyName}Changed` event has nothing to attach to. Both fall through to the next
mechanism down.

`INotifyPropertyChanged` and `Android.Views.View` share a score. `INotifyPropertyChanged` wins that tie.

An `ICreatesObservableForProperty` you register yourself uses the same scores. It takes the property when it
scores higher. The generated code wins a tie.

## Which thread a binding writes on

A UI framework lets only one thread touch a view. That thread is the view's owning thread. A view model can raise
a change on any thread. So a binding moves each write to the owning thread.

The binding asks the object it writes to. Each platform has its own check and its own way to queue work.

| Target | Check | Queue |
|--------|-------|-------|
| WPF `DispatcherObject` | `CheckAccess()` | `Dispatcher.BeginInvoke` |
| WinForms `Control` | `InvokeRequired` | `Control.BeginInvoke` |
| MAUI `BindableObject` | `Dispatcher.IsDispatchRequired` | `Dispatcher.Dispatch` |

WPF can run several UI threads. Each window belongs to one of them. Asking the object sends each write to the
right one.

The binding asks on every write.

- A write on the owning thread runs straight away when no earlier write is waiting. Set a property on the UI
  thread, and the control has the new value on the next line.
- A write from another thread waits for the owning thread. A write on the owning thread also waits while an
  earlier write is waiting.
- Only the latest value waits. A newer change replaces the waiting one, so a burst of changes becomes one write.

Some objects have no owning thread. The binding writes to them straight away.

- A frozen WPF `Freezable`.
- A WinForms control with no window handle yet. Once the handle exists, writes go to the thread that created it.
- A MAUI object with no dispatcher, such as a view in a unit test.
- Any object that is not a WPF, WinForms or MAUI object, such as a plain view model.

Every binding API does this: `BindOneWay`, `BindTwoWay`, `OneWayBind`, `Bind`, `BindTo`, and `BindCommand` when
it binds a new command to the control. Each `Unsafe` twin does the same through the registered invokers.

### Invokers

An `IViewThreadInvoker` does the check and the queueing for one platform. Each platform package has a module
that registers one: `WpfBindingModule`, `WinFormsBindingModule` or `MauiBindingModule`. You can register your own.
An invoker you register is asked first.

A generated binding knows its target's type when it compiles. For a WPF, WinForms or MAUI target, it carries that
platform's invoker. So it routes writes even when the platform module is not registered.

An `Unsafe` binding only finds its target's type while the app runs. It uses the registered invokers alone. Register
the platform module when you use `Unsafe` bindings.

### Choosing the thread yourself

> [!TIP]
> Pass a scheduler to pick the thread for one binding: `vm.BindOneWay(view, x => x.Name, x => x.NameLabel,
> scheduler: someScheduler)`. That binding skips the check.

To change it for every binding, set `BindingSchedulers.MainThread`, or call
`BindingSchedulers.UseSynchronizationContext(context)`. Only writes from another thread go through it. A write on
the owning thread still runs straight away. So does a write to an object no invoker claims.

## Rx library compatibility

Rx means the Reactive Extensions: libraries of operators for `IObservable<T>`. This library's observables and
operators come from **ReactiveUI.Primitives**. `ReactiveUI.Binding` depends on no other Rx library.

`ReactiveUI.Binding.Reactive` uses System.Reactive through **ReactiveUI.Primitives.Reactive**. Neither package
references System.Reactive directly.

| Package | Depends on | Scheduler type |
|---------|------------|----------------|
| `ReactiveUI.Binding` | `ReactiveUI.Primitives` | `ISequencer` |
| `ReactiveUI.Binding.Reactive` | `ReactiveUI.Primitives.Reactive` | `IScheduler` |

Every binding returns a standard .NET `IObservable<T>`. So you are not tied to either library.

| Library | How it works |
|---------|--------------|
| ReactiveUI.Primitives | The default. Reference `ReactiveUI.Binding`. |
| System.Reactive | Reference `ReactiveUI.Binding.Reactive`. |
| R3 | R3 has its own `Observable<T>` class. Convert with `.ToObservable()`. |
| Anything else | Any library that accepts `IObservable<T>` works as it is. |

## Performance

On .NET 10, a thousand property changes through a two-way binding take 53.6 us and allocate 42.2 KB. Observing
one property takes 79.5 us and allocates 65.5 KB. Published ahead of time, the same code runs within a few per
cent of the JIT build.

[src/benchmarks/README.md](src/benchmarks/README.md) lists the benchmarks, the machine they ran on, what each one
covers, and how to run them.

## Diagnostics

The analyzer ships inside the runtime packages. It reports these diagnostics.

| ID | Severity | What it means |
|----|----------|---------------|
| RXUIBIND001 | Info | The expression is not an inline lambda, so nothing is generated. Call the `Unsafe` overload, or the call will throw. |
| RXUIBIND002 | Warning | The type has no observable property and raises no change event. |
| RXUIBIND003 | Warning | The expression reads a private or protected member. A generated extension method cannot read it. |
| RXUIBIND004 | Warning | The type raises no before-change event. `WhenChanging` reads the value once and then stays silent. |
| RXUIBIND005 | Info | The source implements `INotifyDataErrorInfo`. No code is generated for validation state. |
| RXUIBIND006 | Warning | The path contains an indexer, a field or a method call. Only property reads are generated. |
| RXUIBIND007 | Warning | The control named by `BindCommand` has no default event to bind. Pass `toEvent`. |
| RXUIBIND008 | Warning | The property named by `BindInteraction` does not implement `IInteraction<TInput, TOutput>`. |
| RXUIBIND009 | Warning | The generated overload cannot be reached from this file, so the call throws. Not reported when an interceptor takes the call. |
| RXUIBIND010 | Warning | The path passes through a type that raises no change event. The value is read once, and the path is followed no further. |
| RXUIBIND011 | Warning | The call resolved to ReactiveUI's own extension method. Nothing is generated, and the call uses reflection. |

The package's build targets report one error of their own.

| ID | What it means |
|----|---------------|
| RXUIBIND100 | The compiler is older than Roslyn 4.8, so no generator would load. Upgrade to Visual Studio 2022 17.8 or the .NET 8.0.100 SDK. |

## Where this differs from ReactiveUI

The sections below list every way a binding here behaves differently from ReactiveUI.

### Bindings are generated, not reflected

ReactiveUI does the generator's job while the app runs. It compiles the lambda into a delegate, then finds each
property by name. That costs time on every binding. A trimmer cannot see which members those lookups need, so it
may remove them.

The generated code is faster and allocates less. On .NET 10, a thousand property changes through a two-way
binding take 53.6 us and 42.2 KB here. ReactiveUI's engine takes 686.5 us and 932.3 KB. Observing one property
takes 79.5 us and 65.5 KB here, against 145.6 us and 105.5 KB. ReactiveUI's engine compiles expressions at run
time, so it cannot run under NativeAOT.

### ReactiveUI's method names are kept

`WhenAnyValue`, `OneWayBind` and `Bind` use ReactiveUI's names. They make code written for ReactiveUI easier to
move across.

### Observation delivery is serialized

Property observations deliver on the raising thread when the delivery gate is available. A competing producer
waits up to 20 ms, then hands its notification to the delivering thread. Subscriber calls do not hold a lock,
so a subscriber can wait for another thread to update the same property. Observation APIs leave scheduling
to the caller.

After-change and custom-provider observations read inside the gate and collapse contended changes to the
latest value. A change raised by the subscriber is delivered after that subscriber returns. Before-change
observations capture values before the write and preserve their order; a nested notification on the delivering
thread reaches the subscriber before the nested setter writes. Disposal stops pending delivery.

### `TriggerUpdate` and `signalViewUpdate` use `BindUnsafe`

The view-first `BindUnsafe` overloads accept an update stream and `TriggerUpdate`, with either registered
converters or explicit conversion delegates. These overloads resolve property paths by reflection and carry
`RequiresUnreferencedCode`; calling them produces a trimming diagnostic. Generated bindings use the two
property lambdas and do not offer these parameters.

`ViewToViewModel` is the default. A supplied stream replaces the view's change notifications, so editing the
view writes back only when the stream signals. With `ViewModelToView`, the stream drives model-to-view updates
after the initial model notification, and the view's own notifications always participate. A null stream observes
both properties in either mode; pass a typed null such as `(IObservable<int>?)null` for type inference.

Both sides are wired before a single initial signal writes from view model to view. Each signal reads the
current values when delivered, converts in its selected direction, and skips a write when the converted value
equals the destination. Disposing the binding disconnects both directions and the supplied stream.

### Every binding writes on the thread that owns the view

ReactiveUI moves a write to the UI thread only on WPF. It does so for a two-way `Bind`, and when it swaps a
control's `Command`. Its other bindings write on the thread that raised the change. So do all its WinForms and
MAUI bindings.

Here every binding moves its writes, on all three platforms. See
[Which thread a binding writes on](#which-thread-a-binding-writes-on). A background update that throws under
ReactiveUI works here. An update from another thread that ReactiveUI applied at once arrives one turn of the
message loop later.

Where ReactiveUI does move a write, the order is the same. A write on the owning thread runs straight away. A
write from another thread goes through the main-thread scheduler. Set `BindingSchedulers.MainThread` to
ReactiveUI's main-thread scheduler to use the same scheduler.

A burst of changes from another thread is handled differently. ReactiveUI's one-way bindings and `BindTo` write
every value, on the thread that raised it. Its two-way `Bind` queues one signal per change and reads the current
value when each signal runs. Here every binding writes only the latest value, once. A binding's change stream
skips the values in between.

### A binding made through a type parameter is not generated

A generated overload has to name the bound types. A call through a type parameter names none. The type is only
known once code fills in the parameter. So the generator leaves those calls to the plain overload. A generic
binding helper then compiles, but throws when it runs. An overload that named a type parameter would break your
build instead. From the helper, call the `Unsafe` twin to bind by reflection.

### A property with no change event is reported when you build

ReactiveUI logs a warning the first time it observes a property on a type that raises no change event.
RXUIBIND010 reports the same thing when you build. A generator can only report it then. The observation behaves
the same either way. The value is read once, and the path is followed no further.

### A write that throws behaves the same

A write that throws is logged against the bound expression. It is rethrown as a `TargetInvocationException` only
when it has an inner exception. The setter threw on the thread that raised the change, so no caller can catch it.
Swallowing the exception would hide the failure.

## Layout

```
src/
  ReactiveUI.Binding/                     Runtime package, lightweight observables
  ReactiveUI.Binding.Reactive/            Runtime package, System.Reactive schedulers
  ReactiveUI.Binding.Shared/              The runtime source, compiled by both packages above
  ReactiveUI.Binding.SourceGenerators/    The generator, and the shipped props and targets
  ReactiveUI.Binding.Analyzer/            The RXUIBIND analyzers
  ReactiveUI.Binding.*.Roslyn413/         The same generator and analyzer against Roslyn 4.13
  ReactiveUI.Binding.Wpf*/                WPF integration, a standard and a .Reactive package
  ReactiveUI.Binding.WinForms*/           WinForms integration, a standard and a .Reactive package
  ReactiveUI.Binding.Maui*/               MAUI integration, a standard and a .Reactive package
  benchmarks/                             BenchmarkDotNet projects
  tests/                                  Test projects and the shared scenario sources
```

A `*.Shared` folder holds source, not a project. Each package that uses it compiles its own copy. That is how one
set of code builds both a standard package and its `.Reactive` twin. `ReactiveShim.props` looks for the
`.Reactive` suffix on the project name. It defines `REACTIVE_SHIM` and maps the scheduler type to `IScheduler`.

The generator and the analyzer target netstandard2.0, because Roslyn requires it. Generated code compiles as C#
7.3, so the oldest supported project can build it.

## Contribute

ReactiveUI.Binding.SourceGenerators uses an OSI-approved open source license. You can use and share it freely,
including for commercial work. We value everyone who takes part. We would love to have you, even if you have
never contributed to open source before.

Ways to help:

* [Answer questions on GitHub Discussions](https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/discussions)
* [Share what you know and teach other developers](http://ericsink.com/entries/dont_use_rxui.html)
* Improve the docs where something is missing or unclear.
* Contribute code.
