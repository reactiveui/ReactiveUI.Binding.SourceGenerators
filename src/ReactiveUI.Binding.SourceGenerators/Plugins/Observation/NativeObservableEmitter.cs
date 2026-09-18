// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Emits typed subscription storage shared by native notification mechanisms.</summary>
internal static class NativeObservableEmitter
{
    /// <summary>Opens a generated member body.</summary>
    private const string MemberOpen = "            {";

    /// <summary>Declares a helper that keeps source and value types concrete.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="name">The plugin's helper type name.</param>
    internal static void EmitHelper(StringBuilder sb, string name)
    {
        _ = sb.Append("        private sealed class ").Append(name).AppendLine("<TSource, TValue> : global::System.IObservable<TValue>")
            .AppendLine("        {")
            .AppendLine("            private readonly TSource _source;")
            .AppendLine("            private readonly global::System.Func<TSource, global::System.Action, global::System.IDisposable> _subscribe;")
            .AppendLine("            private readonly global::System.Func<TSource, TValue> _getter;")
            .AppendLine("            private readonly bool _distinct;")
            .Append("            internal ").Append(name).AppendLine("(TSource source,")
            .AppendLine("                global::System.Func<TSource, global::System.Action, global::System.IDisposable> subscribe,")
            .AppendLine("                global::System.Func<TSource, TValue> getter, bool distinct)")
            .AppendLine(MemberOpen)
            .AppendLine("                _source = source;")
            .AppendLine("                _subscribe = subscribe;")
            .AppendLine("                _getter = getter;")
            .AppendLine("                _distinct = distinct;")
            .AppendLine("            }")
            .AppendLine("            public global::System.IDisposable Subscribe(global::System.IObserver<TValue> observer)")
            .AppendLine(MemberOpen)
            .AppendLine("                if (observer == null) throw new global::System.ArgumentNullException(nameof(observer));")
            .AppendLine("                return new Subscription(this, observer);")
            .AppendLine("            }")
            .AppendLine("            private sealed class Subscription : global::System.IDisposable")
            .AppendLine(MemberOpen)
            .Append("                private readonly ").Append(name).AppendLine("<TSource, TValue> _parent;")
            .AppendLine("                private readonly global::System.IDisposable _inner;")
            .AppendLine("                private global::System.IObserver<TValue> _observer;")
            .AppendLine("                private TValue _lastValue;")
            .AppendLine("                private bool _hasValue;")
            .Append("                internal Subscription(").Append(name).AppendLine("<TSource, TValue> parent, global::System.IObserver<TValue> observer)")
            .AppendLine("                {")
            .AppendLine("                    _parent = parent;")
            .AppendLine("                    _observer = observer;")
            .AppendLine("                    _inner = parent._subscribe(parent._source, Publish);")
            .AppendLine("                    try")
            .AppendLine("                    {")
            .AppendLine("                        Publish();")
            .AppendLine("                    }")
            .AppendLine("                    catch")
            .AppendLine("                    {")
            .AppendLine("                        Dispose();")
            .AppendLine("                        throw;")
            .AppendLine("                    }")
            .AppendLine("                }");
        AppendDelivery(sb);
    }

    /// <summary>Emits value delivery and idempotent native detachment.</summary>
    /// <param name="sb">The output builder.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendDelivery(StringBuilder sb) =>
        sb.AppendLine("""
                        public void Dispose()
                        {
                            if (global::System.Threading.Interlocked.Exchange(ref _observer, null) != null)
                            {
                                _inner.Dispose();
                            }
                        }
                        private void Publish()
                        {
                            var observer = global::System.Threading.Volatile.Read(ref _observer);
                            if (observer == null)
                            {
                                return;
                            }
                            var value = _parent._getter(_parent._source);
                            if (_parent._distinct && _hasValue && global::System.Collections.Generic.EqualityComparer<TValue>.Default.Equals(_lastValue, value))
                            {
                                return;
                            }
                            _lastValue = value;
                            _hasValue = true;
                            observer.OnNext(value);
                        }
                    }
                }
        """);
}
