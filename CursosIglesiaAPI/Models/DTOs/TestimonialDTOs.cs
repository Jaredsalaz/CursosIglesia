using System.ComponentModel.DataAnnotations;

namespace CursosIglesia.Models.DTOs;

public class AddTestimonialRequest
{
    [Required]
    public Guid CourseId { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "El comentario debe tener entre 10 y 1000 caracteres.")]
    public string Comment { get; set; } = string.Empty;

    [Required]
    [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5 estrellas.")]
    public int Rating { get; set; }
}
