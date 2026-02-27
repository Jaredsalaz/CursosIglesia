using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;

namespace CursosIglesia.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly List<Course> _courses;

    public CourseService()
    {
        _courses = GenerateHardcodedCourses();
    }

    public Task<List<Course>> GetAllCoursesAsync()
        => Task.FromResult(_courses);

    public Task<List<Course>> GetFeaturedCoursesAsync()
        => Task.FromResult(_courses.Where(c => c.IsFeatured).ToList());

    public Task<List<Course>> GetCoursesByCategoryAsync(int categoryId)
        => Task.FromResult(_courses.Where(c => c.CategoryId == categoryId).ToList());

    public Task<List<Course>> SearchCoursesAsync(string query)
        => Task.FromResult(_courses.Where(c =>
            c.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            c.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            c.CategoryName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList());

    public Task<Course?> GetCourseByIdAsync(int id)
        => Task.FromResult(_courses.FirstOrDefault(c => c.Id == id));

    public Task<List<Course>> GetPopularCoursesAsync(int count = 6)
        => Task.FromResult(_courses.OrderByDescending(c => c.StudentsEnrolled).Take(count).ToList());

    public Task<List<Course>> GetRecentCoursesAsync(int count = 6)
        => Task.FromResult(_courses.OrderByDescending(c => c.CreatedDate).Take(count).ToList());

    private static List<Course> GenerateHardcodedCourses()
    {
        return new List<Course>
        {
            new()
            {
                Id = 1,
                Title = "Introducción al Catecismo de la Iglesia Católica",
                ShortDescription = "Conoce los fundamentos de la fe católica a través del Catecismo oficial de la Iglesia.",
                Description = "Este curso te guiará a través de los pilares fundamentales del Catecismo de la Iglesia Católica. Aprenderás sobre la profesión de fe, la celebración del misterio cristiano, la vida en Cristo y la oración cristiana. Ideal para quienes desean profundizar en su conocimiento de la doctrina católica.",
                ImageUrl = "https://images.unsplash.com/photo-1548625149-fc4a29cf7092?w=600&h=400&fit=crop",
                Instructor = "P. Miguel Ángel Torres",
                InstructorImageUrl = "https://ui-avatars.com/api/?name=Miguel+Torres&background=7b2d26&color=fff&size=150",
                InstructorBio = "Sacerdote con más de 20 años de experiencia en catequesis y formación teológica.",
                CategoryId = 1,
                CategoryName = "Catecismo",
                DurationHours = 12,
                LessonsCount = 24,
                StudentsEnrolled = 3450,
                Rating = 4.8,
                RatingsCount = 520,
                Difficulty = DifficultyLevel.Principiante,
                IsFeatured = true,
                IsFree = true,
                Price = 0,
                CreatedDate = new DateTime(2025, 6, 15),
                Topics = new List<string> { "Credo", "Sacramentos", "Mandamientos", "Oración", "Fe Católica" },
                Lessons = GenerateLessons(1, 24, "Catecismo")
            },
            new()
            {
                Id = 2,
                Title = "Los Sacramentos: Signos de la Gracia de Dios",
                ShortDescription = "Profundiza en el significado y la importancia de los siete sacramentos.",
                Description = "Explora cada uno de los siete sacramentos de la Iglesia Católica: Bautismo, Confirmación, Eucaristía, Penitencia, Unción de los Enfermos, Orden Sacerdotal y Matrimonio. Comprende su origen bíblico, su desarrollo histórico y su significado para la vida del cristiano de hoy.",
                ImageUrl = "https://images.unsplash.com/photo-1507692049790-de58290a4334?w=600&h=400&fit=crop",
                Instructor = "Hna. María del Carmen López",
                InstructorImageUrl = "https://ui-avatars.com/api/?name=Maria+Lopez&background=7b2d26&color=fff&size=150",
                InstructorBio = "Religiosa y teóloga con especialización en sacramentología.",
                CategoryId = 2,
                CategoryName = "Sacramentos",
                DurationHours = 14,
                LessonsCount = 28,
                StudentsEnrolled = 2870,
                Rating = 4.9,
                RatingsCount = 430,
                Difficulty = DifficultyLevel.Intermedio,
                IsFeatured = true,
                IsFree = false,
                Price = 29.99m,
                CreatedDate = new DateTime(2025, 7, 20),
                Topics = new List<string> { "Bautismo", "Eucaristía", "Confirmación", "Penitencia", "Matrimonio" },
                Lessons = GenerateLessons(2, 28, "Sacramentos")
            },
            new()
            {
                Id = 3,
                Title = "Sagrada Escritura: Antiguo Testamento",
                ShortDescription = "Recorre los libros del Antiguo Testamento y descubre el plan de salvación de Dios.",
                Description = "Un recorrido completo por los libros del Antiguo Testamento. Desde el Génesis hasta los profetas, comprenderás el contexto histórico, literario y teológico de cada libro. Descubre cómo Dios fue preparando la venida del Mesías a lo largo de la historia de Israel.",
                ImageUrl = "https://images.unsplash.com/photo-1504052434569-70ad5836ab65?w=600&h=400&fit=crop",
                Instructor = "Dr. Roberto Sánchez Villanueva",
                InstructorImageUrl = "https://ui-avatars.com/api/?name=Roberto+Sanchez&background=7b2d26&color=fff&size=150",
                InstructorBio = "Doctor en Sagrada Escritura por el Pontificio Instituto Bíblico de Roma.",
                CategoryId = 3,
                CategoryName = "Biblia",
                DurationHours = 20,
                LessonsCount = 40,
                StudentsEnrolled = 4120,
                Rating = 4.7,
                RatingsCount = 680,
                Difficulty = DifficultyLevel.Intermedio,
                IsFeatured = true,
                IsFree = false,
                Price = 39.99m,
                CreatedDate = new DateTime(2025, 5, 10),
                Topics = new List<string> { "Pentateuco", "Profetas", "Salmos", "Libros Históricos", "Sapienciales" },
                Lessons = GenerateLessons(3, 40, "Biblia AT")
            },
            new()
            {
                Id = 4,
                Title = "Sagrada Escritura: Nuevo Testamento",
                ShortDescription = "Estudia los Evangelios, las cartas de San Pablo y el Apocalipsis.",
                Description = "Adéntrate en el Nuevo Testamento comenzando por los cuatro Evangelios, los Hechos de los Apóstoles, las cartas paulinas y católicas, hasta el Apocalipsis. Comprende el mensaje de Jesús y cómo los primeros cristianos vivieron y transmitieron la fe.",
                ImageUrl = "https://images.unsplash.com/photo-1585829365295-ab7cd400c167?w=600&h=400&fit=crop",
                Instructor = "Dr. Roberto Sánchez Villanueva",
                InstructorImageUrl = "https://ui-avatars.com/api/?name=Roberto+Sanchez&background=7b2d26&color=fff&size=150",
                InstructorBio = "Doctor en Sagrada Escritura por el Pontificio Instituto Bíblico de Roma.",
                CategoryId = 3,
                CategoryName = "Biblia",
                DurationHours = 18,
                LessonsCount = 36,
                StudentsEnrolled = 3890,
                Rating = 4.8,
                RatingsCount = 590,
                Difficulty = DifficultyLevel.Intermedio,
                IsFeatured = true,
                IsFree = false,
                Price = 39.99m,
                CreatedDate = new DateTime(2025, 8, 1),
                Topics = new List<string> { "Evangelios", "Hechos", "Cartas Paulinas", "Apocalipsis", "Parábolas" },
                Lessons = GenerateLessons(4, 36, "Biblia NT")
            },
            new()
            {
                Id = 5,
                Title = "Historia de la Iglesia Católica",
                ShortDescription = "Desde los Apóstoles hasta el Papa Francisco: 2000 años de historia.",
                Description = "Un viaje fascinante por los 2000 años de historia de la Iglesia Católica. Desde la comunidad apostólica, pasando por las persecuciones romanas, los concilios ecuménicos, la Edad Media, la Reforma, hasta los desafíos contemporáneos. Conoce a los santos, papas y eventos que forjaron la Iglesia.",
                ImageUrl = "https://images.unsplash.com/photo-1539037116277-4db20889f2d4?w=600&h=400&fit=crop",
                Instructor = "Dra. Patricia Mendoza Ruiz",
                InstructorImageUrl = "https://ui-avatars.com/api/?name=Patricia+Mendoza&background=7b2d26&color=fff&size=150",
                InstructorBio = "Historiadora especializada en Historia de la Iglesia y profesora universitaria.",
                CategoryId = 4,
                CategoryName = "Historia",
                DurationHours = 16,
                LessonsCount = 32,
                StudentsEnrolled = 2340,
                Rating = 4.6,
                RatingsCount = 345,
                Difficulty = DifficultyLevel.Intermedio,
                IsFeatured = false,
                IsFree = false,
                Price = 34.99m,
                CreatedDate = new DateTime(2025, 9, 5),
                Topics = new List<string> { "Iglesia Primitiva", "Concilios", "Santos", "Papado", "Vaticano II" },
                Lessons = GenerateLessons(5, 32, "Historia")
            },
            new()
            {
                Id = 6,
                Title = "Vida de Oración y Espiritualidad",
                ShortDescription = "Aprende diferentes métodos de oración y fortalece tu vida espiritual.",
                Description = "Descubre las riquezas de la tradición orante de la Iglesia Católica. Desde la Lectio Divina y el Rosario hasta la oración contemplativa de los grandes místicos como Santa Teresa de Jesús y San Juan de la Cruz. Desarrolla una vida de oración profunda y constante.",
                ImageUrl = "https://images.unsplash.com/photo-1545232979-8bf68ee9b1af?w=600&h=400&fit=crop",
                Instructor = "P. Francisco Javier Ruiz",
                InstructorImageUrl = "https://ui-avatars.com/api/?name=Francisco+Ruiz&background=7b2d26&color=fff&size=150",
                InstructorBio = "Director espiritual y experto en mística carmelitana.",
                CategoryId = 5,
                CategoryName = "Espiritualidad",
                DurationHours = 10,
                LessonsCount = 20,
                StudentsEnrolled = 5200,
                Rating = 4.9,
                RatingsCount = 890,
                Difficulty = DifficultyLevel.Principiante,
                IsFeatured = true,
                IsFree = true,
                Price = 0,
                CreatedDate = new DateTime(2025, 10, 12),
                Topics = new List<string> { "Rosario", "Lectio Divina", "Contemplación", "Liturgia de las Horas", "Examen de Conciencia" },
                Lessons = GenerateLessons(6, 20, "Oración")
            },
            new()
            {
                Id = 7,
                Title = "Doctrina Social de la Iglesia",
                ShortDescription = "Principios y valores para transformar la sociedad desde la fe.",
                Description = "Conoce los principios fundamentales de la Doctrina Social de la Iglesia: dignidad humana, bien común, subsidiaridad y solidaridad. Desde la Rerum Novarum hasta Fratelli Tutti, comprende cómo la Iglesia ilumina los desafíos sociales, económicos y políticos de nuestro tiempo.",
                ImageUrl = "https://images.unsplash.com/photo-1469571486292-0ba58a3f068b?w=600&h=400&fit=crop",
                Instructor = "Dr. Alejandro Gutiérrez Mora",
                InstructorImageUrl = "https://ui-avatars.com/api/?name=Alejandro+Gutierrez&background=7b2d26&color=fff&size=150",
                InstructorBio = "Teólogo y sociólogo con maestría en Doctrina Social de la Iglesia.",
                CategoryId = 6,
                CategoryName = "Doctrina Social",
                DurationHours = 12,
                LessonsCount = 24,
                StudentsEnrolled = 1890,
                Rating = 4.5,
                RatingsCount = 267,
                Difficulty = DifficultyLevel.Avanzado,
                IsFeatured = false,
                IsFree = false,
                Price = 29.99m,
                CreatedDate = new DateTime(2025, 11, 8),
                Topics = new List<string> { "Dignidad Humana", "Bien Común", "Solidaridad", "Subsidiaridad", "Encíclicas Sociales" },
                Lessons = GenerateLessons(7, 24, "DSI")
            },
            new()
            {
                Id = 8,
                Title = "Preparación para la Primera Comunión",
                ShortDescription = "Curso completo de catequesis para preparar la Primera Comunión.",
                Description = "Curso diseñado para catequistas y padres de familia que acompañan a niños en su preparación para recibir la Primera Comunión. Incluye material didáctico, actividades y dinámicas para hacer comprensible el misterio eucarístico a los más pequeños.",
                ImageUrl = "https://images.unsplash.com/photo-1438032005730-c779502df39b?w=600&h=400&fit=crop",
                Instructor = "Hna. Guadalupe Flores",
                InstructorImageUrl = "https://ui-avatars.com/api/?name=Guadalupe+Flores&background=7b2d26&color=fff&size=150",
                InstructorBio = "Religiosa con 15 años de experiencia en catequesis infantil.",
                CategoryId = 2,
                CategoryName = "Sacramentos",
                DurationHours = 8,
                LessonsCount = 16,
                StudentsEnrolled = 6780,
                Rating = 4.9,
                RatingsCount = 1120,
                Difficulty = DifficultyLevel.Principiante,
                IsFeatured = true,
                IsFree = true,
                Price = 0,
                CreatedDate = new DateTime(2025, 4, 20),
                Topics = new List<string> { "Eucaristía", "Confesión", "Catequesis Infantil", "Liturgia", "Vida Sacramental" },
                Lessons = GenerateLessons(8, 16, "Primera Comunión")
            },
            new()
            {
                Id = 9,
                Title = "Mariología: La Virgen María en la Fe Católica",
                ShortDescription = "Estudio completo sobre la Santísima Virgen María y sus dogmas.",
                Description = "Profundiza en el papel de María Santísima en la historia de la salvación. Estudia los dogmas marianos, las apariciones aprobadas por la Iglesia, la devoción mariana a lo largo de los siglos y el papel de María como Madre de la Iglesia y modelo de fe para todos los cristianos.",
                ImageUrl = "https://images.unsplash.com/photo-1577495508048-b635879837f1?w=600&h=400&fit=crop",
                Instructor = "P. José Luis Hernández",
                InstructorImageUrl = "https://ui-avatars.com/api/?name=Jose+Hernandez&background=7b2d26&color=fff&size=150",
                InstructorBio = "Mariólogo y autor de diversos libros sobre la Virgen María.",
                CategoryId = 7,
                CategoryName = "Mariología",
                DurationHours = 10,
                LessonsCount = 20,
                StudentsEnrolled = 3100,
                Rating = 4.8,
                RatingsCount = 478,
                Difficulty = DifficultyLevel.Intermedio,
                IsFeatured = true,
                IsFree = false,
                Price = 24.99m,
                CreatedDate = new DateTime(2025, 12, 1),
                Topics = new List<string> { "Dogmas Marianos", "Apariciones", "Rosario", "Advocaciones", "María en la Biblia" },
                Lessons = GenerateLessons(9, 20, "Mariología")
            },
            new()
            {
                Id = 10,
                Title = "Teología Moral Fundamental",
                ShortDescription = "Principios de la moral católica para la vida cristiana en el mundo actual.",
                Description = "Estudia los fundamentos de la teología moral católica: la conciencia, la ley moral, las virtudes, el pecado y la gracia. Comprende cómo aplicar los principios morales católicos a los dilemas éticos contemporáneos con fidelidad al Magisterio de la Iglesia.",
                ImageUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=600&h=400&fit=crop",
                Instructor = "Dr. Fernando Castillo Pérez",
                InstructorImageUrl = "https://ui-avatars.com/api/?name=Fernando+Castillo&background=7b2d26&color=fff&size=150",
                InstructorBio = "Filósofo y teólogo moralista con doctorado por la Universidad Gregoriana.",
                CategoryId = 8,
                CategoryName = "Teología Moral",
                DurationHours = 15,
                LessonsCount = 30,
                StudentsEnrolled = 1560,
                Rating = 4.6,
                RatingsCount = 234,
                Difficulty = DifficultyLevel.Avanzado,
                IsFeatured = false,
                IsFree = false,
                Price = 44.99m,
                CreatedDate = new DateTime(2026, 1, 10),
                Topics = new List<string> { "Conciencia Moral", "Virtudes", "Pecado", "Ley Natural", "Bioética" },
                Lessons = GenerateLessons(10, 30, "Moral")
            },
            new()
            {
                Id = 11,
                Title = "Liturgia y Año Litúrgico",
                ShortDescription = "Comprende la riqueza de la liturgia católica y el ciclo del año litúrgico.",
                Description = "Descubre el significado profundo de la liturgia católica. Aprende sobre la Santa Misa, el año litúrgico (Adviento, Navidad, Cuaresma, Pascua, Tiempo Ordinario), los colores litúrgicos, los gestos y símbolos. Un curso esencial para quienes participan en ministerios litúrgicos.",
                ImageUrl = "https://images.unsplash.com/photo-1544427920-c49ccfb85579?w=600&h=400&fit=crop",
                Instructor = "Mons. Ricardo Valenzuela",
                InstructorImageUrl = "https://ui-avatars.com/api/?name=Ricardo+Valenzuela&background=7b2d26&color=fff&size=150",
                InstructorBio = "Liturgista y profesor del Seminario Diocesano.",
                CategoryId = 9,
                CategoryName = "Liturgia",
                DurationHours = 10,
                LessonsCount = 20,
                StudentsEnrolled = 2670,
                Rating = 4.7,
                RatingsCount = 398,
                Difficulty = DifficultyLevel.Principiante,
                IsFeatured = false,
                IsFree = false,
                Price = 24.99m,
                CreatedDate = new DateTime(2025, 11, 15),
                Topics = new List<string> { "Santa Misa", "Adviento", "Cuaresma", "Pascua", "Ministerios Litúrgicos" },
                Lessons = GenerateLessons(11, 20, "Liturgia")
            },
            new()
            {
                Id = 12,
                Title = "Formación para Catequistas",
                ShortDescription = "Herramientas pedagógicas y teológicas para la catequesis parroquial.",
                Description = "Curso integral de formación para catequistas parroquiales. Incluye fundamentos teológicos, metodología catequética, planificación de sesiones, manejo de grupo y recursos didácticos. Prepárate para ser un catequista efectivo y comprometido con la evangelización.",
                ImageUrl = "https://images.unsplash.com/photo-1529070538774-1843cb3265df?w=600&h=400&fit=crop",
                Instructor = "Lic. Ana María Rodríguez",
                InstructorImageUrl = "https://ui-avatars.com/api/?name=Ana+Rodriguez&background=7b2d26&color=fff&size=150",
                InstructorBio = "Pedagoga y coordinadora diocesana de catequesis con 18 años de experiencia.",
                CategoryId = 10,
                CategoryName = "Catequesis",
                DurationHours = 14,
                LessonsCount = 28,
                StudentsEnrolled = 4500,
                Rating = 4.8,
                RatingsCount = 720,
                Difficulty = DifficultyLevel.Intermedio,
                IsFeatured = true,
                IsFree = false,
                Price = 34.99m,
                CreatedDate = new DateTime(2025, 3, 1),
                Topics = new List<string> { "Pedagogía", "Didáctica", "Evangelización", "Planificación", "Recursos" },
                Lessons = GenerateLessons(12, 28, "Catequistas")
            }
        };
    }

    private static List<Lesson> GenerateLessons(int courseId, int count, string prefix)
    {
        var lessons = new List<Lesson>();
        var types = new[] { LessonType.Video, LessonType.Lectura, LessonType.Video, LessonType.Cuestionario };

        for (int i = 1; i <= count; i++)
        {
            lessons.Add(new Lesson
            {
                Id = courseId * 100 + i,
                CourseId = courseId,
                Order = i,
                Title = $"{prefix} - Lección {i}",
                Description = $"Contenido de la lección {i} del curso.",
                DurationMinutes = 15 + (i % 4) * 5,
                IsFree = i <= 2,
                Type = types[(i - 1) % types.Length]
            });
        }

        return lessons;
    }
}
