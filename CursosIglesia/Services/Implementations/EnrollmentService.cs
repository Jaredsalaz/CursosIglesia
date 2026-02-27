using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;

namespace CursosIglesia.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    // Static list to persist across scoped instances during app lifetime
    private static readonly List<Enrollment> _enrollments = new();
    private static readonly object _lock = new();

    public Task<bool> EnrollAsync(int courseId)
    {
        lock (_lock)
        {
            if (_enrollments.Any(e => e.CourseId == courseId))
                return Task.FromResult(false);

            _enrollments.Add(new Enrollment
            {
                CourseId = courseId,
                EnrolledDate = DateTime.Now,
                Progress = 0,
                CompletedLessonIds = new List<int>(),
                CurrentLessonId = null,
                IsCompleted = false
            });
        }
        return Task.FromResult(true);
    }

    public Task<bool> UnenrollAsync(int courseId)
    {
        lock (_lock)
        {
            var enrollment = _enrollments.FirstOrDefault(e => e.CourseId == courseId);
            if (enrollment == null)
                return Task.FromResult(false);

            _enrollments.Remove(enrollment);
        }
        return Task.FromResult(true);
    }

    public Task<bool> IsEnrolledAsync(int courseId)
        => Task.FromResult(_enrollments.Any(e => e.CourseId == courseId));

    public Task<List<Enrollment>> GetEnrollmentsAsync()
        => Task.FromResult(_enrollments.ToList());

    public Task<Enrollment?> GetEnrollmentAsync(int courseId)
        => Task.FromResult(_enrollments.FirstOrDefault(e => e.CourseId == courseId));

    public Task<bool> CompleteLessonAsync(int courseId, int lessonId, int totalLessons)
    {
        lock (_lock)
        {
            var enrollment = _enrollments.FirstOrDefault(e => e.CourseId == courseId);
            if (enrollment == null)
                return Task.FromResult(false);

            if (!enrollment.CompletedLessonIds.Contains(lessonId))
            {
                enrollment.CompletedLessonIds.Add(lessonId);
            }

            enrollment.Progress = totalLessons > 0
                ? Math.Round((double)enrollment.CompletedLessonIds.Count / totalLessons * 100, 1)
                : 0;

            enrollment.IsCompleted = enrollment.CompletedLessonIds.Count >= totalLessons;

            return Task.FromResult(true);
        }
    }

    public Task<bool> SetCurrentLessonAsync(int courseId, int lessonId)
    {
        lock (_lock)
        {
            var enrollment = _enrollments.FirstOrDefault(e => e.CourseId == courseId);
            if (enrollment == null)
                return Task.FromResult(false);

            enrollment.CurrentLessonId = lessonId;
            return Task.FromResult(true);
        }
    }
}
