using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface IDailyVerseService
{
    Task<DailyVerseDTO?> GetDailyVerseAsync();
}
