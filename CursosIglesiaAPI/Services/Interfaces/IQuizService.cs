using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface IQuizService
{
    Task<Quiz?> GetQuizByTemaAsync(Guid temaId);
    Task<Quiz> CreateQuizAsync(Guid temaId, CreateQuizRequest request);
    Task<Quiz> UpdateQuizAsync(Guid quizId, CreateQuizRequest request);
    Task<bool> DeleteQuizAsync(Guid quizId);
    Task<Quiz> GenerateQuizWithAIAsync(Guid temaId, string contenidoLeccion);
}
