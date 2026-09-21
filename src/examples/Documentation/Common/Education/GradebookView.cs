// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>
/// The gradebook screen: a course list, the roster of the selected course, the assignments and a grade entry
/// area. A real UI framework builds these controls from markup; here the view creates them in code.
/// </summary>
[System.Diagnostics.DebuggerDisplay("ViewModel = {ViewModel}")]
public sealed class GradebookView : ObservableObject, IViewFor<GradebookViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public GradebookViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the list of courses.</summary>
    public ItemsListControl<Course> CourseList { get; } = new();

    /// <summary>Gets the list of students in the selected course.</summary>
    public ItemsListControl<Student> RosterList { get; } = new();

    /// <summary>Gets the list of students who could join the selected course.</summary>
    public ItemsListControl<Student> CandidateList { get; } = new();

    /// <summary>Gets the list of assignments of the selected course.</summary>
    public ItemsListControl<Assignment> AssignmentList { get; } = new();

    /// <summary>Gets the button that enrols the chosen candidate.</summary>
    public ButtonControl EnrolButton { get; } = new() { Content = "Enrol" };

    /// <summary>Gets the button that drops the selected student.</summary>
    public ButtonControl DropButton { get; } = new() { Content = "Drop" };

    /// <summary>Gets the box where the teacher types a score.</summary>
    public TextBoxControl ScoreTextBox { get; } = new() { Placeholder = "Score" };

    /// <summary>Gets the button that records the score.</summary>
    public ButtonControl RecordGradeButton { get; } = new() { Content = "Record grade" };

    /// <summary>Gets the label that shows the name of the selected student.</summary>
    public LabelControl StudentNameLabel { get; } = new();

    /// <summary>Gets the label that shows the weighted average of the selected student.</summary>
    public LabelControl AverageLabel { get; } = new();

    /// <summary>Gets the bar that shows the weighted average of the selected student.</summary>
    public ProgressBarControl AverageBar { get; } = new();

    /// <summary>Gets the box that is ticked while the selected student passes.</summary>
    public CheckBoxControl PassCheckBox { get; } = new() { Content = "Passing" };

    /// <summary>Gets the label that shows the last error.</summary>
    public LabelControl ErrorLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (GradebookViewModel?)value;
    }
}
