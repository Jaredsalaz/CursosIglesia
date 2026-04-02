using CursosIglesia.Models;

namespace CursosIglesia.Models.DTOs;

// ===== Dashboard Metrics =====
public class DashboardMetrics
{
    public int TotalCursos { get; set; }
    public int TotalMaestros { get; set; }
    public int TotalInscripciones { get; set; }
    public int TotalEstudiantes { get; set; }
    public int TotalCategorias { get; set; }
    public int TotalDocumentos { get; set; }
    public int InscripcionesUltimos30Dias { get; set; }
    public int CursosNuevosUltimos30Dias { get; set; }
}

public class MaestroMetrics
{
    public int TotalCursos { get; set; }
    public int TotalAlumnos { get; set; }
    public int TotalLecciones { get; set; }
    public int TotalDocumentos { get; set; }
    public int InscripcionesRecientes { get; set; }
}

public class MaestroDetail
{
    public Guid Id { get; set; }
    public Guid IdUsuario { get; set; }
    public string Especialidad { get; set; } = string.Empty;
    public int ExperienciaAnios { get; set; }
    public bool Activo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? UrlAvatar { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public int TotalCursos { get; set; }
    public int TotalAlumnos { get; set; }
    public string NombreCompleto => $"{Nombre} {Apellidos}";
}

public class CreateMaestroRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Especialidad { get; set; } = string.Empty;
    public int ExperienciaAnios { get; set; }
}

public class CreateCourseRequest
{
    public Guid? IdMaestro { get; set; }
    public Guid IdCategoria { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string DescripcionCorta { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public DifficultyLevel Dificultad { get; set; }
    public bool EsGratis { get; set; } = true;
    public bool EsDestacado { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public int DuracionHoras { get; set; }
}

public class UpdateCourseRequest : CreateCourseRequest
{
}

public class AssignCourseRequest
{
    public Guid IdCurso { get; set; }
    public Guid IdMaestro { get; set; }
}

public class EnrollmentDetail
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public DateTime EnrolledDate { get; set; }
    public double Progress { get; set; }
    public bool IsCompleted { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
}

public class CourseWithMaestro
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public bool IsFree { get; set; }
    public bool IsFeatured { get; set; }
    public int DurationHours { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid? IdMaestro { get; set; }
    public Guid? MaestroId { get; set; }
    public string Instructor { get; set; } = string.Empty;
    public int LessonsCount { get; set; }
    public int StudentsEnrolled { get; set; }
}

public class CreateLessonRequest
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class CreateTemaRequest
{
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public string TextContent { get; set; } = string.Empty;
    public string ContentUrl { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int Order { get; set; }
    public bool IsFree { get; set; }
}

public class CreateCategoryRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public string? ImagenUrl { get; set; }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}

public class FileUploadResponse
{
    public bool Success { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
