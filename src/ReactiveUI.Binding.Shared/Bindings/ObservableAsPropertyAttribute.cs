// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Marks a partial, get-only property as backed by an <see cref="ObservableAsPropertyHelper{T}"/>. The source
/// generator writes the property's body and a helper field named <c>_{name}Helper</c>, which you assign with
/// <c>ToProperty</c>.
/// </summary>
/// <remarks>
/// The property has to be declared by you, as a partial property, so every generator in the build can see it. A
/// property only another generator writes is invisible to this one, so it could not be observed or bound.
/// </remarks>
/// <example>
/// <code language="csharp">
/// <![CDATA[
/// public partial class PersonViewModel : INotifyPropertyChanged
/// {
///     public PersonViewModel(IObservable<string> names) =>
///         _fullNameHelper = names.ToProperty(this, x => x.FullName);
///
///     public event PropertyChangedEventHandler? PropertyChanged;
///
///     [ObservableAsProperty]
///     public partial string FullName { get; }
/// }
/// ]]>
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ObservableAsPropertyAttribute : Attribute;
