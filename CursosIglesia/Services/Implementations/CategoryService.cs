using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;

namespace CursosIglesia.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly List<Category> _categories;

    public CategoryService()
    {
        _categories = new List<Category>
        {
            new() { Id = 1, Name = "Catecismo", Description = "Fundamentos de la fe católica", Icon = "bi-book", ImageUrl = "https://images.unsplash.com/photo-1548625149-fc4a29cf7092?w=300&h=200&fit=crop", CourseCount = 1 },
            new() { Id = 2, Name = "Sacramentos", Description = "Los siete sacramentos de la Iglesia", Icon = "bi-droplet", ImageUrl = "https://images.unsplash.com/photo-1507692049790-de58290a4334?w=300&h=200&fit=crop", CourseCount = 2 },
            new() { Id = 3, Name = "Biblia", Description = "Estudio de la Sagrada Escritura", Icon = "bi-journal-text", ImageUrl = "https://images.unsplash.com/photo-1504052434569-70ad5836ab65?w=300&h=200&fit=crop", CourseCount = 2 },
            new() { Id = 4, Name = "Historia", Description = "Historia de la Iglesia Católica", Icon = "bi-clock-history", ImageUrl = "https://images.unsplash.com/photo-1539037116277-4db20889f2d4?w=300&h=200&fit=crop", CourseCount = 1 },
            new() { Id = 5, Name = "Espiritualidad", Description = "Vida de oración y crecimiento espiritual", Icon = "bi-heart", ImageUrl = "https://images.unsplash.com/photo-1545232979-8bf68ee9b1af?w=300&h=200&fit=crop", CourseCount = 1 },
            new() { Id = 6, Name = "Doctrina Social", Description = "La Iglesia y la sociedad", Icon = "bi-people", ImageUrl = "https://images.unsplash.com/photo-1469571486292-0ba58a3f068b?w=300&h=200&fit=crop", CourseCount = 1 },
            new() { Id = 7, Name = "Mariología", Description = "Estudio sobre la Virgen María", Icon = "bi-star", ImageUrl = "https://images.unsplash.com/photo-1577495508048-b635879837f1?w=300&h=200&fit=crop", CourseCount = 1 },
            new() { Id = 8, Name = "Teología Moral", Description = "Ética y moral cristiana", Icon = "bi-shield-check", ImageUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=300&h=200&fit=crop", CourseCount = 1 },
            new() { Id = 9, Name = "Liturgia", Description = "Celebración y año litúrgico", Icon = "bi-music-note-beamed", ImageUrl = "https://images.unsplash.com/photo-1544427920-c49ccfb85579?w=300&h=200&fit=crop", CourseCount = 1 },
            new() { Id = 10, Name = "Catequesis", Description = "Formación de catequistas", Icon = "bi-mortarboard", ImageUrl = "https://images.unsplash.com/photo-1529070538774-1843cb3265df?w=300&h=200&fit=crop", CourseCount = 1 },
        };
    }

    public Task<List<Category>> GetAllCategoriesAsync()
        => Task.FromResult(_categories);

    public Task<Category?> GetCategoryByIdAsync(int id)
        => Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));
}
