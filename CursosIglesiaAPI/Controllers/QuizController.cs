using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    // GET api/quiz/tema/{temaId}  — public so students can access
    [HttpGet("tema/{temaId:guid}")]
    public async Task<ActionResult<Quiz>> GetByTema(Guid temaId)
    {
        var quiz = await _quizService.GetQuizByTemaAsync(temaId);
        if (quiz == null) return NotFound();
        return Ok(quiz);
    }

    // POST api/quiz/tema/{temaId}  — Maestro only
    [HttpPost("tema/{temaId:guid}")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<Quiz>> Create(Guid temaId, [FromBody] CreateQuizRequest req)
    {
        var quiz = await _quizService.CreateQuizAsync(temaId, req);
        return Ok(quiz);
    }

    // PUT api/quiz/{quizId}
    [HttpPut("{quizId:guid}")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<Quiz>> Update(Guid quizId, [FromBody] CreateQuizRequest req)
    {
        var quiz = await _quizService.UpdateQuizAsync(quizId, req);
        return Ok(quiz);
    }

    // DELETE api/quiz/{quizId}
    [HttpDelete("{quizId:guid}")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult> Delete(Guid quizId)
    {
        var ok = await _quizService.DeleteQuizAsync(quizId);
        return ok ? NoContent() : NotFound();
    }

    // POST api/quiz/tema/{temaId}/generate  — AI generation
    [HttpPost("tema/{temaId:guid}/generate")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult<Quiz>> Generate(Guid temaId, [FromBody] GenerateQuizRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.TextoContenido))
            return BadRequest("El tema no tiene contenido de texto para analizar.");

        var quiz = await _quizService.GenerateQuizWithAIAsync(temaId, req.TextoContenido);
        return Ok(quiz);
    }
}
