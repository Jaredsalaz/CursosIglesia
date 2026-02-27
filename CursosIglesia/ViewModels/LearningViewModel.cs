using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.ViewModels.Base;

namespace CursosIglesia.ViewModels;

public class LearningViewModel : ViewModelBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;

    private Course? _course;
    public Course? Course
    {
        get => _course;
        set => SetProperty(ref _course, value);
    }

    private Enrollment? _enrollment;
    public Enrollment? Enrollment
    {
        get => _enrollment;
        set => SetProperty(ref _enrollment, value);
    }

    private Lesson? _currentLesson;
    public Lesson? CurrentLesson
    {
        get => _currentLesson;
        set => SetProperty(ref _currentLesson, value);
    }

    private bool _isCurrentLessonCompleted;
    public bool IsCurrentLessonCompleted
    {
        get => _isCurrentLessonCompleted;
        set => SetProperty(ref _isCurrentLessonCompleted, value);
    }

    private bool _isSidebarOpen = true;
    public bool IsSidebarOpen
    {
        get => _isSidebarOpen;
        set => SetProperty(ref _isSidebarOpen, value);
    }

    public double Progress => Enrollment?.Progress ?? 0;

    public int CompletedCount => Enrollment?.CompletedLessonIds.Count ?? 0;

    public int TotalLessons => Course?.Lessons.Count ?? 0;

    public LearningViewModel(ICourseService courseService, IEnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    public async Task LoadCourseAsync(int courseId)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Course = await _courseService.GetCourseByIdAsync(courseId);
            Enrollment = await _enrollmentService.GetEnrollmentAsync(courseId);

            if (Course == null)
            {
                ErrorMessage = "Curso no encontrado.";
                return;
            }

            if (Enrollment == null)
            {
                ErrorMessage = "No estás inscrito en este curso.";
                return;
            }

            // Set current lesson
            if (Enrollment.CurrentLessonId.HasValue)
            {
                CurrentLesson = Course.Lessons.FirstOrDefault(l => l.Id == Enrollment.CurrentLessonId.Value);
            }

            // If no current lesson set, start with the first one
            if (CurrentLesson == null && Course.Lessons.Any())
            {
                CurrentLesson = Course.Lessons.OrderBy(l => l.Order).First();
                await _enrollmentService.SetCurrentLessonAsync(courseId, CurrentLesson.Id);
            }

            UpdateCurrentLessonStatus();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar el curso: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SelectLessonAsync(int lessonId)
    {
        if (Course == null || Enrollment == null) return;

        var lesson = Course.Lessons.FirstOrDefault(l => l.Id == lessonId);
        if (lesson == null) return;

        CurrentLesson = lesson;
        await _enrollmentService.SetCurrentLessonAsync(Course.Id, lessonId);
        UpdateCurrentLessonStatus();
        NotifyProgressChanged();
    }

    public async Task CompleteCurrentLessonAsync()
    {
        if (Course == null || Enrollment == null || CurrentLesson == null) return;

        await _enrollmentService.CompleteLessonAsync(Course.Id, CurrentLesson.Id, Course.Lessons.Count);

        // Refresh enrollment data
        Enrollment = await _enrollmentService.GetEnrollmentAsync(Course.Id);

        UpdateCurrentLessonStatus();
        NotifyProgressChanged();
    }

    public async Task CompleteAndNextAsync()
    {
        if (Course == null || CurrentLesson == null) return;

        await CompleteCurrentLessonAsync();

        // Move to next lesson
        var nextLesson = Course.Lessons
            .Where(l => l.Order > CurrentLesson.Order)
            .OrderBy(l => l.Order)
            .FirstOrDefault();

        if (nextLesson != null)
        {
            await SelectLessonAsync(nextLesson.Id);
        }
    }

    public void ToggleSidebar()
    {
        IsSidebarOpen = !IsSidebarOpen;
    }

    public bool IsLessonCompleted(int lessonId)
        => Enrollment?.CompletedLessonIds.Contains(lessonId) ?? false;

    private void UpdateCurrentLessonStatus()
    {
        IsCurrentLessonCompleted = CurrentLesson != null &&
            (Enrollment?.CompletedLessonIds.Contains(CurrentLesson.Id) ?? false);
    }

    private void NotifyProgressChanged()
    {
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(TotalLessons));
    }
}
