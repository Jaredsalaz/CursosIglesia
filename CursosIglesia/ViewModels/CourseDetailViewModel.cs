using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.ViewModels.Base;

namespace CursosIglesia.ViewModels;

public class CourseDetailViewModel : ViewModelBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ITestimonialService _testimonialService;

    private Course? _course;
    public Course? Course
    {
        get => _course;
        set => SetProperty(ref _course, value);
    }

    private List<Testimonial> _testimonials = new();
    public List<Testimonial> Testimonials
    {
        get => _testimonials;
        set => SetProperty(ref _testimonials, value);
    }

    private string _newComment = string.Empty;
    public string NewComment
    {
        get => _newComment;
        set => SetProperty(ref _newComment, value);
    }

    private int _newRating = 5;
    public int NewRating
    {
        get => _newRating;
        set => SetProperty(ref _newRating, value);
    }

    private bool _isSubmittingTestimonial;
    public bool IsSubmittingTestimonial
    {
        get => _isSubmittingTestimonial;
        set => SetProperty(ref _isSubmittingTestimonial, value);
    }

    private string? _testimonialSuccessMessage;
    public string? TestimonialSuccessMessage
    {
        get => _testimonialSuccessMessage;
        set => SetProperty(ref _testimonialSuccessMessage, value);
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

    public CourseDetailViewModel(ICourseService courseService, IEnrollmentService enrollmentService, ITestimonialService testimonialService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _testimonialService = testimonialService;
    }

    public async Task LoadCourseAsync(Guid courseId)
    {
        IsLoading = true;
        ErrorMessage = null;
        TestimonialSuccessMessage = null;

        try
        {
            Course = await _courseService.GetCourseByIdAsync(courseId);

            if (Course != null)
            {
                var enrolledTask = _enrollmentService.IsEnrolledAsync(courseId);
                var categoryCoursesTask = _courseService.GetCoursesByCategoryAsync(Course.CategoryId);
                var testimonialsTask = _testimonialService.GetByCourseIdAsync(courseId);

                await Task.WhenAll(enrolledTask, categoryCoursesTask, testimonialsTask);

                IsEnrolled = await enrolledTask;
                var categoryCourses = await categoryCoursesTask;
                RelatedCourses = categoryCourses.Where(c => c.Id != courseId).Take(3).ToList();
                Testimonials = await testimonialsTask;
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

    public async Task SubmitTestimonialAsync()
    {
        if (Course == null) return;
        if (string.IsNullOrWhiteSpace(NewComment) || NewComment.Length < 10)
        {
            ErrorMessage = "El comentario debe tener al menos 10 caracteres.";
            return;
        }

        IsSubmittingTestimonial = true;
        ErrorMessage = null;
        TestimonialSuccessMessage = null;

        try
        {
            var success = await _testimonialService.AddTestimonialAsync(Course.Id, NewComment, NewRating);
            if (success)
            {
                TestimonialSuccessMessage = "¡Gracias! Tu testimonio ha sido enviado y aparecerá una vez sea aprobado.";
                NewComment = string.Empty;
                NewRating = 5;
            }
            else
            {
                ErrorMessage = "No se pudo enviar el testimonio. Inténtalo de nuevo.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al enviar testimonio: {ex.Message}";
        }
        finally
        {
            IsSubmittingTestimonial = false;
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
                // Reload to sync state
                await LoadCourseAsync(Course.Id);
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
