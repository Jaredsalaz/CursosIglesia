using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CursosIglesia.Services.Implementations;

public class TestimonialService : ITestimonialService
{
    private readonly string _connectionString;

    public TestimonialService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<List<Testimonial>> GetTestimonialsAsync(int count = 6)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var testimonials = await db.QueryAsync<Testimonial>(
            "usp_GestionTestimonios",
            new { Accion = "ListarAprobados", Top = count },
            commandType: CommandType.StoredProcedure
        );
        return testimonials.ToList();
    }

    public async Task<List<Testimonial>> GetByCourseIdAsync(Guid courseId, bool onlyApproved = true)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var testimonials = await db.QueryAsync<Testimonial>(
            "usp_GestionTestimonios",
            new { Accion = "ListarPorCurso", IdCurso = courseId },
            commandType: CommandType.StoredProcedure
        );
        return testimonials.ToList();
    }

    public async Task<List<Testimonial>> GetPendingTestimonialsAsync()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var testimonials = await db.QueryAsync<Testimonial>(
            "usp_GestionTestimonios",
            new { Accion = "ListarPendientes" },
            commandType: CommandType.StoredProcedure
        );
        return testimonials.ToList();
    }

    public async Task<bool> AddTestimonialAsync(Guid userId, Guid courseId, string comment, int rating)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "InsertarOActualizar");
        parameters.Add("@IdUsuario", userId);
        parameters.Add("@IdCurso", courseId);
        parameters.Add("@Comentario", comment);
        parameters.Add("@Calificacion", rating);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_GestionTestimonios", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<bool>("@Exito");
    }

    public async Task<bool> ApproveTestimonialAsync(Guid testimonialId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "Aprobar");
        parameters.Add("@IdTestimonio", testimonialId);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_GestionTestimonios", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<bool>("@Exito");
    }
}
