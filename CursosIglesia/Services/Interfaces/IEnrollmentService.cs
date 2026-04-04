using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface IEnrollmentService
{
    Task<bool> EnrollAsync(Guid courseId);
    Task<bool> UnenrollAsync(Guid courseId);
    Task<bool> IsEnrolledAsync(Guid courseId);
    Task<List<Enrollment>> GetEnrollmentsAsync();
    Task<Enrollment?> GetEnrollmentAsync(Guid courseId);
    Task<bool> CompleteLessonAsync(LessonUpdateProgressRequest request);
    Task<bool> SetCurrentLessonAsync(Guid courseId, Guid lessonId);
    Task<bool> SetCurrentTopicAsync(Guid courseId, Guid topicId);
    Task<bool> CompleteTopicAsync(TopicUpdateProgressRequest request);
    Task<bool> SaveQuizAttemptAsync(QuizAttempt attempt);
}
