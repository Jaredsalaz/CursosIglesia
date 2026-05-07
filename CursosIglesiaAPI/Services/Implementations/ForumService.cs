using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CursosIglesia.Services.Implementations;

public class ForumService : IForumService
{
    private readonly string _connectionString;

    public ForumService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    // CRUD de Foros
    public async Task<ApiResponse<Guid>> CreateForumAsync(Guid userId, CreateForumRequest request)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            var forumId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var sql = @"
                INSERT INTO Forums (Id, CourseId, TemaId, LessonId, Title, Description,
                    CreatedByUserId, CreatedAt, UpdatedAt)
                VALUES (@Id, @CourseId, @TemaId, @LessonId, @Title, @Description,
                    @CreatedByUserId, @CreatedAt, @UpdatedAt)";

            await db.ExecuteAsync(sql, new
            {
                Id = forumId,
                request.CourseId,
                request.TemaId,
                request.LessonId,
                request.Title,
                request.Description,
                CreatedByUserId = userId,
                CreatedAt = now,
                UpdatedAt = now
            });

            return new ApiResponse<Guid>
            {
                Success = true,
                Data = forumId,
                Message = "Foro creado exitosamente"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = $"Error creando foro: {ex.Message}"
            };
        }
    }

    public async Task<Forum?> GetForumAsync(Guid forumId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var sql = "SELECT * FROM Forums WHERE Id = @Id";
        var forum = await db.QueryFirstOrDefaultAsync<Forum>(sql, new { Id = forumId });

        return forum;
    }

    public async Task<List<ForumDto>> GetCourseForumsAsync(Guid courseId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var sql = @"
            SELECT
                f.Id,
                f.CourseId,
                f.TemaId,
                f.LessonId,
                f.Title,
                f.Description,
                (SELECT COUNT(*) FROM ForumPosts WHERE ForumId = f.Id) as TotalPosts,
                (SELECT COUNT(DISTINCT UserId) FROM ForumPosts WHERE ForumId = f.Id) as TotalParticipants,
                f.CreatedAt,
                f.UpdatedAt
            FROM Forums f
            WHERE f.CourseId = @CourseId
            ORDER BY f.CreatedAt DESC";

        var forums = (await db.QueryAsync<ForumDto>(sql, new { CourseId = courseId })).ToList();
        return forums;
    }

    public async Task<ApiResponse> DeleteForumAsync(Guid forumId)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            // Las relaciones en cascada eliminarán posts y likes
            var sql = "DELETE FROM Forums WHERE Id = @Id";
            await db.ExecuteAsync(sql, new { Id = forumId });

            return new ApiResponse
            {
                Success = true,
                Message = "Foro eliminado exitosamente"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Success = false,
                Message = $"Error eliminando foro: {ex.Message}"
            };
        }
    }

    // Gestión de Posts
    public async Task<ApiResponse<Guid>> CreatePostAsync(Guid userId, CreateForumPostRequest request)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            var postId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var sql = @"
                INSERT INTO ForumPosts (Id, ForumId, UserId, ParentPostId, Title, Content,
                    Likes, IsPinned, CreatedAt, UpdatedAt)
                VALUES (@Id, @ForumId, @UserId, @ParentPostId, @Title, @Content,
                    0, 0, @CreatedAt, @UpdatedAt)";

            await db.ExecuteAsync(sql, new
            {
                Id = postId,
                request.ForumId,
                UserId = userId,
                request.ParentPostId,
                request.Title,
                request.Content,
                CreatedAt = now,
                UpdatedAt = now
            });

            return new ApiResponse<Guid>
            {
                Success = true,
                Data = postId,
                Message = "Post creado exitosamente"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = $"Error creando post: {ex.Message}"
            };
        }
    }

    public async Task<List<ForumPostDto>> GetForumPostsAsync(Guid forumId, Guid? currentUserId = null)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var sql = @"
            SELECT
                fp.Id,
                fp.ForumId,
                fp.UserId,
                up.FirstName + ' ' + up.LastName as UserName,
                up.UrlAvatar as UserAvatar,
                fp.ParentPostId,
                fp.Title,
                fp.Content,
                fp.Likes,
                (SELECT COUNT(*) FROM ForumLikes WHERE PostId = fp.Id AND UserId = @CurrentUserId) as IsLikedByCurrentUser,
                fp.IsPinned,
                fp.CreatedAt,
                fp.UpdatedAt
            FROM ForumPosts fp
            JOIN Usuarios up ON fp.UserId = up.Id
            WHERE fp.ForumId = @ForumId AND fp.ParentPostId IS NULL
            ORDER BY fp.IsPinned DESC, fp.CreatedAt DESC";

        var posts = (await db.QueryAsync<ForumPostDto>(sql,
            new { ForumId = forumId, CurrentUserId = currentUserId })).ToList();

        // Obtener respuestas para cada post principal
        foreach (var post in posts)
        {
            var repliesSql = @"
                SELECT
                    fp.Id,
                    fp.ForumId,
                    fp.UserId,
                    up.FirstName + ' ' + up.LastName as UserName,
                    up.UrlAvatar as UserAvatar,
                    fp.ParentPostId,
                    fp.Title,
                    fp.Content,
                    fp.Likes,
                    (SELECT COUNT(*) FROM ForumLikes WHERE PostId = fp.Id AND UserId = @CurrentUserId) as IsLikedByCurrentUser,
                    fp.IsPinned,
                    fp.CreatedAt,
                    fp.UpdatedAt
                FROM ForumPosts fp
                JOIN Usuarios up ON fp.UserId = up.Id
                WHERE fp.ParentPostId = @ParentPostId
                ORDER BY fp.CreatedAt ASC";

            var replies = (await db.QueryAsync<ForumPostDto>(repliesSql,
                new { ParentPostId = post.Id, CurrentUserId = currentUserId })).ToList();

            post.Replies = replies;
        }

        return posts;
    }

    public async Task<ForumPostDto?> GetPostAsync(Guid postId, Guid? currentUserId = null)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var sql = @"
            SELECT
                fp.Id,
                fp.ForumId,
                fp.UserId,
                up.FirstName + ' ' + up.LastName as UserName,
                up.UrlAvatar as UserAvatar,
                fp.ParentPostId,
                fp.Title,
                fp.Content,
                fp.Likes,
                (SELECT COUNT(*) FROM ForumLikes WHERE PostId = fp.Id AND UserId = @CurrentUserId) as IsLikedByCurrentUser,
                fp.IsPinned,
                fp.CreatedAt,
                fp.UpdatedAt
            FROM ForumPosts fp
            JOIN Usuarios up ON fp.UserId = up.Id
            WHERE fp.Id = @PostId";

        return await db.QueryFirstOrDefaultAsync<ForumPostDto>(sql,
            new { PostId = postId, CurrentUserId = currentUserId });
    }

    public async Task<ApiResponse> UpdatePostAsync(Guid postId, UpdateForumPostRequest request)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            var sql = @"
                UPDATE ForumPosts
                SET Content = @Content, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            await db.ExecuteAsync(sql, new
            {
                Content = request.Content,
                UpdatedAt = DateTime.UtcNow,
                Id = postId
            });

            return new ApiResponse
            {
                Success = true,
                Message = "Post actualizado exitosamente"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Success = false,
                Message = $"Error actualizando post: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse> DeletePostAsync(Guid postId)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            // Las respuestas se eliminarán en cascada
            var sql = "DELETE FROM ForumPosts WHERE Id = @Id";
            await db.ExecuteAsync(sql, new { Id = postId });

            return new ApiResponse
            {
                Success = true,
                Message = "Post eliminado exitosamente"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Success = false,
                Message = $"Error eliminando post: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse> PinPostAsync(Guid postId, bool isPinned)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            var sql = @"
                UPDATE ForumPosts
                SET IsPinned = @IsPinned
                WHERE Id = @Id";

            await db.ExecuteAsync(sql, new
            {
                IsPinned = isPinned,
                Id = postId
            });

            return new ApiResponse
            {
                Success = true,
                Message = isPinned ? "Post fijado exitosamente" : "Post desfijado exitosamente"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Success = false,
                Message = $"Error fijando post: {ex.Message}"
            };
        }
    }

    // Interacciones
    public async Task<ApiResponse> LikePostAsync(Guid userId, Guid postId)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            db.Open();

            using var transaction = db.BeginTransaction();
            try
            {
                // Verificar si ya existe el like
                var existingSql = "SELECT COUNT(*) FROM ForumLikes WHERE PostId = @PostId AND UserId = @UserId";
                var count = await db.ExecuteScalarAsync<int>(existingSql,
                    new { PostId = postId, UserId = userId }, transaction);

                if (count > 0)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Ya has dado like a este post"
                    };
                }

                // Insertar like
                var likeId = Guid.NewGuid();
                var insertLikeSql = @"
                    INSERT INTO ForumLikes (Id, PostId, UserId, CreatedAt)
                    VALUES (@Id, @PostId, @UserId, @CreatedAt)";

                await db.ExecuteAsync(insertLikeSql, new
                {
                    Id = likeId,
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                }, transaction);

                // Incrementar contador de likes
                var updateLikesSql = "UPDATE ForumPosts SET Likes = Likes + 1 WHERE Id = @PostId";
                await db.ExecuteAsync(updateLikesSql, new { PostId = postId }, transaction);

                transaction.Commit();

                return new ApiResponse
                {
                    Success = true,
                    Message = "Like agregado exitosamente"
                };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Success = false,
                Message = $"Error agregando like: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse> UnlikePostAsync(Guid userId, Guid postId)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            db.Open();

            using var transaction = db.BeginTransaction();
            try
            {
                // Eliminar like
                var deleteLikeSql = "DELETE FROM ForumLikes WHERE PostId = @PostId AND UserId = @UserId";
                var rowsAffected = await db.ExecuteAsync(deleteLikeSql,
                    new { PostId = postId, UserId = userId }, transaction);

                if (rowsAffected == 0)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "No has dado like a este post"
                    };
                }

                // Decrementar contador de likes
                var updateLikesSql = "UPDATE ForumPosts SET Likes = CASE WHEN Likes > 0 THEN Likes - 1 ELSE 0 END WHERE Id = @PostId";
                await db.ExecuteAsync(updateLikesSql, new { PostId = postId }, transaction);

                transaction.Commit();

                return new ApiResponse
                {
                    Success = true,
                    Message = "Like removido exitosamente"
                };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Success = false,
                Message = $"Error removiendo like: {ex.Message}"
            };
        }
    }

    // Estadísticas
    public async Task<ForumEngagementStatsDto?> GetForumStatsAsync(Guid forumId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var forum = await db.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT Title FROM Forums WHERE Id = @Id", new { Id = forumId });

        if (forum == null) return null;

        var stats = new ForumEngagementStatsDto
        {
            ForumId = forumId,
            ForumTitle = forum.Title
        };

        // Obtener estadísticas generales
        var statsSql = @"
            SELECT
                COUNT(*) as TotalPosts,
                (SELECT COUNT(*) FROM ForumPosts WHERE ForumId = @ForumId AND ParentPostId IS NOT NULL) as TotalReplies,
                (SELECT COUNT(DISTINCT UserId) FROM ForumPosts WHERE ForumId = @ForumId) as UniqueParticipants,
                (SELECT SUM(Likes) FROM ForumPosts WHERE ForumId = @ForumId) as TotalLikes
            FROM ForumPosts
            WHERE ForumId = @ForumId";

        var generalStats = await db.QueryFirstOrDefaultAsync<dynamic>(statsSql, new { ForumId = forumId });

        stats.TotalPosts = generalStats?.TotalPosts ?? 0;
        stats.TotalReplies = generalStats?.TotalReplies ?? 0;
        stats.UniqueParticipants = generalStats?.UniqueParticipants ?? 0;
        stats.TotalLikes = generalStats?.TotalLikes ?? 0;

        // Obtener top contributors
        var topContributorsSql = @"
            SELECT TOP 5
                up.Id as UserId,
                up.FirstName + ' ' + up.LastName as UserName,
                COUNT(fp.Id) as PostCount,
                SUM(COALESCE(fl.LikeCount, 0)) as LikeCount
            FROM Usuarios up
            LEFT JOIN ForumPosts fp ON up.Id = fp.UserId AND fp.ForumId = @ForumId
            LEFT JOIN (
                SELECT PostId, COUNT(*) as LikeCount
                FROM ForumLikes
                GROUP BY PostId
            ) fl ON fp.Id = fl.PostId
            GROUP BY up.Id, up.FirstName, up.LastName
            ORDER BY PostCount DESC, LikeCount DESC";

        var topContributors = (await db.QueryAsync<UserActivityDto>(topContributorsSql,
            new { ForumId = forumId })).ToList();

        stats.TopContributors = topContributors;

        return stats;
    }
}
