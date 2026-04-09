using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
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
    private readonly IMaestroService _maestroService;

    public FileUploadController(IWebHostEnvironment env, ILogger<FileUploadController> logger, IMaestroService maestroService)
    {
        _env = env;
        _logger = logger;
        _maestroService = maestroService;
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

    /// <summary>
    /// Sube un archivo de apoyo (imagen, Word, PDF) a un tema específico.
    /// Límite: 3MB, máximo 3 archivos por tema.
    /// Solo accesible por Maestros y SuperAdmin.
    /// </summary>
    [HttpPost("topic-file")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult> UploadTopicFile(IFormFile file, [FromQuery] Guid temaId)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "No se seleccionó ningún archivo" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf", ".doc", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { success = false, message = "Tipo no permitido. Use: jpg, png, webp, pdf, doc, docx" });

        if (file.Length > 3 * 1024 * 1024) // 3MB max
            return BadRequest(new { success = false, message = "El archivo es muy grande. Máximo 3MB" });

        if (temaId == Guid.Empty)
            return BadRequest(new { success = false, message = "ID de tema inválido" });

        try
        {
            var baseDir = _env.WebRootPath ?? _env.ContentRootPath;
            var uploadsDir = Path.Combine(baseDir, "uploads", "temas", temaId.ToString());
            Directory.CreateDirectory(uploadsDir);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var rutaRelativa = $"/uploads/temas/{temaId}/{uniqueFileName}";

            var archivo = new ArchivoTema
            {
                TemaId = temaId,
                NombreOriginal = file.FileName,
                NombreServidor = uniqueFileName,
                RutaArchivo = rutaRelativa,
                TipoArchivo = extension.TrimStart('.'),
                TamanoBytes = file.Length
            };

            var result = await _maestroService.AgregarArchivoTemaAsync(archivo);

            if (!result.Success)
            {
                // Si la BD rechaza (ej. ya hay 3 archivos), eliminar el archivo del disco
                System.IO.File.Delete(filePath);
                return BadRequest(new { success = false, message = result.Message });
            }

            _logger.LogInformation("Topic file uploaded: {Url}", rutaRelativa);
            return Ok(new
            {
                success = true,
                archivo = new
                {
                    id = result.Data,
                    temaId,
                    nombreOriginal = file.FileName,
                    nombreServidor = uniqueFileName,
                    rutaArchivo = rutaRelativa,
                    tipoArchivo = extension.TrimStart('.'),
                    tamanoBytes = file.Length,
                    fechaSubida = DateTime.Now
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading topic file");
            return StatusCode(500, new { success = false, message = "Error al subir el archivo" });
        }
    }

    /// <summary>
    /// Elimina un archivo de apoyo de un tema. Borra del disco y de la BD.
    /// </summary>
    [HttpDelete("topic-file/{archivoId:guid}")]
    [Authorize(Roles = "Maestro,SuperAdmin")]
    public async Task<ActionResult> DeleteTopicFile(Guid archivoId)
    {
        var result = await _maestroService.EliminarArchivoTemaAsync(archivoId);

        if (!result.Success)
            return NotFound(new { success = false, message = result.Message });

        // Eliminar del disco si la ruta fue devuelta
        if (!string.IsNullOrEmpty(result.Data))
        {
            var baseDir = _env.WebRootPath ?? _env.ContentRootPath;
            var fullPath = Path.Combine(baseDir, result.Data.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        return Ok(new { success = true, message = "Archivo eliminado" });
    }
}
