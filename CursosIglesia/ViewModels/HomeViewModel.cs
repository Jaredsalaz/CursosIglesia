using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.ViewModels.Base;

namespace CursosIglesia.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private readonly ITestimonialService _testimonialService;

    private List<Course> _featuredCourses = new();
    public List<Course> FeaturedCourses
    {
        get => _featuredCourses;
        set => SetProperty(ref _featuredCourses, value);
    }

    private List<Course> _popularCourses = new();
    public List<Course> PopularCourses
    {
        get => _popularCourses;
        set => SetProperty(ref _popularCourses, value);
    }

    private List<Course> _recentCourses = new();
    public List<Course> RecentCourses
    {
        get => _recentCourses;
        set => SetProperty(ref _recentCourses, value);
    }

    private List<Category> _categories = new();
    public List<Category> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    private List<Testimonial> _testimonials = new();
    public List<Testimonial> Testimonials
    {
        get => _testimonials;
        set => SetProperty(ref _testimonials, value);
    }

    private int _totalStudents;
    public int TotalStudents
    {
        get => _totalStudents;
        set => SetProperty(ref _totalStudents, value);
    }

    private int _totalCourses;
    public int TotalCourses
    {
        get => _totalCourses;
        set => SetProperty(ref _totalCourses, value);
    }

    public HomeViewModel(ICourseService courseService, ICategoryService categoryService, ITestimonialService testimonialService)
    {
        _courseService = courseService;
        _categoryService = categoryService;
        _testimonialService = testimonialService;
    }

    public override async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var featuredTask = _courseService.GetFeaturedCoursesAsync();
            var popularTask = _courseService.GetPopularCoursesAsync(6);
            var recentTask = _courseService.GetRecentCoursesAsync(6);
            var categoriesTask = _categoryService.GetAllCategoriesAsync();
            var testimonialsTask = _testimonialService.GetTestimonialsAsync(5);
            var allCoursesTask = _courseService.GetAllCoursesAsync();

            await Task.WhenAll(featuredTask, popularTask, recentTask, categoriesTask, testimonialsTask, allCoursesTask);

            FeaturedCourses = featuredTask.Result;
            PopularCourses = popularTask.Result;
            RecentCourses = recentTask.Result;
            Categories = categoriesTask.Result;
            Testimonials = testimonialsTask.Result;

            var allCourses = allCoursesTask.Result;
            TotalCourses = allCourses.Count;
            TotalStudents = allCourses.Sum(c => c.StudentsEnrolled);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar los datos: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
