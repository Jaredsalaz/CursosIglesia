using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.ViewModels.Base;

namespace CursosIglesia.ViewModels;

public class LearningViewModel : ViewModelBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IAuthService _authService;
    private readonly IActivityService _activityService;

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

    private Tema? _currentTopic;
    public Tema? CurrentTopic
    {
        get => _currentTopic;
        set => SetProperty(ref _currentTopic, value);
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

    private bool _hasUngradedActivities;
    public bool HasUngradedActivities
    {
        get => _hasUngradedActivities;
        set => SetProperty(ref _hasUngradedActivities, value);
    }

    public double Progress => Enrollment?.Progress ?? 0;

    public int CompletedCount => Enrollment?.CompletedTopicIds.Count ?? 0;

    public int TotalTopics => Course?.Lessons.SelectMany(l => l.Topics ?? new List<Tema>()).Count() ?? 0;

    public LearningViewModel(ICourseService courseService, IEnrollmentService enrollmentService, IAuthService authService, IActivityService activityService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _authService = authService;
        _activityService = activityService;
    }

    public async Task LoadCourseAsync(Guid courseId)
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

            // Set current lesson/topic
            if (Enrollment.CurrentLessonId.HasValue && Enrollment.CurrentLessonId.Value != Guid.Empty)
            {
                CurrentLesson = Course.Lessons.FirstOrDefault(l => l.Id == Enrollment.CurrentLessonId.Value);
            }

            if (CurrentLesson == null && Course.Lessons.Any())
            {
                CurrentLesson = Course.Lessons.OrderBy(l => l.Order)
                                              .FirstOrDefault(l => !IsLessonLocked(l.Id)) ?? Course.Lessons.OrderBy(l => l.Order).First();
            }

            // Select first topic of current lesson if none selected
            if (CurrentTopic == null && CurrentLesson != null && CurrentLesson.Topics != null && CurrentLesson.Topics.Any())
            {
                CurrentTopic = CurrentLesson.Topics.OrderBy(t => t.Order).First();
            }

            UpdateCurrentLessonStatus();
            await CheckUngradedActivitiesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar el curso: {ex.Message}";
        }
        finally
        {
            OnPropertyChanged(nameof(Progress));
            IsLoading = false;
        }
    }

    public async Task SelectLessonAsync(Guid lessonId)
    {
        if (Course == null || Enrollment == null) return;
        if (IsLessonLocked(lessonId)) return;

        var lesson = Course.Lessons.FirstOrDefault(l => l.Id == lessonId);
        if (lesson == null) return;

        CurrentLesson = lesson;
        await _enrollmentService.SetCurrentLessonAsync(Course.Id, lessonId);
        
        // Auto-select first topic of the lesson
        if (lesson.Topics != null && lesson.Topics.Any())
        {
            CurrentTopic = lesson.Topics.OrderBy(t => t.Order).First();
            await _enrollmentService.SetCurrentTopicAsync(Course.Id, CurrentTopic.Id);
        }
        else
        {
            CurrentTopic = null;
        }

        UpdateCurrentLessonStatus();
        NotifyProgressChanged();
    }

    public async Task SelectTopicAsync(Tema topic)
    {
        if (IsTopicLocked(topic.Id)) return;
        
        CurrentTopic = topic;
        if (topic.LessonId != CurrentLesson?.Id)
        {
            CurrentLesson = Course?.Lessons.FirstOrDefault(l => l.Id == topic.LessonId);
        }
        
        await _enrollmentService.SetCurrentTopicAsync(Course!.Id, topic.Id);
        UpdateCurrentLessonStatus();
        NotifyProgressChanged();
    }

    public async Task CompleteCurrentTopicAsync()
    {
        if (Course == null || Enrollment == null || CurrentTopic == null) return;

        var request = new TopicUpdateProgressRequest
        {
            CourseId = Course.Id,
            TopicId = CurrentTopic.Id,
            TotalTopics = TotalTopics
        };

        await _enrollmentService.CompleteTopicAsync(request);

        // Refresh enrollment data
        Enrollment = await _enrollmentService.GetEnrollmentAsync(Course.Id);

        UpdateCurrentLessonStatus();
        await CheckUngradedActivitiesAsync();
        NotifyProgressChanged();
    }

    public async Task CheckUngradedActivitiesAsync()
    {
        if (Course == null) return;
        HasUngradedActivities = await _activityService.HasUngradedActivitiesAsync(Course.Id);
    }

    public async Task CompleteAndNextAsync()
    {
        if (Course == null || CurrentTopic == null) return;

        // 1. Complete current topic
        await CompleteCurrentTopicAsync();

        // 2. Find next topic in linear order
        var allTopics = Course.Lessons
            .OrderBy(l => l.Order)
            .SelectMany(l => (l.Topics ?? new List<Tema>()).OrderBy(t => t.Order))
            .ToList();

        var currentIndex = allTopics.FindIndex(t => t.Id == CurrentTopic.Id);
        
        if (currentIndex != -1 && currentIndex < allTopics.Count - 1)
        {
            var nextTopic = allTopics[currentIndex + 1];
            await SelectTopicAsync(nextTopic);
        }
        else if (currentIndex == allTopics.Count - 1)
        {
            // Course finished
            NotifyProgressChanged();
        }
    }

    public async Task SaveQuizAttemptAsync(Guid idQuiz, double score, int minRequired)
    {
        // Ensure auth is initialized if it hasn't been yet (precaution)
        if (_authService.CurrentUser == null)
        {
            await _authService.InitializeAsync();
        }

        var attempt = new QuizAttempt
        {
            IdUsuario = _authService.CurrentUser?.Id ?? Guid.Empty,
            IdQuiz = idQuiz,
            PuntajeObtenido = score,
            MinimoRequerido = minRequired
        };

        if (attempt.IdUsuario == Guid.Empty)
        {
            // Logging or handling error: User not identified
            return;
        }

        await _enrollmentService.SaveQuizAttemptAsync(attempt);
    }

    public void ToggleSidebar()
    {
        IsSidebarOpen = !IsSidebarOpen;
    }

    public bool IsTopicCompleted(Guid topicId)
        => Enrollment?.CompletedTopicIds.Contains(topicId) ?? false;

    public bool IsTopicLocked(Guid topicId)
    {
        if (Course == null || Enrollment == null) return true;

        var allTopics = Course.Lessons
            .OrderBy(l => l.Order)
            .SelectMany(l => (l.Topics ?? new List<Tema>()).OrderBy(t => t.Order))
            .ToList();

        var index = allTopics.FindIndex(t => t.Id == topicId);
        if (index <= 0) return false; // First topic is never locked

        // Locked if previous topic is not completed
        var prevTopic = allTopics[index - 1];
        return !IsTopicCompleted(prevTopic.Id);
    }

    public bool IsLessonLocked(Guid lessonId)
    {
        if (Course == null) return true;
        var lesson = Course.Lessons.FirstOrDefault(l => l.Id == lessonId);
        if (lesson == null) return true;

        // Locked if it's not the first lesson AND the last topic of the previous lesson is not completed
        var prevLesson = Course.Lessons
            .Where(l => l.Order < lesson.Order)
            .OrderByDescending(l => l.Order)
            .FirstOrDefault();

        if (prevLesson == null) return false;

        var lastTopicOfPrev = prevLesson.Topics?.OrderByDescending(t => t.Order).FirstOrDefault();
        if (lastTopicOfPrev == null) return false; // If prev lesson has no topics, it can't block

        return !IsTopicCompleted(lastTopicOfPrev.Id);
    }

    public bool IsLessonCompleted(Guid lessonId)
    {
        if (Course == null || Enrollment == null) return false;
        var lesson = Course.Lessons.FirstOrDefault(l => l.Id == lessonId);
        if (lesson == null || lesson.Topics == null || !lesson.Topics.Any()) return false;

        // A lesson is completed if all its topics are completed
        return lesson.Topics.All(t => IsTopicCompleted(t.Id));
    }

    private void UpdateCurrentLessonStatus()
    {
        IsCurrentLessonCompleted = CurrentTopic != null && IsTopicCompleted(CurrentTopic.Id);
    }

    private void NotifyProgressChanged()
    {
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(TotalTopics));
        OnPropertyChanged(nameof(IsCurrentLessonCompleted));
    }
}
