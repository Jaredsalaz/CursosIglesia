using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CursosIglesia.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly string _connectionString;

    public CourseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<List<Course>> GetAllCoursesAsync()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var courses = await db.QueryAsync<Course>(
            "usp_CatalogoYCursos",
            new { Accion = "BuscarCursos" },
            commandType: CommandType.StoredProcedure
        );
        return courses.ToList();
    }

    public async Task<List<Course>> GetFeaturedCoursesAsync()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var courses = await db.QueryAsync<Course>(
            "usp_CatalogoYCursos",
            new { Accion = "BuscarCursos", SoloDestacados = 1 },
            commandType: CommandType.StoredProcedure
        );
        return courses.ToList();
    }

    public async Task<List<Course>> GetCoursesByCategoryAsync(Guid categoryId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var courses = await db.QueryAsync<Course>(
            "usp_CatalogoYCursos",
            new { Accion = "BuscarCursos", IdCategoria = categoryId },
            commandType: CommandType.StoredProcedure
        );
        return courses.ToList();
    }

    public async Task<List<Course>> SearchCoursesAsync(CourseSearchRequest request)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var courses = await db.QueryAsync<Course>(
            "usp_CatalogoYCursos",
            new { 
                Accion = "BuscarCursos", 
                Busqueda = request.Query, 
                IdCategoria = request.CategoryId 
            },
            commandType: CommandType.StoredProcedure
        );
        return courses.ToList();
    }

    public async Task<Course?> GetCourseByIdAsync(Guid id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        using var multi = await db.QueryMultipleAsync(
            "usp_CatalogoYCursos",
            new { Accion = "ObtenerCursoDetalle", IdCurso = id },
            commandType: CommandType.StoredProcedure
        );

        var course = await multi.ReadFirstOrDefaultAsync<Course>();
        if (course != null)
        {
            course.Lessons = (await multi.ReadAsync<Lesson>()).ToList();
        }
        return course;
    }

    public async Task<List<Course>> GetPopularCoursesAsync(int count = 6)
    {
        // For simplicity, we'll use the search and take count. 
        // In a real scenario, we could add a specific action or order to the SP.
        var courses = await GetAllCoursesAsync();
        return courses.OrderByDescending(c => c.StudentsEnrolled).Take(count).ToList();
    }

    public async Task<List<Course>> GetRecentCoursesAsync(int count = 6)
    {
        var courses = await GetAllCoursesAsync();
        return courses.OrderByDescending(c => c.CreatedDate).Take(count).ToList();
    }
}
