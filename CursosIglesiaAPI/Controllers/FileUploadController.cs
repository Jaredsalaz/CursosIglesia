using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursosIglesia.Controllers;

[ApiController]
[Route("api/upload")]
[Authorize]
public class FileUploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(IWebHostEnvironment env, ILogger<FileUploadController> logger)
    {
        _env = env;
        _logger = logger;
    }

    [HttpPost("image")]
    public async Task<ActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "No se seleccionó ningún archivo" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { success = false, message = "Tipo de archivo no permitido. Use: jpg, png, webp, gif, svg" });

        if (file.Length > 5 * 1024 * 1024) // 5MB max
            return BadRequest(new { success = false, message = "El archivo es muy grande. Máximo 5MB" });

        try
        {
            var uploadsDir = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", "images");
            Directory.CreateDirectory(uploadsDir);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/images/{uniqueFileName}";
            _logger.LogInformation("Image uploaded: {Url}", imageUrl);

            return Ok(new { success = true, url = imageUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, new { success = false, message = "Error al subir la imagen" });
        }
    }
}
