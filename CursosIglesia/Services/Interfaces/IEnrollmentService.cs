using CursosIglesia.Models;

namespace CursosIglesia.Services.Interfaces;

public interface IEnrollmentService
{
    Task<bool> EnrollAsync(int courseId);
    Task<bool> UnenrollAsync(int courseId);
    Task<bool> IsEnrolledAsync(int courseId);
    Task<List<Enrollment>> GetEnrollmentsAsync();
    Task<Enrollment?> GetEnrollmentAsync(int courseId);
    Task<bool> CompleteLessonAsync(int courseId, int lessonId, int totalLessons);
    Task<bool> SetCurrentLessonAsync(int courseId, int lessonId);
}
