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

    public async Task<List<Testimonial>> GetTestimonialsAsync(int count = 5)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var testimonials = await db.QueryAsync<Testimonial>(
            @"SELECT TOP (@Count) 
                t.IdTestimonio AS Id, 
                u.Nombre + ' ' + u.Apellidos AS StudentName, 
                u.UrlAvatar AS StudentImageUrl, 
                c.Titulo AS CourseName, 
                t.Comentario AS Comment, 
                t.Calificacion AS Rating, 
                t.FechaTestimonio AS Date 
            FROM Testimonios t
            JOIN Usuarios u ON t.IdUsuario = u.IdUsuario
            JOIN Cursos c ON t.IdCurso = c.IdCurso
            ORDER BY t.FechaTestimonio DESC",
            new { Count = count }
        );
        return testimonials.ToList();
    }
}
