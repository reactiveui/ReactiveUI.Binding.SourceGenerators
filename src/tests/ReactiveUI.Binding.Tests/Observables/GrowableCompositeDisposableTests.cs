// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Tests for <see cref="GrowableCompositeDisposable"/>.</summary>
public class GrowableCompositeDisposableTests
{
    /// <summary>The number of resources added in the multiple-resource test.</summary>
    private const int ResourceCount = 3;

    /// <summary>One disposal apiece, the expected outcome for <see cref="ResourceCount"/> resources.</summary>
    private static readonly int[] OneDisposalEach = [1, 1, 1];

    /// <summary>Verifies that every retained resource is disposed exactly once.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Dispose_DisposesEveryAddedResource()
    {
        var composite = new GrowableCompositeDisposable();
        var disposeCounts = new int[ResourceCount];

        for (var i = 0; i < ResourceCount; i++)
        {
            var index = i;
            composite.Add(new ActionDisposable(() => disposeCounts[index]++));
        }

        composite.Dispose();
        composite.Dispose();

        await Assert.That(disposeCounts).IsEquivalentTo(OneDisposalEach);
    }

    /// <summary>Verifies that a resource handed over after disposal is disposed rather than retained.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Add_AfterDispose_DisposesImmediately()
    {
        var composite = new GrowableCompositeDisposable();
        var disposed = false;

        composite.Dispose();
        composite.Add(new ActionDisposable(() => disposed = true));

        await Assert.That(disposed).IsTrue();
    }

    /// <summary>Verifies that a composite with nothing in it disposes cleanly.</summary>
    [Test]
    public void Dispose_WithNothingAdded_DoesNotThrow()
    {
        var composite = new GrowableCompositeDisposable();

        composite.Dispose();
    }

    /// <summary>Verifies that Add rejects a null resource.</summary>
    [Test]
    public void Add_Null_ThrowsArgumentNullException()
    {
        var composite = new GrowableCompositeDisposable();

        _ = Assert.Throws<ArgumentNullException>(() => composite.Add(null!));
    }
}
