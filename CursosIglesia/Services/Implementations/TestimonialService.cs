using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;

namespace CursosIglesia.Services.Implementations;

public class TestimonialService : ITestimonialService
{
    private readonly List<Testimonial> _testimonials;

    public TestimonialService()
    {
        _testimonials = new List<Testimonial>
        {
            new()
            {
                Id = 1,
                StudentName = "Laura Martínez García",
                StudentImageUrl = "https://ui-avatars.com/api/?name=Laura+Martinez&background=c4a265&color=fff&size=100",
                CourseName = "Introducción al Catecismo",
                Comment = "Este curso transformó mi comprensión de la fe católica. Los videos son claros y el material es excelente. ¡Lo recomiendo a todos!",
                Rating = 5,
                Date = new DateTime(2025, 8, 15)
            },
            new()
            {
                Id = 2,
                StudentName = "Carlos Eduardo Ríos",
                StudentImageUrl = "https://ui-avatars.com/api/?name=Carlos+Rios&background=c4a265&color=fff&size=100",
                CourseName = "Sagrada Escritura: Antiguo Testamento",
                Comment = "Increíble profundidad en el análisis de las Escrituras. El Dr. Sánchez explica todo de manera comprensible y enriquecedora.",
                Rating = 5,
                Date = new DateTime(2025, 9, 20)
            },
            new()
            {
                Id = 3,
                StudentName = "María José Fernández",
                StudentImageUrl = "https://ui-avatars.com/api/?name=Maria+Fernandez&background=c4a265&color=fff&size=100",
                CourseName = "Vida de Oración y Espiritualidad",
                Comment = "Mi vida de oración ha crecido enormemente gracias a este curso. Las prácticas de Lectio Divina son maravillosas.",
                Rating = 5,
                Date = new DateTime(2025, 11, 5)
            },
            new()
            {
                Id = 4,
                StudentName = "Pedro Antonio Vega",
                StudentImageUrl = "https://ui-avatars.com/api/?name=Pedro+Vega&background=c4a265&color=fff&size=100",
                CourseName = "Formación para Catequistas",
                Comment = "Como catequista parroquial, este curso me dio herramientas invaluables. Mis clases ahora son mucho más dinámicas y efectivas.",
                Rating = 4,
                Date = new DateTime(2025, 10, 28)
            },
            new()
            {
                Id = 5,
                StudentName = "Isabel Cristina Morales",
                StudentImageUrl = "https://ui-avatars.com/api/?name=Isabel+Morales&background=c4a265&color=fff&size=100",
                CourseName = "Los Sacramentos",
                Comment = "Ahora comprendo mucho mejor el significado de cada sacramento. La hermana María del Carmen es una excelente instructora.",
                Rating = 5,
                Date = new DateTime(2025, 12, 10)
            }
        };
    }

    public Task<List<Testimonial>> GetTestimonialsAsync(int count = 5)
        => Task.FromResult(_testimonials.Take(count).ToList());
}
