// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.ViewThread;

/// <summary>Names the invoker a generated binding carries for one UI platform, and emits its declaration.</summary>
internal interface IViewThreadPlugin
{
    /// <summary>Gets the metadata name of the platform type whose instances this invoker writes to.</summary>
    string OwnerMetadataName { get; }

    /// <summary>Gets the name of the invoker class the generator declares.</summary>
    string InvokerTypeName { get; }

    /// <summary>Emits the invoker class declaration.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="nullableSuffix">The annotation for a nullable reference type, or empty below C# 8.</param>
    void EmitInvoker(StringBuilder sb, string nullableSuffix);
}
