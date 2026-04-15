using CursosIglesiaAPI.Models.DTOs;

namespace CursosIglesiaAPI.Services.Interfaces;

public interface IDailyVerseService
{
    Task<DailyVerseDTO> GetDailyVerseAsync();
}
