using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.ViewModels.Base;

namespace CursosIglesia.ViewModels;

public class CourseDetailViewModel : ViewModelBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;

    private Course? _course;
    public Course? Course
    {
        get => _course;
        set => SetProperty(ref _course, value);
    }

    private List<Course> _relatedCourses = new();
    public List<Course> RelatedCourses
    {
        get => _relatedCourses;
        set => SetProperty(ref _relatedCourses, value);
    }

    private bool _showAllLessons;
    public bool ShowAllLessons
    {
        get => _showAllLessons;
        set => SetProperty(ref _showAllLessons, value);
    }

    private bool _isEnrolled;
    public bool IsEnrolled
    {
        get => _isEnrolled;
        set => SetProperty(ref _isEnrolled, value);
    }

    private bool _isEnrolling;
    public bool IsEnrolling
    {
        get => _isEnrolling;
        set => SetProperty(ref _isEnrolling, value);
    }

    public List<Lesson> VisibleLessons => ShowAllLessons
        ? Course?.Lessons ?? new()
        : (Course?.Lessons.Take(5).ToList() ?? new());

    public CourseDetailViewModel(ICourseService courseService, IEnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    public async Task LoadCourseAsync(Guid courseId)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Course = await _courseService.GetCourseByIdAsync(courseId);

            if (Course != null)
            {
                var enrolledTask = _enrollmentService.IsEnrolledAsync(courseId);
                var categoryCourses = await _courseService.GetCoursesByCategoryAsync(Course.CategoryId);
                RelatedCourses = categoryCourses.Where(c => c.Id != courseId).Take(3).ToList();
                IsEnrolled = await enrolledTask;
            }
            else
            {
                ErrorMessage = "Curso no encontrado.";
            }
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

    public async Task EnrollAsync()
    {
        if (Course == null) return;

        IsEnrolling = true;
        try
        {
            var result = await _enrollmentService.EnrollAsync(Course.Id);
            if (result)
            {
                IsEnrolled = true;
                // Set first lesson as current
                var firstLesson = Course.Lessons.OrderBy(l => l.Order).FirstOrDefault();
                if (firstLesson != null)
                {
                    await _enrollmentService.SetCurrentLessonAsync(Course.Id, firstLesson.Id);
                }
            }
        }
        finally
        {
            IsEnrolling = false;
        }
    }

    public void ToggleLessons()
    {
        ShowAllLessons = !ShowAllLessons;
        OnPropertyChanged(nameof(VisibleLessons));
    }
}
