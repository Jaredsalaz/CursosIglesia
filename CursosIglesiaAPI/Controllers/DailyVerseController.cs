using CursosIglesiaAPI.Models.DTOs;
using CursosIglesiaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/daily-verse")]
public class DailyVerseController : ControllerBase
{
    private readonly IDailyVerseService _dailyVerseService;
    private readonly ILogger<DailyVerseController> _logger;

    public DailyVerseController(IDailyVerseService dailyVerseService, ILogger<DailyVerseController> logger)
    {
        _dailyVerseService = dailyVerseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<DailyVerseDTO>> GetDailyVerse()
    {
        try
        {
            var verse = await _dailyVerseService.GetDailyVerseAsync();
            return Ok(verse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo versículo del día");
            return StatusCode(500, new { success = false, message = "Error obteniendo versículo del día" });
        }
    }
}
