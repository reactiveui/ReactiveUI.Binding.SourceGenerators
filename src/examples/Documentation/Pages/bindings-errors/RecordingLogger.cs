// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Splat;

namespace ReactiveUI.Binding.Documentation.BindingsErrors;

/// <summary>A logger that keeps every entry the application writes, so an example can read what a binding reported.</summary>
[System.Diagnostics.DebuggerDisplay("Messages = {Messages.Count}")]
public sealed class RecordingLogger : ILogger
{
    /// <summary>Gets the text of every entry, in the order written.</summary>
    public List<string> Messages { get; } = [];

    /// <summary>Gets the exception of every entry that carries one, in the order written.</summary>
    public List<Exception> Exceptions { get; } = [];

    /// <inheritdoc/>
    public LogLevel Level { get; set; } = LogLevel.Debug;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(string message, LogLevel logLevel) => Messages.Add(message);

    /// <inheritdoc/>
    public void Write(Exception exception, string message, LogLevel logLevel)
    {
        Messages.Add(message);
        Exceptions.Add(exception);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(string message, Type type, LogLevel logLevel) => Messages.Add(message);

    /// <inheritdoc/>
    public void Write(Exception exception, string message, Type type, LogLevel logLevel)
    {
        Messages.Add(message);
        Exceptions.Add(exception);
    }

    /// <summary>Forgets every entry written so far.</summary>
    public void Clear()
    {
        Messages.Clear();
        Exceptions.Clear();
    }
}
