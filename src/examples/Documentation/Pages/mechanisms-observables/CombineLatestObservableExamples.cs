// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Documentation.MechanismsObservables;

/// <summary>
/// Combines two to sixteen sources with <see cref="CombineLatestObservable"/>. It delivers a result once every
/// source has produced a value, and again each time any source produces a new one.
/// </summary>
public static class CombineLatestObservableExamples
{
    /// <summary>The title of the registration task.</summary>
    private const string RegistrationTitle = "Renew car registration";

    /// <summary>The title the registration task is renamed to.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The prefix of a finished checklist line.</summary>
    private const string DoneMark = "[x] ";

    /// <summary>The prefix of an open checklist line.</summary>
    private const string OpenMark = "[ ] ";

    /// <summary>The value every completed field of a form contributes.</summary>
    private const int CompletedField = 1;

    /// <summary>The number of fields combined by <see cref="CombineFourFields"/>.</summary>
    private const int FourFields = 4;

    /// <summary>The number of fields combined by <see cref="CombineFiveFields"/>.</summary>
    private const int FiveFields = 5;

    /// <summary>The number of fields combined by <see cref="CombineSixFields"/>.</summary>
    private const int SixFields = 6;

    /// <summary>The number of fields combined by <see cref="CombineSevenFields"/>.</summary>
    private const int SevenFields = 7;

    /// <summary>The number of fields combined by <see cref="CombineEightFields"/>.</summary>
    private const int EightFields = 8;

    /// <summary>The number of fields combined by <see cref="CombineNineFields"/>.</summary>
    private const int NineFields = 9;

    /// <summary>The number of fields combined by <see cref="CombineTenFields"/>.</summary>
    private const int TenFields = 10;

    /// <summary>The number of fields combined by <see cref="CombineElevenFields"/>.</summary>
    private const int ElevenFields = 11;

    /// <summary>The number of fields combined by <see cref="CombineTwelveFields"/>.</summary>
    private const int TwelveFields = 12;

    /// <summary>The number of fields combined by <see cref="CombineThirteenFields"/>.</summary>
    private const int ThirteenFields = 13;

    /// <summary>The number of fields combined by <see cref="CombineFourteenFields"/>.</summary>
    private const int FourteenFields = 14;

    /// <summary>The number of fields combined by <see cref="CombineFifteenFields"/>.</summary>
    private const int FifteenFields = 15;

    /// <summary>The number of fields combined by <see cref="CombineSixteenFields"/>.</summary>
    private const int SixteenFields = 16;

    /// <summary>Combines the title and the done flag of a to-do item into a checklist line.</summary>
    public static void CombineTwoSources()
    {
        TodoItem item = new() { Title = RegistrationTitle };
        CapturingObserver<string> lines = new();

        using (CombineLatestObservable.Create(Title(item), IsDone(item), static (title, isDone) => (isDone ? DoneMark : OpenMark) + title).Subscribe(lines))
        {
            item.IsDone = true;
            item.Title = RenamedTitle;
        }

        SampleCheck.SequenceEqual([OpenMark + RegistrationTitle, DoneMark + RegistrationTitle, DoneMark + RenamedTitle], lines.Values);
    }

    /// <summary>Combines the title, the done flag and the priority of a to-do item into one summary.</summary>
    public static void CombineThreeSources()
    {
        TodoItem item = new() { Title = RegistrationTitle };
        CapturingObserver<string> summaries = new();

        using (CombineLatestObservable.Create(Title(item), IsDone(item), Priority(item), static (title, isDone, priority) => $"{title} {isDone} {priority}").Subscribe(summaries))
        {
            item.Priority = TodoPriority.High;
        }

        SampleCheck.SequenceEqual([$"{RegistrationTitle} False Normal", $"{RegistrationTitle} False High"], summaries.Values);
    }

    /// <summary>Counts the completed fields of a form with four fields.</summary>
    public static void CombineFourFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4) => f1 + f2 + f3 + f4));

        SampleCheck.SequenceEqual([FourFields], counts);
    }

    /// <summary>Counts the completed fields of a form with five fields.</summary>
    public static void CombineFiveFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4, f5) => f1 + f2 + f3 + f4 + f5));

        SampleCheck.SequenceEqual([FiveFields], counts);
    }

    /// <summary>Counts the completed fields of a form with six fields.</summary>
    public static void CombineSixFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4, f5, f6) => f1 + f2 + f3 + f4 + f5 + f6));

        SampleCheck.SequenceEqual([SixFields], counts);
    }

    /// <summary>Counts the completed fields of a form with seven fields.</summary>
    public static void CombineSevenFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4, f5, f6, f7) => f1 + f2 + f3 + f4 + f5 + f6 + f7));

        SampleCheck.SequenceEqual([SevenFields], counts);
    }

    /// <summary>Counts the completed fields of a form with eight fields.</summary>
    public static void CombineEightFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4, f5, f6, f7, f8) => f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8));

        SampleCheck.SequenceEqual([EightFields], counts);
    }

    /// <summary>Counts the completed fields of a form with nine fields.</summary>
    public static void CombineNineFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4, f5, f6, f7, f8, f9) => f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9));

        SampleCheck.SequenceEqual([NineFields], counts);
    }

    /// <summary>Counts the completed fields of a form with ten fields.</summary>
    public static void CombineTenFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10) => f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10));

        SampleCheck.SequenceEqual([TenFields], counts);
    }

    /// <summary>Counts the completed fields of a form with eleven fields.</summary>
    public static void CombineElevenFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11) => f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11));

        SampleCheck.SequenceEqual([ElevenFields], counts);
    }

    /// <summary>Counts the completed fields of a form with twelve fields.</summary>
    public static void CombineTwelveFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12) => f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12));

        SampleCheck.SequenceEqual([TwelveFields], counts);
    }

    /// <summary>Counts the completed fields of a form with thirteen fields.</summary>
    public static void CombineThirteenFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13) =>
                    f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12 + f13));

        SampleCheck.SequenceEqual([ThirteenFields], counts);
    }

    /// <summary>Counts the completed fields of a form with fourteen fields.</summary>
    public static void CombineFourteenFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14) =>
                    f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12 + f13 + f14));

        SampleCheck.SequenceEqual([FourteenFields], counts);
    }

    /// <summary>Counts the completed fields of a form with fifteen fields.</summary>
    public static void CombineFifteenFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14, f15) =>
                    f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12 + f13 + f14 + f15));

        SampleCheck.SequenceEqual([FifteenFields], counts);
    }

    /// <summary>Counts the completed fields of a form with sixteen fields.</summary>
    public static void CombineSixteenFields()
    {
        var counts = Collect(
            CombineLatestObservable.Create(
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                Field(),
                static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14, f15, f16) =>
                    f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12 + f13 + f14 + f15 + f16));

        SampleCheck.SequenceEqual([SixteenFields], counts);
    }

    /// <summary>Creates a form field that is complete and never changes.</summary>
    /// <returns>The field.</returns>
    private static UnchangingPropertyObservable<int> Field() => new(CompletedField);

    /// <summary>Observes the title of a to-do item.</summary>
    /// <param name="item">The item to observe.</param>
    /// <returns>The title now and after each change.</returns>
    private static PropertyObservable<string> Title(TodoItem item) => new(item, nameof(TodoItem.Title), static source => ((TodoItem)source).Title, true);

    /// <summary>Observes the done flag of a to-do item.</summary>
    /// <param name="item">The item to observe.</param>
    /// <returns>The flag now and after each change.</returns>
    private static PropertyObservable<bool> IsDone(TodoItem item) => new(item, nameof(TodoItem.IsDone), static source => ((TodoItem)source).IsDone, true);

    /// <summary>Observes the priority of a to-do item.</summary>
    /// <param name="item">The item to observe.</param>
    /// <returns>The priority now and after each change.</returns>
    private static PropertyObservable<TodoPriority> Priority(TodoItem item) => new(item, nameof(TodoItem.Priority), static source => ((TodoItem)source).Priority, true);

    /// <summary>Subscribes, keeps what the sequence produced while subscribing, and unsubscribes.</summary>
    /// <param name="source">The sequence to read.</param>
    /// <returns>The values the sequence produced.</returns>
    private static List<int> Collect(IObservable<int> source)
    {
        CapturingObserver<int> observer = new();

        using var subscription = source.Subscribe(observer);

        return [.. observer.Values];
    }
}
