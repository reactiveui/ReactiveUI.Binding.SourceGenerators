// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Names the thread that owns one object, so a binding can deliver its write there.</summary>
/// <remarks>
/// <para>
/// Registered by a platform package. The question is asked of the object being written to rather than of the
/// process, because thread affinity belongs to the object: WPF allows several UI threads, each owning its own
/// windows, and a write marshalled to the wrong one throws exactly as an unmarshalled write does.
/// </para>
/// <para>
/// A resolver answers null for anything it does not recognise, which lets several be registered together - a
/// process hosting both WPF and WinForms asks each until one claims the target.
/// </para>
/// </remarks>
public interface IViewThreadResolver
{
    /// <summary>Names the thread that owns <paramref name="target"/>.</summary>
    /// <param name="target">The object a binding is about to write to.</param>
    /// <returns>The context owning it, or null when this resolver does not recognise it.</returns>
    /// <remarks>
    /// <para>
    /// A context rather than a sequencer, so a platform package never has to know which runtime flavour it was
    /// compiled into. The runtime wraps it in whichever abstraction that flavour speaks.
    /// </para>
    /// <para>
    /// The context is expected to run a callback inline when the caller is already on the thread it names, and
    /// to queue only a callback from elsewhere. A binding asks on every write, so a context that always queues
    /// turns an ordinary on-thread update into another turn of the message loop, and a caller that sets a
    /// property and reads the view back sees the value it had before.
    /// </para>
    /// </remarks>
    SynchronizationContext? ContextFor(object target);
}
