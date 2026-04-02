using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CursosIglesia.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly string _connectionString;

    public AdminService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<DashboardMetrics> GetDashboardMetricsAsync()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var metrics = await db.QueryFirstOrDefaultAsync<DashboardMetrics>(
            "usp_SuperAdmin",
            new { Accion = "MetricasDashboard" },
            commandType: CommandType.StoredProcedure
        );
        return metrics ?? new DashboardMetrics();
    }

    public async Task<List<MaestroDetail>> GetAllMaestrosAsync()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var maestros = await db.QueryAsync<MaestroDetail>(
            "usp_SuperAdmin",
            new { Accion = "ListarMaestros" },
            commandType: CommandType.StoredProcedure
        );
        return maestros.ToList();
    }

    public async Task<ApiResponse<Guid>> CrearMaestroAsync(CreateMaestroRequest request)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "CrearMaestro");
        parameters.Add("@Nombre", request.Nombre);
        parameters.Add("@Apellidos", request.Apellidos);
        parameters.Add("@Email", request.Email);
        parameters.Add("@PasswordHash", BCrypt.Net.BCrypt.HashPassword(request.Password));
        parameters.Add("@Telefono", request.Telefono);
        parameters.Add("@Especialidad", request.Especialidad);
        parameters.Add("@ExperienciaAnios", request.ExperienciaAnios);
        parameters.Add("@IdNuevo", dbType: DbType.Guid, direction: ParameterDirection.Output);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_SuperAdmin", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        if (success)
        {
            return new ApiResponse<Guid> 
            { 
                Success = true, 
                Data = parameters.Get<Guid>("@IdNuevo"),
                Message = "Maestro creado exitosamente"
            };
        }

        return new ApiResponse<Guid> 
        { 
            Success = false, 
            Message = parameters.Get<string>("@MensajeError") ?? "Error al crear maestro" 
        };
    }

    public async Task<List<CourseWithMaestro>> GetAllCoursesAsync()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var courses = await db.QueryAsync<CourseWithMaestro>(
            "usp_SuperAdmin",
            new { Accion = "ListarCursos" },
            commandType: CommandType.StoredProcedure
        );
        return courses.ToList();
    }

    public async Task<ApiResponse<Guid>> CrearCursoAsync(CreateCourseRequest request)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "CrearCurso");
        parameters.Add("@IdMaestro", request.IdMaestro);
        parameters.Add("@IdCategoria", request.IdCategoria);
        parameters.Add("@Titulo", request.Titulo);
        parameters.Add("@Descripcion", request.Descripcion);
        parameters.Add("@DescripcionCorta", request.DescripcionCorta);
        parameters.Add("@Precio", request.Precio);
        parameters.Add("@Dificultad", (int)request.Dificultad);
        parameters.Add("@EsGratis", request.EsGratis);
        parameters.Add("@EsDestacado", request.EsDestacado);
        parameters.Add("@ImagenUrl", request.ImagenUrl);
        parameters.Add("@DuracionHoras", request.DuracionHoras);
        parameters.Add("@IdNuevo", dbType: DbType.Guid, direction: ParameterDirection.Output);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_SuperAdmin", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        if (success)
        {
            return new ApiResponse<Guid> 
            { 
                Success = true, 
                Data = parameters.Get<Guid>("@IdNuevo"),
                Message = "Curso creado exitosamente"
            };
        }

        return new ApiResponse<Guid> 
        { 
            Success = false, 
            Message = parameters.Get<string>("@MensajeError") ?? "Error al crear curso" 
        };
    }

    public async Task<ApiResponse> ActualizarCursoAsync(Guid courseId, UpdateCourseRequest request)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "ActualizarCurso");
        parameters.Add("@IdCurso", courseId);
        parameters.Add("@IdCategoria", request.IdCategoria);
        parameters.Add("@IdMaestro", request.IdMaestro);
        parameters.Add("@Titulo", request.Titulo);
        parameters.Add("@Descripcion", request.Descripcion);
        parameters.Add("@DescripcionCorta", request.DescripcionCorta);
        parameters.Add("@Precio", request.Precio);
        parameters.Add("@Dificultad", (int)request.Dificultad);
        parameters.Add("@EsGratis", request.EsGratis);
        parameters.Add("@EsDestacado", request.EsDestacado);
        parameters.Add("@ImagenUrl", request.ImagenUrl);
        parameters.Add("@DuracionHoras", request.DuracionHoras);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_SuperAdmin", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        return new ApiResponse
        {
            Success = success,
            Message = success ? "Curso actualizado exitosamente" : (parameters.Get<string>("@MensajeError") ?? "Error al actualizar curso")
        };
    }

    public async Task<ApiResponse> EliminarCursoAsync(Guid courseId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "EliminarCurso");
        parameters.Add("@IdCurso", courseId);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_SuperAdmin", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        return new ApiResponse
        {
            Success = success,
            Message = success ? "Curso eliminado exitosamente" : (parameters.Get<string>("@MensajeError") ?? "Error al eliminar curso")
        };
    }

    public async Task<ApiResponse> AsignarCursoAsync(Guid courseId, Guid maestroId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "AsignarCurso");
        parameters.Add("@IdCurso", courseId);
        parameters.Add("@IdMaestro", maestroId);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_SuperAdmin", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        return new ApiResponse 
        { 
            Success = success, 
            Message = success ? "Curso asignado exitosamente" : (parameters.Get<string>("@MensajeError") ?? "Error al asignar curso")
        };
    }

    public async Task<List<EnrollmentDetail>> GetAllInscripcionesAsync()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var enrollments = await db.QueryAsync<EnrollmentDetail>(
            "usp_SuperAdmin",
            new { Accion = "ListarInscripciones" },
            commandType: CommandType.StoredProcedure
        );
        return enrollments.ToList();
    }

    public async Task<List<TeacherDocument>> GetAllDocumentosAsync()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var documents = await db.QueryAsync<TeacherDocument>(
            "usp_SuperAdmin",
            new { Accion = "ListarDocumentos" },
            commandType: CommandType.StoredProcedure
        );
        return documents.ToList();
    }

    public async Task<List<Category>> GetCategoriasAsync()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var categories = await db.QueryAsync<Category>(
            "usp_SuperAdmin",
            new { Accion = "ListarCategorias" },
            commandType: CommandType.StoredProcedure
        );
        return categories.ToList();
    }

    public async Task<ApiResponse<Guid>> CrearCategoriaAsync(string nombre, string? descripcion, string? icono, string? imagenUrl)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "CrearCategoria");
        parameters.Add("@NombreCategoria", nombre);
        parameters.Add("@DescripcionCategoria", descripcion);
        parameters.Add("@IconoCategoria", icono);
        parameters.Add("@ImagenCategoria", imagenUrl);
        parameters.Add("@IdNuevo", dbType: DbType.Guid, direction: ParameterDirection.Output);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_SuperAdmin", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        return new ApiResponse<Guid>
        {
            Success = success,
            Data = success ? parameters.Get<Guid>("@IdNuevo") : Guid.Empty,
            Message = success ? "Categoría creada" : (parameters.Get<string>("@MensajeError") ?? "Error")
        };
    }

    public async Task<ApiResponse> ActualizarCategoriaAsync(Guid id, string? nombre, string? descripcion, string? icono, string? imagenUrl)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "ActualizarCategoria");
        parameters.Add("@IdCategoria", id);
        parameters.Add("@NombreCategoria", nombre);
        parameters.Add("@DescripcionCategoria", descripcion);
        parameters.Add("@IconoCategoria", icono);
        parameters.Add("@ImagenCategoria", imagenUrl);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_SuperAdmin", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        return new ApiResponse
        {
            Success = success,
            Message = success ? "Categoría actualizada" : (parameters.Get<string>("@MensajeError") ?? "Error")
        };
    }

    public async Task<ApiResponse> EliminarCategoriaAsync(Guid id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "EliminarCategoria");
        parameters.Add("@IdCategoria", id);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_SuperAdmin", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        return new ApiResponse
        {
            Success = success,
            Message = success ? "Categoría eliminada" : (parameters.Get<string>("@MensajeError") ?? "Error")
        };
    }
}
