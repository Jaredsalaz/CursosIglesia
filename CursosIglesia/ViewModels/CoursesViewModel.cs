using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.ViewModels.Base;

namespace CursosIglesia.ViewModels;

public class CoursesViewModel : ViewModelBase
{
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;

    private List<Course> _courses = new();
    public List<Course> Courses
    {
        get => _courses;
        set => SetProperty(ref _courses, value);
    }

    private List<Course> _filteredCourses = new();
    public List<Course> FilteredCourses
    {
        get => _filteredCourses;
        set => SetProperty(ref _filteredCourses, value);
    }

    private List<Category> _categories = new();
    public List<Category> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    private int? _selectedCategoryId;
    public int? SelectedCategoryId
    {
        get => _selectedCategoryId;
        set
        {
            if (SetProperty(ref _selectedCategoryId, value))
                ApplyFilters();
        }
    }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                ApplyFilters();
        }
    }

    private DifficultyLevel? _selectedDifficulty;
    public DifficultyLevel? SelectedDifficulty
    {
        get => _selectedDifficulty;
        set
        {
            if (SetProperty(ref _selectedDifficulty, value))
                ApplyFilters();
        }
    }

    private string _sortBy = "popular";
    public string SortBy
    {
        get => _sortBy;
        set
        {
            if (SetProperty(ref _sortBy, value))
                ApplyFilters();
        }
    }

    public CoursesViewModel(ICourseService courseService, ICategoryService categoryService)
    {
        _courseService = courseService;
        _categoryService = categoryService;
    }

    public override async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var coursesTask = _courseService.GetAllCoursesAsync();
            var categoriesTask = _categoryService.GetAllCategoriesAsync();

            await Task.WhenAll(coursesTask, categoriesTask);

            Courses = coursesTask.Result;
            Categories = categoriesTask.Result;
            ApplyFilters();
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

    public void ApplyFilters()
    {
        var filtered = Courses.AsEnumerable();

        if (SelectedCategoryId.HasValue && SelectedCategoryId.Value > 0)
            filtered = filtered.Where(c => c.CategoryId == SelectedCategoryId.Value);

        if (!string.IsNullOrWhiteSpace(SearchQuery))
            filtered = filtered.Where(c =>
                c.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                c.CategoryName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                c.Instructor.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

        if (SelectedDifficulty.HasValue)
            filtered = filtered.Where(c => c.Difficulty == SelectedDifficulty.Value);

        filtered = SortBy switch
        {
            "recent" => filtered.OrderByDescending(c => c.CreatedDate),
            "rating" => filtered.OrderByDescending(c => c.Rating),
            "name" => filtered.OrderBy(c => c.Title),
            "price-low" => filtered.OrderBy(c => c.Price),
            "price-high" => filtered.OrderByDescending(c => c.Price),
            _ => filtered.OrderByDescending(c => c.StudentsEnrolled) // popular
        };

        FilteredCourses = filtered.ToList();
    }

    public void ClearFilters()
    {
        _searchQuery = string.Empty;
        _selectedCategoryId = null;
        _selectedDifficulty = null;
        _sortBy = "popular";
        OnPropertyChanged(nameof(SearchQuery));
        OnPropertyChanged(nameof(SelectedCategoryId));
        OnPropertyChanged(nameof(SelectedDifficulty));
        OnPropertyChanged(nameof(SortBy));
        ApplyFilters();
    }
}
