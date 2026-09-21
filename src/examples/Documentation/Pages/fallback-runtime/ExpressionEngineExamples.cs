// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Reflection;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Expressions;

namespace ReactiveUI.Binding.Documentation.FallbackRuntime;

/// <summary>
/// Demonstrates the expression helpers the runtime fallback reads a property path with: <see cref="ExpressionMixins"/>
/// splits an expression into its links, and <see cref="Reflection"/> reads and writes the value each link holds.
/// </summary>
public static class ExpressionEngineExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives an item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The name of the <see cref="TransferViewModel.Draft"/> property.</summary>
    private const string DraftName = nameof(TransferViewModel.Draft);

    /// <summary>The name of the <see cref="TransferDraft.Amount"/> property.</summary>
    private const string AmountName = nameof(TransferDraft.Amount);

    /// <summary>The name of the <see cref="TodoItem.Title"/> property.</summary>
    private const string TitleName = nameof(TodoItem.Title);

    /// <summary>The amount an example writes.</summary>
    private const decimal AmountValue = 125.50M;

    /// <summary>The number of links in the path <c>x.Draft.Amount</c>.</summary>
    private const int DraftAmountLinks = 2;

    /// <summary>The index an example reads a tag at.</summary>
    private const int FirstTagIndex = 0;

    /// <summary>The first tag of the item an example reads.</summary>
    private const string FirstTag = "car";

    /// <summary>Names the properties of a path, joined by dots.</summary>
    public static void NameThePathOfAnExpression()
    {
        Expression<Func<TransferViewModel, decimal>> path = x => x.Draft.Amount;

        SampleCheck.Equal("Draft.Amount", Reflection.ExpressionToPropertyNames(path.Body));
    }

    /// <summary>Splits an expression into one link for each member, from the root outwards.</summary>
    public static void SplitAnExpressionIntoLinks()
    {
        Expression<Func<TransferViewModel, decimal>> path = x => x.Draft.Amount;

        var chain = new List<Expression>(path.Body.GetExpressionChain());

        SampleCheck.Equal(DraftAmountLinks, chain.Count);
        SampleCheck.Equal(DraftName, ((MemberExpression)chain[0]).Member.Name);
        SampleCheck.Equal(AmountName, ((MemberExpression)chain[1]).Member.Name);
    }

    /// <summary>Finds the member an expression points at, looking through the conversion a compiler adds when it boxes a value.</summary>
    public static void FindTheMemberBehindAConversion()
    {
        Expression<Func<TodoItem, object>> path = x => x.Id;

        SampleCheck.Equal(ExpressionType.Convert, path.Body.NodeType);
        SampleCheck.Equal(nameof(TodoItem.Id), path.Body.GetMemberInfo()!.Name);
    }

    /// <summary>Finds the expression a member is read from.</summary>
    public static void FindTheParentOfAMember()
    {
        Expression<Func<TransferViewModel, decimal>> path = x => x.Draft.Amount;

        var parent = (MemberExpression)path.Body.GetParent()!;

        SampleCheck.Equal(DraftName, parent.Member.Name);
    }

    /// <summary>Reads the arguments of an indexer link, and reports none for a plain member.</summary>
    public static void ReadTheArgumentsOfAnIndexer()
    {
        Expression<Func<TodoItem, string>> path = x => x.Tags[0];

        var indexer = Reflection.Rewrite(path.Body);
        var arguments = indexer.GetArgumentsArray();

        SampleCheck.Equal(ExpressionType.Index, indexer.NodeType);
        SampleCheck.Equal(1, arguments!.Length);
        SampleCheck.Equal(FirstTagIndex, (int)arguments[0]!);

        Expression<Func<TodoItem, string>> member = x => x.Title;

        SampleCheck.Equal(true, member.Body.GetArgumentsArray() is null);
    }

    /// <summary>Rewrites an expression into the plain shape the chain helpers read: conversions go and an indexer call becomes an indexer link.</summary>
    public static void RewriteAnExpressionIntoChainShape()
    {
        Expression<Func<TodoItem, object>> boxed = x => x.Id;

        SampleCheck.Equal(ExpressionType.MemberAccess, Reflection.Rewrite(boxed.Body).NodeType);

        Expression<Func<TodoItem, string>> indexed = x => x.Tags[0];

        SampleCheck.Equal(ExpressionType.Call, indexed.Body.NodeType);
        SampleCheck.Equal(ExpressionType.Index, Reflection.Rewrite(indexed.Body).NodeType);
    }

    /// <summary>Builds a function that reads a property, and reports none for a member that is not a property or a field.</summary>
    public static void ReadAPropertyWithAFetcher()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var title = typeof(TodoItem).GetProperty(TitleName);

        var fetcher = Reflection.GetValueFetcherForProperty(title);

        SampleCheck.Equal(OriginalTitle, (string)fetcher!(item, null)!);
        SampleCheck.Equal(true, Reflection.GetValueFetcherForProperty(typeof(TodoItem).GetMethod(nameof(TodoItem.Clone))) is null);
    }

    /// <summary>Builds a function that reads a property, and throws for a member that is not a property or a field.</summary>
    public static void ReadAPropertyWithAFetcherOrThrow()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var fetcher = Reflection.GetValueFetcherOrThrow(typeof(TodoItem).GetProperty(TitleName));

        SampleCheck.Equal(OriginalTitle, (string)fetcher(item, null)!);
        SampleCheck.Equal(true, Throws<ArgumentException>(static () => Reflection.GetValueFetcherOrThrow(typeof(TodoItem).GetMethod(nameof(TodoItem.Clone)))));
    }

    /// <summary>Builds a function that writes a property, and reports none for a member that is not a property or a field.</summary>
    public static void WriteAPropertyWithASetter()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var setter = Reflection.GetValueSetterForProperty(typeof(TodoItem).GetProperty(TitleName));

        setter!(item, RenamedTitle, null);

        SampleCheck.Equal(RenamedTitle, item.Title);
        SampleCheck.Equal(true, Reflection.GetValueSetterForProperty(typeof(TodoItem).GetMethod(nameof(TodoItem.Clone))) is null);
    }

    /// <summary>Builds a function that writes a property, and throws for a member that is not a property or a field.</summary>
    public static void WriteAPropertyWithASetterOrThrow()
    {
        var item = new TodoItem { Title = OriginalTitle };
        var setter = Reflection.GetValueSetterOrThrow(typeof(TodoItem).GetProperty(TitleName));

        setter(item, RenamedTitle, null);

        SampleCheck.Equal(RenamedTitle, item.Title);
        SampleCheck.Equal(true, Throws<ArgumentException>(static () => Reflection.GetValueSetterOrThrow(typeof(TodoItem).GetMethod(nameof(TodoItem.Clone)))));
    }

    /// <summary>Reads the value at the end of a path.</summary>
    public static void ReadTheValueAtTheEndOfAPath()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend(Infrastructure.ManualClock.StartOfWorkingDay()));
        viewModel.Draft.Amount = AmountValue;
        var chain = CreateChain<TransferViewModel, decimal>(static x => x.Draft.Amount);

        var found = Reflection.TryGetValueForPropertyChain<decimal>(out var amount, viewModel, chain);

        SampleCheck.Equal(true, found);
        SampleCheck.Equal(AmountValue, amount);
    }

    /// <summary>Reports that a path has no value when a link along it is null.</summary>
    public static void ReadThroughANullLink()
    {
        var viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());
        var chain = CreateChain<TodoListViewModel, string>(static x => x.SelectedItem!.Title);

        var found = Reflection.TryGetValueForPropertyChain<string>(out var title, viewModel, chain);

        SampleCheck.Equal(false, found);
        SampleCheck.Equal(true, title is null);
    }

    /// <summary>Reads the value of every link along a path, each with the object it was read from.</summary>
    public static void ReadEveryValueAlongAPath()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend(Infrastructure.ManualClock.StartOfWorkingDay()));
        viewModel.Draft.Amount = AmountValue;
        var chain = CreateChain<TransferViewModel, decimal>(static x => x.Draft.Amount);

        var found = Reflection.TryGetAllValuesForPropertyChain(out var changes, viewModel, chain);

        SampleCheck.Equal(true, found);
        SampleCheck.Equal(DraftAmountLinks, changes.Length);
        SampleCheck.Equal(true, ReferenceEquals(viewModel, changes[0].Sender));
        SampleCheck.Equal(true, ReferenceEquals(viewModel.Draft, changes[0].Value));
        SampleCheck.Equal(true, ReferenceEquals(viewModel.Draft, changes[1].Sender));
        SampleCheck.Equal(AmountValue, (decimal)changes[1].Value!);
    }

    /// <summary>Writes the value at the end of a path.</summary>
    public static void WriteTheValueAtTheEndOfAPath()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend(Infrastructure.ManualClock.StartOfWorkingDay()));
        var chain = CreateChain<TransferViewModel, decimal>(static x => x.Draft.Amount);

        var written = Reflection.TrySetValueToPropertyChain(viewModel, chain, AmountValue);

        SampleCheck.Equal(true, written);
        SampleCheck.Equal(AmountValue, viewModel.Draft.Amount);
    }

    /// <summary>Writes the value at the end of a path and chooses whether a missing member throws.</summary>
    public static void WriteTheValueAtTheEndOfAPathWithoutThrowing()
    {
        var item = new TodoItem { Tags = [FirstTag] };
        var chain = CreateChain<TodoItem, string>(static x => x.Title);

        var written = Reflection.TrySetValueToPropertyChain(item, chain, RenamedTitle, false);

        SampleCheck.Equal(true, written);
        SampleCheck.Equal(RenamedTitle, item.Title);
    }

    /// <summary>Builds the links of a path from the expression the way the runtime fallback does.</summary>
    /// <typeparam name="TRoot">The type the path starts from.</typeparam>
    /// <typeparam name="TValue">The type of the value the path ends at.</typeparam>
    /// <param name="path">The path to split.</param>
    /// <returns>The links of the path.</returns>
    private static List<Expression> CreateChain<TRoot, TValue>(Expression<Func<TRoot, TValue>> path) =>
        [.. Reflection.Rewrite(path.Body).GetExpressionChain()];

    /// <summary>Runs an action and reports whether it throws the given exception.</summary>
    /// <typeparam name="TException">The exception to look for.</typeparam>
    /// <param name="action">The action to run.</param>
    /// <returns><see langword="true"/> when the action throws <typeparamref name="TException"/>.</returns>
    private static bool Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return true;
        }

        return false;
    }
}
