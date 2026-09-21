// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>
/// A student records service that lives in memory. It refuses a request the way a real registrar does: a full
/// course, a timetable clash, a score out of range. Time comes from a <see cref="ManualClock"/>, and the
/// <see cref="Gate"/> decides when each response arrives. It hands out copies, so changing a student you read
/// does not change the records.
/// </summary>
/// <param name="clock">The clock that stamps submissions and grades.</param>
[System.Diagnostics.DebuggerDisplay("Students = {_students.Count}, Enrolments = {_enrolments.Count}, IsUnavailable = {IsUnavailable}")]
public sealed class InMemoryStudentRecords(ManualClock clock) : IStudentRecords
{
    /// <summary>The courses a new service starts with. CS101 has three places, and HIST110 meets while CS101 does.</summary>
    private static readonly Course[] _seedCourses =
    [
        new("CS101", "Introduction to Programming", 3, DayOfWeek.Tuesday, 9, 2, 50),
        new("MATH201", "Linear Algebra", 30, DayOfWeek.Wednesday, 10, 2, 50),
        new("HIST110", "Modern European History", 30, DayOfWeek.Tuesday, 10, 2, 50),
    ];

    /// <summary>The assignments a new service starts with.</summary>
    private static readonly Assignment[] _seedAssignments =
    [
        new(1, "CS101", "Loops and functions", 20, 100, SeedData.Date("2026-03-06")),
        new(2, "CS101", "Text adventure", 30, 100, SeedData.Date("2026-03-27")),
        new(3, "CS101", "Final project", 50, 100, SeedData.Date("2026-05-29")),
        new(4, "MATH201", "Problem set 1", 40, 50, SeedData.Date("2026-03-13")),
        new(5, "MATH201", "Midterm", 60, 100, SeedData.Date("2026-04-10")),
        new(6, "HIST110", "Essay on the 1848 revolutions", 100, 100, SeedData.Date("2026-04-17")),
    ];

    /// <summary>The students a new service starts with.</summary>
    private static readonly (int Id, string Name)[] _seedStudents =
    [
        (1, "Aisha Rahman"),
        (2, "Ben Carter"),
        (3, "Chloe Nguyen"),
        (4, "Diego Alvarez"),
        (5, "Emma Kowalski"),
    ];

    /// <summary>The enrolments a new service starts with: Aisha, Ben and Chloe fill CS101.</summary>
    private static readonly Enrolment[] _seedEnrolments =
    [
        new(
            1,
            "CS101",
            SeedData.Date("2026-02-16"),
            [new(1, 1, 85, SeedData.Instant("2026-03-09T12:00:00Z")), new(1, 2, 72, SeedData.Instant("2026-03-30T12:00:00Z"))],
            [new(1, 1, SeedData.Instant("2026-03-05T10:00:00Z"), false)]),
        new(2, "CS101", SeedData.Date("2026-02-16"), [new(2, 1, 40, SeedData.Instant("2026-03-09T12:00:00Z")), new(2, 2, 35, SeedData.Instant("2026-03-30T12:00:00Z"))], []),
        new(3, "CS101", SeedData.Date("2026-02-17"), [], []),
        new(3, "MATH201", SeedData.Date("2026-02-17"), [new(3, 4, 45, SeedData.Instant("2026-03-16T12:00:00Z"))], []),
        new(4, "HIST110", SeedData.Date("2026-02-18"), [], []),
        new(5, "MATH201", SeedData.Date("2026-02-18"), [new(5, 4, 30, SeedData.Instant("2026-03-16T12:00:00Z"))], []),
    ];

    /// <summary>The courses, by code.</summary>
    private readonly List<Course> _courses = [.. _seedCourses];

    /// <summary>The assignments, by identifier.</summary>
    private readonly List<Assignment> _assignments = [.. _seedAssignments];

    /// <summary>The students, without their enrolments.</summary>
    private readonly List<Student> _students = [];

    /// <summary>The enrolments of every student. An enrolment is replaced, not changed, when its grades or submissions change.</summary>
    private readonly List<Enrolment> _enrolments = [.. _seedEnrolments];

    /// <summary>Gets the gate that decides when each response arrives.</summary>
    public ResponseGate Gate { get; } = new();

    /// <summary>Gets or sets a value indicating whether every call fails with <see cref="RecordsFailure.ServiceUnavailable"/>.</summary>
    public bool IsUnavailable { get; set; }

    /// <summary>Creates a service with three courses, five students and six assignments.</summary>
    /// <param name="clock">The clock that stamps submissions and grades.</param>
    /// <returns>A new service.</returns>
    public static InMemoryStudentRecords CreateSeeded(ManualClock clock)
    {
        InMemoryStudentRecords records = new(clock);
        foreach (var (id, name) in _seedStudents)
        {
            records._students.Add(new() { Id = id, Name = name, Email = $"student{id}@example.edu" });
        }

        return records;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Course>> GetCoursesAsync()
    {
        await EnterAsync().ConfigureAwait(false);

        return [.. _courses];
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Student>> GetStudentsAsync()
    {
        await EnterAsync().ConfigureAwait(false);

        List<Student> students = [];
        foreach (var student in _students)
        {
            students.Add(Snapshot(student));
        }

        return students;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Student>> GetRosterAsync(string courseCode)
    {
        await EnterAsync().ConfigureAwait(false);

        _ = FindCourse(courseCode);
        List<Student> roster = [];
        foreach (var student in _students)
        {
            if (IndexOfEnrolment(student.Id, courseCode) >= 0)
            {
                roster.Add(Snapshot(student));
            }
        }

        return roster;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Assignment>> GetAssignmentsAsync(string courseCode)
    {
        await EnterAsync().ConfigureAwait(false);

        _ = FindCourse(courseCode);
        List<Assignment> assignments = [];
        foreach (var assignment in _assignments)
        {
            if (assignment.CourseCode == courseCode)
            {
                assignments.Add(assignment);
            }
        }

        return assignments;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Enrolment>> GetEnrolmentsAsync(int studentId)
    {
        await EnterAsync().ConfigureAwait(false);

        return EnrolmentsOf(FindStudent(studentId).Id);
    }

    /// <inheritdoc/>
    public async Task<Enrolment> EnrolAsync(int studentId, string courseCode)
    {
        await EnterAsync().ConfigureAwait(false);

        var student = FindStudent(studentId);
        var course = FindCourse(courseCode);

        if (IndexOfEnrolment(student.Id, course.Code) >= 0)
        {
            throw new StudentRecordsException(RecordsFailure.AlreadyEnrolled, $"{student.Name} is already enrolled in {course.Code}.");
        }

        if (CountEnrolled(course.Code) >= course.Capacity)
        {
            throw new StudentRecordsException(RecordsFailure.CourseFull, $"{course.Code} has no places left.");
        }

        foreach (var existing in EnrolmentsOf(student.Id))
        {
            var other = FindCourse(existing.CourseCode);
            if (course.ClashesWith(other))
            {
                throw new StudentRecordsException(RecordsFailure.TimetableClash, $"{course.Code} meets at the same time as {other.Code}.");
            }
        }

        Enrolment enrolment = new(student.Id, course.Code, Today(), [], []);
        _enrolments.Add(enrolment);
        return enrolment;
    }

    /// <inheritdoc/>
    public async Task DropAsync(int studentId, string courseCode)
    {
        await EnterAsync().ConfigureAwait(false);

        var student = FindStudent(studentId);
        var course = FindCourse(courseCode);
        var index = IndexOfEnrolment(student.Id, course.Code);
        if (index < 0)
        {
            throw new StudentRecordsException(RecordsFailure.NotEnrolled, $"{student.Name} is not enrolled in {course.Code}.");
        }

        _enrolments.RemoveAt(index);
    }

    /// <inheritdoc/>
    public async Task<Submission> SubmitAssignmentAsync(int studentId, int assignmentId)
    {
        await EnterAsync().ConfigureAwait(false);

        var assignment = FindAssignment(assignmentId);
        var index = RequireEnrolment(studentId, assignment.CourseCode);
        var enrolment = _enrolments[index];

        var now = clock.GetUtcNow();
        Submission submission = new(studentId, assignmentId, now, DateOnly.FromDateTime(now.UtcDateTime) > assignment.DueDate);
        List<Submission> submissions = [.. enrolment.Submissions];
        _ = submissions.RemoveAll(existing => existing.AssignmentId == assignmentId);
        submissions.Add(submission);
        _enrolments[index] = enrolment with { Submissions = submissions };
        return submission;
    }

    /// <inheritdoc/>
    public async Task<Grade> RecordGradeAsync(int studentId, int assignmentId, decimal score)
    {
        await EnterAsync().ConfigureAwait(false);

        var assignment = FindAssignment(assignmentId);
        var index = RequireEnrolment(studentId, assignment.CourseCode);
        if (score < 0 || score > assignment.MaxScore)
        {
            throw new StudentRecordsException(RecordsFailure.ScoreOutOfRange, $"A score for '{assignment.Title}' must be between 0 and {assignment.MaxScore}.");
        }

        var enrolment = _enrolments[index];
        Grade grade = new(studentId, assignmentId, score, clock.GetUtcNow());
        List<Grade> grades = [.. enrolment.Grades];
        _ = grades.RemoveAll(existing => existing.AssignmentId == assignmentId);
        grades.Add(grade);
        _enrolments[index] = enrolment with { Grades = grades };
        return grade;
    }

    /// <summary>Waits for the gate, then fails when the service is unavailable.</summary>
    /// <returns>A task that completes when the call may proceed.</returns>
    /// <exception cref="StudentRecordsException">The service is unavailable.</exception>
    private async Task EnterAsync()
    {
        await Gate.WaitAsync().ConfigureAwait(false);

        if (IsUnavailable)
        {
            throw new StudentRecordsException(RecordsFailure.ServiceUnavailable, "The student records service is unavailable.");
        }
    }

    /// <summary>Gets today's date from the clock.</summary>
    /// <returns>The date in UTC.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DateOnly Today() => DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

    /// <summary>Copies a student together with the courses the student takes.</summary>
    /// <param name="student">The stored student.</param>
    /// <returns>An independent copy.</returns>
    private Student Snapshot(Student student)
    {
        var copy = student.Clone();
        copy.Enrolments = EnrolmentsOf(student.Id);
        return copy;
    }

    /// <summary>Lists the enrolments of a student.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <returns>The enrolments, in the order they were made.</returns>
    private List<Enrolment> EnrolmentsOf(int studentId)
    {
        List<Enrolment> enrolments = [];
        foreach (var enrolment in _enrolments)
        {
            if (enrolment.StudentId == studentId)
            {
                enrolments.Add(enrolment);
            }
        }

        return enrolments;
    }

    /// <summary>Finds where an enrolment is stored.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>The index of the enrolment, or -1 when the student is not enrolled.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int IndexOfEnrolment(int studentId, string courseCode) =>
        _enrolments.FindIndex(enrolment => enrolment.StudentId == studentId && enrolment.CourseCode == courseCode);

    /// <summary>Finds where an enrolment is stored, and refuses when there is none.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>The index of the enrolment.</returns>
    /// <exception cref="StudentRecordsException">The student does not exist or is not enrolled.</exception>
    private int RequireEnrolment(int studentId, string courseCode)
    {
        var student = FindStudent(studentId);
        var index = IndexOfEnrolment(student.Id, courseCode);
        return index >= 0
            ? index
            : throw new StudentRecordsException(RecordsFailure.NotEnrolled, $"{student.Name} is not enrolled in {courseCode}.");
    }

    /// <summary>Counts the students enrolled in a course.</summary>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>The number of enrolments.</returns>
    private int CountEnrolled(string courseCode)
    {
        var count = 0;
        foreach (var enrolment in _enrolments)
        {
            if (enrolment.CourseCode == courseCode)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Finds a course.</summary>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>The course.</returns>
    /// <exception cref="StudentRecordsException">The course does not exist.</exception>
    private Course FindCourse(string courseCode) =>
        _courses.Find(course => course.Code == courseCode)
        ?? throw new StudentRecordsException(RecordsFailure.UnknownCourse, $"There is no course {courseCode}.");

    /// <summary>Finds a student.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <returns>The stored student.</returns>
    /// <exception cref="StudentRecordsException">The student does not exist.</exception>
    private Student FindStudent(int studentId) =>
        _students.Find(student => student.Id == studentId)
        ?? throw new StudentRecordsException(RecordsFailure.UnknownStudent, $"There is no student {studentId}.");

    /// <summary>Finds an assignment.</summary>
    /// <param name="assignmentId">The identifier of the assignment.</param>
    /// <returns>The assignment.</returns>
    /// <exception cref="StudentRecordsException">The assignment does not exist.</exception>
    private Assignment FindAssignment(int assignmentId) =>
        _assignments.Find(assignment => assignment.Id == assignmentId)
        ?? throw new StudentRecordsException(RecordsFailure.UnknownAssignment, $"There is no assignment {assignmentId}.");
}
