// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Todo;

/// <summary>
/// A to-do database that lives in memory. It keeps its own rows and hands out copies, so changing an item you
/// read does not change the database until you call <see cref="UpdateAsync"/>.
/// </summary>
[System.Diagnostics.DebuggerDisplay("Rows = {RowCount}, IsOffline = {IsOffline}")]
public sealed class InMemoryTodoStore : ITodoStore
{
    /// <summary>The message of the exception thrown while the database is offline.</summary>
    private const string OfflineMessage = "The to-do database is unreachable.";

    /// <summary>The stored rows, in the order they were added.</summary>
    private readonly List<TodoItem> _rows = [];

    /// <summary>The identifier the next added item receives.</summary>
    private int _nextId = 1;

    /// <summary>Gets the gate that decides when each call returns.</summary>
    public ResponseGate Gate { get; } = new();

    /// <summary>Gets or sets a value indicating whether every call fails with a <see cref="TodoStoreException"/>.</summary>
    public bool IsOffline { get; set; }

    /// <summary>Gets the number of stored rows, without going through the gate.</summary>
    public int RowCount => _rows.Count;

    /// <summary>Creates a database that holds a few realistic household tasks.</summary>
    /// <returns>A new database with four items, one of them finished.</returns>
    public static InMemoryTodoStore CreateSeeded()
    {
        InMemoryTodoStore store = new();
        store.Seed("Renew car registration", "Bring the insurance certificate.", SeedData.Date("2026-03-20"), TodoPriority.High, false, "car", "admin");
        store.Seed("Book dentist appointment", "Ask about the evening slots.", SeedData.Date("2026-04-02"), TodoPriority.Normal, false, "health");
        store.Seed("Buy birthday present for Sam", string.Empty, null, TodoPriority.Low, false, "family");
        store.Seed("File quarterly tax return", "Receipts are in the shared folder.", SeedData.Date("2026-02-28"), TodoPriority.High, true, "admin", "money");
        return store;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<IReadOnlyList<TodoItem>> QueryAsync() => QueryAsync(string.Empty);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoItem>> QueryAsync(string text)
    {
        await EnterAsync().ConfigureAwait(false);

        List<TodoItem> matches = [];
        foreach (var row in _rows)
        {
            if (row.Matches(text))
            {
                matches.Add(row.Clone());
            }
        }

        return matches;
    }

    /// <inheritdoc/>
    public async Task<TodoItem?> GetAsync(int id)
    {
        await EnterAsync().ConfigureAwait(false);

        return _rows.Find(row => row.Id == id)?.Clone();
    }

    /// <inheritdoc/>
    public async Task<TodoItem> AddAsync(TodoItem item)
    {
        await EnterAsync().ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(item.Title))
        {
            throw new TodoStoreException("A to-do item needs a title.");
        }

        var row = item.Clone();
        row.Id = _nextId;
        _nextId++;
        _rows.Add(row);
        return row.Clone();
    }

    /// <inheritdoc/>
    public async Task<TodoItem> UpdateAsync(TodoItem item)
    {
        await EnterAsync().ConfigureAwait(false);

        var index = _rows.FindIndex(row => row.Id == item.Id);
        if (index < 0)
        {
            throw new TodoStoreException($"There is no to-do item {item.Id}.");
        }

        _rows[index] = item.Clone();
        return item.Clone();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        await EnterAsync().ConfigureAwait(false);

        if (_rows.RemoveAll(row => row.Id == id) == 0)
        {
            throw new TodoStoreException($"There is no to-do item {id}.");
        }
    }

    /// <summary>Waits for the gate, then fails when the database is offline.</summary>
    /// <returns>A task that completes when the call may proceed.</returns>
    /// <exception cref="TodoStoreException">The database is offline.</exception>
    private async Task EnterAsync()
    {
        await Gate.WaitAsync().ConfigureAwait(false);

        if (IsOffline)
        {
            throw new TodoStoreException(OfflineMessage);
        }
    }

    /// <summary>Adds a row without going through the gate.</summary>
    /// <param name="title">The title of the row.</param>
    /// <param name="notes">The notes of the row.</param>
    /// <param name="dueDate">The due date of the row.</param>
    /// <param name="priority">The priority of the row.</param>
    /// <param name="isDone">Whether the row is finished.</param>
    /// <param name="tags">The tags of the row.</param>
    private void Seed(string title, string notes, DateOnly? dueDate, TodoPriority priority, bool isDone, params string[] tags)
    {
        TodoItem row = new() { Id = _nextId, Title = title, Notes = notes, DueDate = dueDate, Priority = priority, IsDone = isDone, Tags = tags };
        _nextId++;
        _rows.Add(row);
    }
}
