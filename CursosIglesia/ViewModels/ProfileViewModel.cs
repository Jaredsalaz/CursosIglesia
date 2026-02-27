using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.ViewModels.Base;

namespace CursosIglesia.ViewModels;

public class ProfileViewModel : ViewModelBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICourseService _courseService;

    private List<EnrolledCourseInfo> _enrolledCourses = new();
    public List<EnrolledCourseInfo> EnrolledCourses
    {
        get => _enrolledCourses;
        set => SetProperty(ref _enrolledCourses, value);
    }

    private int _totalEnrolled;
    public int TotalEnrolled
    {
        get => _totalEnrolled;
        set => SetProperty(ref _totalEnrolled, value);
    }

    private double _averageProgress;
    public double AverageProgress
    {
        get => _averageProgress;
        set => SetProperty(ref _averageProgress, value);
    }

    private int _completedCourses;
    public int CompletedCourses
    {
        get => _completedCourses;
        set => SetProperty(ref _completedCourses, value);
    }

    public ProfileViewModel(IEnrollmentService enrollmentService, ICourseService courseService)
    {
        _enrollmentService = enrollmentService;
        _courseService = courseService;
    }

    public override async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var enrollments = await _enrollmentService.GetEnrollmentsAsync();
            var allCourses = await _courseService.GetAllCoursesAsync();

            var enrolledList = new List<EnrolledCourseInfo>();
            foreach (var enrollment in enrollments)
            {
                var course = allCourses.FirstOrDefault(c => c.Id == enrollment.CourseId);
                if (course != null)
                {
                    enrolledList.Add(new EnrolledCourseInfo
                    {
                        Course = course,
                        Enrollment = enrollment
                    });
                }
            }

            EnrolledCourses = enrolledList.OrderByDescending(e => e.Enrollment.EnrolledDate).ToList();
            TotalEnrolled = enrolledList.Count;
            CompletedCourses = enrolledList.Count(e => e.Enrollment.IsCompleted);
            AverageProgress = enrolledList.Any()
                ? Math.Round(enrolledList.Average(e => e.Enrollment.Progress), 1)
                : 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar los cursos: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task UnenrollAsync(int courseId)
    {
        await _enrollmentService.UnenrollAsync(courseId);
        await InitializeAsync();
    }
}

public class EnrolledCourseInfo
{
    public Course Course { get; set; } = default!;
    public Enrollment Enrollment { get; set; } = default!;
}
