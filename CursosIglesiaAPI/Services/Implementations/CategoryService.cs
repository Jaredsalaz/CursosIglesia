using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CursosIglesia.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly string _connectionString;

    public CategoryService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var categories = await db.QueryAsync<Category>(
            "usp_CatalogoYCursos",
            new { Accion = "ObtenerCategorias" },
            commandType: CommandType.StoredProcedure
        );
        return categories.ToList();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        return await db.QueryFirstOrDefaultAsync<Category>(
            "usp_CatalogoYCursos",
            new { Accion = "ObtenerCategorias", IdCategoria = id }, // Note: Our SP filter for single ID might need adjustment if not implemented, but the query handles it better via parameters if we add it. 
            commandType: CommandType.StoredProcedure
        );
    }
}
