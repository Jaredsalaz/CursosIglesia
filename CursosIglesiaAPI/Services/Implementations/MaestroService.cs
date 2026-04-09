using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CursosIglesia.Services.Implementations;

public class MaestroService : IMaestroService
{
    private readonly string _connectionString;

    public MaestroService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<MaestroMetrics> GetMaestroMetricsAsync(Guid userId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var metrics = await db.QueryFirstOrDefaultAsync<MaestroMetrics>(
            "usp_DashboardMaestro",
            new { Accion = "MetricasMaestro", IdUsuario = userId },
            commandType: CommandType.StoredProcedure
        );
        return metrics ?? new MaestroMetrics();
    }

    public async Task<List<Course>> GetMaestroCoursesAsync(Guid userId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var courses = await db.QueryAsync<Course>(
            "usp_DashboardMaestro",
            new { Accion = "MisCursos", IdUsuario = userId },
            commandType: CommandType.StoredProcedure
        );
        return courses.ToList();
    }

    public async Task<List<Lesson>> GetLeccionesCursoAsync(Guid courseId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var lessons = await db.QueryAsync<Lesson>(
            "usp_DashboardMaestro",
            new { Accion = "LeccionesPorCurso", IdCurso = courseId },
            commandType: CommandType.StoredProcedure
        );
        return lessons.ToList();
    }

    public async Task<ApiResponse<Guid>> CrearLeccionAsync(Guid userId, CreateLessonRequest request)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "CrearLeccion");
        parameters.Add("@IdUsuario", userId);
        parameters.Add("@IdCurso", request.CourseId);
        parameters.Add("@TituloLeccion", request.Title);
        parameters.Add("@DescripcionLeccion", request.Description);
        parameters.Add("@Orden", request.Order);
        parameters.Add("@IdNuevo", dbType: DbType.Guid, direction: ParameterDirection.Output);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_DashboardMaestro", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        if (success)
        {
            return new ApiResponse<Guid>
            {
                Success = true,
                Data = parameters.Get<Guid>("@IdNuevo"),
                Message = "Lección creada exitosamente"
            };
        }

        return new ApiResponse<Guid>
        {
            Success = false,
            Message = parameters.Get<string>("@MensajeError") ?? "Error al crear lección"
        };
    }

    public async Task<ApiResponse> ActualizarLeccionAsync(Guid lessonId, CreateLessonRequest request)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "ActualizarLeccion");
        parameters.Add("@IdLeccion", lessonId);
        parameters.Add("@TituloLeccion", request.Title);
        parameters.Add("@DescripcionLeccion", request.Description);
        parameters.Add("@Orden", request.Order);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_DashboardMaestro", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        return new ApiResponse
        {
            Success = success,
            Message = success ? "Lección actualizada" : (parameters.Get<string>("@MensajeError") ?? "Error al actualizar")
        };
    }

    public async Task<ApiResponse> EliminarLeccionAsync(Guid lessonId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "EliminarLeccion");
        parameters.Add("@IdLeccion", lessonId);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_DashboardMaestro", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        return new ApiResponse
        {
            Success = success,
            Message = success ? "Lección eliminada" : (parameters.Get<string>("@MensajeError") ?? "Error al eliminar")
        };
    }

    public async Task<List<Tema>> GetTemasLeccionAsync(Guid lessonId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var temas = await db.QueryAsync<Tema>(
            "usp_DashboardTemas",
            new { Accion = "ListarTemasPorLeccion", IdLeccion = lessonId },
            commandType: CommandType.StoredProcedure
        );
        return temas.ToList();
    }

    public async Task<ApiResponse<Guid>> CrearTemaAsync(Guid userId, CreateTemaRequest request)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "CrearTema");
        parameters.Add("@IdUsuario", userId);
        parameters.Add("@IdLeccion", request.LessonId);
        parameters.Add("@Titulo", request.Title);
        parameters.Add("@DescripcionCorta", request.Description);
        parameters.Add("@ContenidoTexto", request.TextContent);
        parameters.Add("@UrlContenido", request.ContentUrl);
        parameters.Add("@TipoContenido", (int)request.ContentType);
        parameters.Add("@DuracionMinutos", request.DurationMinutes);
        parameters.Add("@Orden", request.Order);
        parameters.Add("@EsGratis", request.IsFree);
        parameters.Add("@IdNuevo", dbType: DbType.Guid, direction: ParameterDirection.Output);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_DashboardTemas", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        if (success)
        {
            return new ApiResponse<Guid>
            {
                Success = true,
                Data = parameters.Get<Guid>("@IdNuevo"),
                Message = "Tema creado exitosamente"
            };
        }

        return new ApiResponse<Guid>
        {
            Success = false,
            Message = parameters.Get<string>("@MensajeError") ?? "Error al crear tema"
        };
    }

    public async Task<ApiResponse> ActualizarTemaAsync(Guid temaId, CreateTemaRequest request)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "ActualizarTema");
        parameters.Add("@IdTema", temaId);
        parameters.Add("@Titulo", request.Title);
        parameters.Add("@DescripcionCorta", request.Description);
        parameters.Add("@ContenidoTexto", request.TextContent);
        parameters.Add("@UrlContenido", request.ContentUrl);
        parameters.Add("@TipoContenido", (int)request.ContentType);
        parameters.Add("@DuracionMinutos", request.DurationMinutes);
        parameters.Add("@Orden", request.Order);
        parameters.Add("@EsGratis", request.IsFree);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_DashboardTemas", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        return new ApiResponse
        {
            Success = success,
            Message = success ? "Tema actualizado" : (parameters.Get<string>("@MensajeError") ?? "Error al actualizar")
        };
    }

    public async Task<ApiResponse> EliminarTemaAsync(Guid temaId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "EliminarTema");
        parameters.Add("@IdTema", temaId);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_DashboardTemas", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        return new ApiResponse
        {
            Success = success,
            Message = success ? "Tema eliminado" : (parameters.Get<string>("@MensajeError") ?? "Error al eliminar")
        };
    }

    public async Task<List<EnrollmentDetail>> GetInscripcionesCursoAsync(Guid courseId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var enrollments = await db.QueryAsync<EnrollmentDetail>(
            "usp_DashboardMaestro",
            new { Accion = "InscripcionesPorCurso", IdCurso = courseId },
            commandType: CommandType.StoredProcedure
        );
        return enrollments.ToList();
    }

    public async Task<List<EnrollmentDetail>> GetTodasMisInscripcionesAsync(Guid userId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var enrollments = await db.QueryAsync<EnrollmentDetail>(
            "usp_DashboardMaestro",
            new { Accion = "TodasMisInscripciones", IdUsuario = userId },
            commandType: CommandType.StoredProcedure
        );
        return enrollments.ToList();
    }

    public async Task<List<TeacherDocument>> GetDocumentosMaestroAsync(Guid userId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var documents = await db.QueryAsync<TeacherDocument>(
            "usp_DashboardMaestro",
            new { Accion = "MisDocumentos", IdUsuario = userId },
            commandType: CommandType.StoredProcedure
        );
        return documents.ToList();
    }

    public async Task<ApiResponse> SubirDocumentoAsync(Guid userId, TeacherDocument document)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "SubirDocumento");
        parameters.Add("@IdUsuario", userId);
        parameters.Add("@IdCurso", document.IdCurso);
        parameters.Add("@NombreDocumento", document.NombreArchivo);
        parameters.Add("@RutaDocumento", document.UrlArchivo);
        parameters.Add("@TipoDocumento", document.TipoDocumento);
        parameters.Add("@IdNuevo", dbType: DbType.Guid, direction: ParameterDirection.Output);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_DashboardMaestro", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        return new ApiResponse
        {
            Success = success,
            Message = success ? "Documento subido exitosamente" : (parameters.Get<string>("@MensajeError") ?? "Error al subir documento")
        };
    }

    public async Task<List<ArchivoTema>> GetArchivosTemaAsync(Guid temaId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var archivos = await db.QueryAsync<ArchivoTema>(
            "usp_DashboardTemas",
            new { Accion = "ListarArchivosPorTema", IdTema = temaId },
            commandType: CommandType.StoredProcedure
        );
        return archivos.ToList();
    }

    public async Task<ApiResponse<Guid>> AgregarArchivoTemaAsync(ArchivoTema archivo)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "AgregarArchivoTema");
        parameters.Add("@IdTema", archivo.TemaId);
        parameters.Add("@NombreOriginal", archivo.NombreOriginal);
        parameters.Add("@NombreServidor", archivo.NombreServidor);
        parameters.Add("@RutaArchivo", archivo.RutaArchivo);
        parameters.Add("@TipoArchivo", archivo.TipoArchivo);
        parameters.Add("@TamanoBytes", archivo.TamanoBytes);
        parameters.Add("@IdNuevo", dbType: DbType.Guid, direction: ParameterDirection.Output);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_DashboardTemas", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        if (success)
            return new ApiResponse<Guid> { Success = true, Data = parameters.Get<Guid>("@IdNuevo"), Message = "Archivo agregado" };

        return new ApiResponse<Guid> { Success = false, Message = parameters.Get<string>("@MensajeError") ?? "Error al agregar archivo" };
    }

    public async Task<ApiResponse<string>> EliminarArchivoTemaAsync(Guid archivoId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "EliminarArchivoTema");
        parameters.Add("@IdArchivo", archivoId);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

        // SP returns a resultset with RutaArchivo before deleting
        var rutaArchivo = await db.QueryFirstOrDefaultAsync<string>("usp_DashboardTemas", parameters, commandType: CommandType.StoredProcedure);

        bool success = parameters.Get<bool>("@Exito");
        if (success)
            return new ApiResponse<string> { Success = true, Data = rutaArchivo ?? "", Message = "Archivo eliminado" };

        return new ApiResponse<string> { Success = false, Message = parameters.Get<string>("@MensajeError") ?? "Error al eliminar archivo" };
    }
}
