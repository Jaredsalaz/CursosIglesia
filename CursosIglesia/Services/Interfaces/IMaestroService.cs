using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface IMaestroService
{
    Task<List<Course>> GetMaestroCoursesAsync(Guid maestroId);
    Task<Guid> CrearCursoAsync(Course course);
    Task ActualizarCursoAsync(Course course);
    Task SubirDocumentoAsync(TeacherDocument document);
    Task<List<TeacherDocument>> GetDocumentosMaestroAsync(Guid maestroId);
}
