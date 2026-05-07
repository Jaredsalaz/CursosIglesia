using CursosIglesiaAPI.Models.DTOs;
using CursosIglesiaAPI.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text.Json;

namespace CursosIglesiaAPI.Services.Implementations;

public class DailyVerseService : IDailyVerseService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DailyVerseService> _logger;

    // Libros y sus capítulos (clásicos de la Biblia)
    private readonly Dictionary<string, int> _bibleBooks = new()
    {
        { "John", 21 },
        { "Matthew", 28 },
        { "Mark", 16 },
        { "Luke", 24 },
        { "Romans", 16 },
        { "1 Corinthians", 16 },
        { "2 Corinthians", 13 },
        { "Galatians", 6 },
        { "Ephesians", 6 },
        { "Philippians", 4 },
        { "Colossians", 4 },
        { "1 Thessalonians", 5 },
        { "2 Thessalonians", 3 },
        { "1 Timothy", 6 },
        { "2 Timothy", 4 },
        { "Titus", 3 },
        { "Philemon", 1 },
        { "Hebrews", 13 },
        { "James", 5 },
        { "1 Peter", 5 },
        { "2 Peter", 3 },
        { "1 John", 5 },
        { "2 John", 1 },
        { "3 John", 1 },
        { "Jude", 1 },
        { "Revelation", 22 },
        { "Psalms", 150 },
        { "Proverbs", 31 },
        { "Genesis", 50 },
        { "Exodus", 40 },
        { "Leviticus", 27 },
        { "Numbers", 36 },
        { "Deuteronomy", 34 }
    };

    // Versículos por defecto en ESPAÑOL (biblia católica)
    private readonly List<DailyVerseDTO> _fallbackVerses = new()
    {
        new() { Text = "Ama a tu prójimo como a ti mismo.", Reference = "Mateo 22:39", Book = "Mateo", Chapter = 22, Verse = 39, Theme = "Amor" },
        new() { Text = "Porque Dios amó al mundo de tal manera que ha dado a su Hijo unigénito, para que todo aquel que en él cree no se pierda, mas tenga vida eterna.", Reference = "Juan 3:16", Book = "Juan", Chapter = 3, Verse = 16, Theme = "Amor y Fe" },
        new() { Text = "Estad siempre gozosos. Orad sin cesar. Dad gracias en todo, porque esta es la voluntad de Dios para con vosotros en Cristo Jesús.", Reference = "1 Tesalonicenses 5:16-18", Book = "1 Tesalonicenses", Chapter = 5, Verse = 16, Theme = "Gozo y Gratitud" },
        new() { Text = "Todo lo puedo en Cristo que me fortalece.", Reference = "Filipenses 4:13", Book = "Filipenses", Chapter = 4, Verse = 13, Theme = "Fortaleza" },
        new() { Text = "Porque somos hechura de Dios, creados en Cristo Jesús para buenas obras, las cuales Dios preparó de antemano para que anduviésemos en ellas.", Reference = "Efesios 2:10", Book = "Efesios", Chapter = 2, Verse = 10, Theme = "Propósito" },
        new() { Text = "Fíate de Jehová de todo tu corazón, y no te apoyes en tu propia prudencia.", Reference = "Proverbios 3:5", Book = "Proverbios", Chapter = 3, Verse = 5, Theme = "Fe" },
        new() { Text = "Espera a Jehová; esfuérzate, y aliéntese tu corazón; sí, espera a Jehová.", Reference = "Salmos 27:14", Book = "Salmos", Chapter = 27, Verse = 14, Theme = "Esperanza" },
        new() { Text = "La gracia y la paz sean multiplicadas en vosotros, por el conocimiento de Dios y de Jesús nuestro Señor.", Reference = "2 Pedro 1:2", Book = "2 Pedro", Chapter = 1, Verse = 2, Theme = "Paz" },
        new() { Text = "Cada uno según el don que ha recibido, adminístrelo a los otros, como buen dispensador de la multiforme gracia de Dios.", Reference = "1 Pedro 4:10", Book = "1 Pedro", Chapter = 4, Verse = 10, Theme = "Servicio" },
        new() { Text = "He aquí, te mando que te esfuerces y seas valiente; no temas ni desmayes, porque Jehová tu Dios estará contigo en dondequiera que vayas.", Reference = "Josué 1:9", Book = "Josué", Chapter = 1, Verse = 9, Theme = "Valentía" },
        new() { Text = "Por nada estéis afanosos, sino sean conocidas vuestras peticiones delante de Dios en toda oración y ruego, con acción de gracias.", Reference = "Filipenses 4:6", Book = "Filipenses", Chapter = 4, Verse = 6, Theme = "Paz Mental" },
        new() { Text = "Y sabemos que a los que aman a Dios, todas las cosas les ayudan a bien, esto es, a los que conforme a su propósito son llamados.", Reference = "Romanos 8:28", Book = "Romanos", Chapter = 8, Verse = 28, Theme = "Providencia" },
        new() { Text = "Jesús le dijo: Yo soy el camino, y la verdad, y la vida; nadie viene al Padre, sino por mí.", Reference = "Juan 14:6", Book = "Juan", Chapter = 14, Verse = 6, Theme = "Verdad" }
    };

    private readonly string _jsonPath;

    public DailyVerseService(IMemoryCache cache, ILogger<DailyVerseService> logger, IWebHostEnvironment env)
    {
        _cache = cache;
        _logger = logger;
        _jsonPath = Path.Combine(env.ContentRootPath, "Data", "biblia_latinoamericana.json");
    }

    public async Task<DailyVerseDTO> GetDailyVerseAsync()
    {
        var cacheKey = $"daily_verse_{DateTime.Now.DayOfYear}";

        // Verificar si ya está en caché
        if (_cache.TryGetValue(cacheKey, out DailyVerseDTO? cachedVerse) && cachedVerse != null)
        {
            _logger.LogInformation("Versículo del día obtenido del caché");
            return cachedVerse;
        }

        try
        {
            // Cargar versículos desde JSON local
            var verses = await LoadVersesFromJsonAsync();
            
            if (verses == null || !verses.Any())
            {
                _logger.LogWarning("No se encontraron versículos en el JSON local.");
                return _fallbackVerses[0];
            }

            // Selección rotativa basada en el día del año para consistencia diaria
            var index = DateTime.Now.DayOfYear % verses.Count;
            var verse = verses[index];
            verse.Date = DateTime.Now;

            CacheVerse(cacheKey, verse);
            _logger.LogInformation($"Versículo del día (BL95): {verse.Reference}");
            return verse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo versículo del día desde JSON local");
            var fallbackVerse = _fallbackVerses[0];
            fallbackVerse.Date = DateTime.Now;
            return fallbackVerse;
        }
    }

    private async Task<List<DailyVerseDTO>> LoadVersesFromJsonAsync()
    {
        try
        {
            if (!File.Exists(_jsonPath))
            {
                _logger.LogError($"Archivo JSON no encontrado en: {_jsonPath}");
                return _fallbackVerses;
            }

            var jsonContent = await File.ReadAllTextAsync(_jsonPath);
            return System.Text.Json.JsonSerializer.Deserialize<List<DailyVerseDTO>>(jsonContent) ?? _fallbackVerses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializando JSON de Biblia");
            return _fallbackVerses;
        }
    }


    private void CacheVerse(string key, DailyVerseDTO verse)
    {
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(24));

        _cache.Set(key, verse, cacheOptions);
    }
}
