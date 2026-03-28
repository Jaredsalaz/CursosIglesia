using CursosIglesia.Models;
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

    public async Task<List<Course>> GetMaestroCoursesAsync(Guid maestroId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var courses = await db.QueryAsync<Course>(
            "usp_MaestroYCursos",
            new { Accion = "MisCursosComoMaestro", IdUsuario = maestroId }, // Using IdUsuario as IdMaestro for now
            commandType: CommandType.StoredProcedure
        );
        return courses.ToList();
    }

    public async Task<Guid> CrearCursoAsync(Course course)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "CrearCurso");
        parameters.Add("@IdMaestro", course.InstructorId); // We should have this in the model
        parameters.Add("@IdCategoria", course.CategoryId);
        parameters.Add("@Titulo", course.Title);
        parameters.Add("@Descripcion", course.Description);
        parameters.Add("@Precio", course.Price);
        parameters.Add("@IdCursoNuevo", dbType: DbType.Guid, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_MaestroYCursos", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<Guid>("@IdCursoNuevo");
    }

    public async Task ActualizarCursoAsync(Course course)
    {
        // Add update logic if needed
        await Task.CompletedTask;
    }

    public async Task SubirDocumentoAsync(TeacherDocument document)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        await db.ExecuteAsync("usp_MaestroYCursos",
            new { 
                Accion = "SubirDocumentacion", 
                IdMaestro = document.IdMaestro,
                NombreDocumento = document.NombreArchivo,
                RutaDocumento = document.UrlArchivo 
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<List<TeacherDocument>> GetDocumentosMaestroAsync(Guid maestroId)
    {
        // Mock or implement in SP
        return await Task.FromResult(new List<TeacherDocument>());
    }
}
