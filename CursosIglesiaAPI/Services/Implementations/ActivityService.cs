using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace CursosIglesia.Services.Implementations;

public class ActivityService : IActivityService
{
    private readonly string _connectionString;

    public ActivityService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    // CRUD de Actividades
    public async Task<ApiResponse<Guid>> CreateActivityAsync(Guid userId, CreateActivityRequest request)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            db.Open();

            using var transaction = db.BeginTransaction();
            try
            {
                var activityId = Guid.NewGuid();
                var now = DateTime.UtcNow;

                // Insertar actividad
                var insertActivitySql = @"
                    INSERT INTO Activities (IdActividad, IdTema, Tipo, Titulo, Descripcion, Instrucciones,
                        FechaEntrega, PuntosMaximos, MostrarRespuestas, PermitirEnvioTarde, IdUsuarioCreador,
                        FechaCreacion, FechaActualizacion)
                    VALUES (@IdActividad, @IdTema, @Tipo, @Titulo, @Descripcion, @Instrucciones,
                        @FechaEntrega, @PuntosMaximos, @MostrarRespuestas, @PermitirEnvioTarde, @IdUsuarioCreador,
                        @FechaCreacion, @FechaActualizacion)";

                await db.ExecuteAsync(insertActivitySql, new
                {
                    IdActividad = activityId,
                    request.IdTema,
                    request.Tipo,
                    request.Titulo,
                    request.Descripcion,
                    request.Instrucciones,
                    request.FechaEntrega,
                    request.PuntosMaximos,
                    request.MostrarRespuestas,
                    request.PermitirEnvioTarde,
                    IdUsuarioCreador = userId,
                    FechaCreacion = now,
                    FechaActualizacion = now
                }, transaction);

                // Insertar preguntas
                foreach (var question in request.Preguntas)
                {
                    var questionId = Guid.NewGuid();
                    var insertQuestionSql = @"
                        INSERT INTO ActivityQuestions (IdActivityQuestion, IdActividad, TextoPregunta, TipoPregunta,
                            Orden, EsObligatoria, Puntos, Opciones, RespuestaCorrecta, TextoFeedback, FechaCreacion)
                        VALUES (@IdActivityQuestion, @IdActividad, @TextoPregunta, @TipoPregunta,
                            @Orden, @EsObligatoria, @Puntos, @Opciones, @RespuestaCorrecta, @TextoFeedback, @FechaCreacion)";

                    var optionsJson = question.Opciones != null ? JsonSerializer.Serialize(question.Opciones) : null;

                    await db.ExecuteAsync(insertQuestionSql, new
                    {
                        IdActivityQuestion = questionId,
                        IdActividad = activityId,
                        question.TextoPregunta,
                        question.TipoPregunta,
                        question.Orden,
                        question.EsObligatoria,
                        question.Puntos,
                        Opciones = optionsJson,
                        question.RespuestaCorrecta,
                        question.TextoFeedback,
                        FechaCreacion = now
                    }, transaction);
                }

                transaction.Commit();

                return new ApiResponse<Guid>
                {
                    Success = true,
                    Data = activityId,
                    Message = "Actividad creada exitosamente"
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
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = $"Error creando actividad: {ex.Message}"
            };
        }
    }

    public async Task<Activity?> GetActivityAsync(Guid activityId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var sql = @"
            SELECT * FROM Activities WHERE IdActividad = @Id;
            SELECT * FROM ActivityQuestions WHERE IdActividad = @Id ORDER BY Orden";

        using var reader = await db.QueryMultipleAsync(sql, new { Id = activityId });

        var activity = await reader.ReadFirstOrDefaultAsync<Activity>();
        if (activity == null) return null;

        var questions = (await reader.ReadAsync<ActivityQuestion>()).ToList();
        activity.Questions = questions;

        return activity;
    }

    public async Task<List<Activity>> GetActivitiesByTemaAsync(Guid temaId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var sql = @"SELECT * FROM Activities WHERE IdTema = @TemaId ORDER BY FechaCreacion DESC";
        var activities = (await db.QueryAsync<Activity>(sql, new { TemaId = temaId })).ToList();

        return activities;
    }

    public async Task<ApiResponse> UpdateActivityAsync(Guid activityId, CreateActivityRequest request)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            db.Open();

            using var transaction = db.BeginTransaction();
            try
            {
                var now = DateTime.UtcNow;

                // Actualizar actividad
                var updateActivitySql = @"
                    UPDATE Activities SET 
                        Tipo = @Tipo, Titulo = @Titulo, Descripcion = @Descripcion, 
                        Instrucciones = @Instrucciones, FechaEntrega = @FechaEntrega, 
                        PuntosMaximos = @PuntosMaximos, MostrarRespuestas = @MostrarRespuestas,
                        PermitirEnvioTarde = @PermitirEnvioTarde, FechaActualizacion = @FechaActualizacion
                    WHERE IdActividad = @IdActividad";

                await db.ExecuteAsync(updateActivitySql, new
                {
                    IdActividad = activityId,
                    request.Tipo,
                    request.Titulo,
                    request.Descripcion,
                    request.Instrucciones,
                    request.FechaEntrega,
                    request.PuntosMaximos,
                    request.MostrarRespuestas,
                    request.PermitirEnvioTarde,
                    FechaActualizacion = now
                }, transaction);

                // Eliminar preguntas anteriores
                await db.ExecuteAsync("DELETE FROM ActivityQuestions WHERE IdActividad = @ActivityId",
                    new { ActivityId = activityId }, transaction);

                // Insertar nuevas preguntas
                foreach (var question in request.Preguntas)
                {
                    var questionId = Guid.NewGuid();
                    var insertQuestionSql = @"
                        INSERT INTO ActivityQuestions (IdActivityQuestion, IdActividad, TextoPregunta, TipoPregunta,
                            Orden, EsObligatoria, Puntos, Opciones, RespuestaCorrecta, TextoFeedback, FechaCreacion)
                        VALUES (@IdActivityQuestion, @IdActividad, @TextoPregunta, @TipoPregunta,
                            @Orden, @EsObligatoria, @Puntos, @Opciones, @RespuestaCorrecta, @TextoFeedback, @FechaCreacion)";

                    var optionsJson = question.Opciones != null ? JsonSerializer.Serialize(question.Opciones) : null;

                    await db.ExecuteAsync(insertQuestionSql, new
                    {
                        IdActivityQuestion = questionId,
                        IdActividad = activityId,
                        question.TextoPregunta,
                        question.TipoPregunta,
                        question.Orden,
                        question.EsObligatoria,
                        question.Puntos,
                        Opciones = optionsJson,
                        question.RespuestaCorrecta,
                        question.TextoFeedback,
                        FechaCreacion = now
                    }, transaction);
                }

                transaction.Commit();

                return new ApiResponse
                {
                    Success = true,
                    Message = "Actividad actualizada exitosamente"
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
                Message = $"Error actualizando actividad: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse> DeleteActivityAsync(Guid activityId)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            var sql = "DELETE FROM Activities WHERE IdActividad = @Id";
            var rows = await db.ExecuteAsync(sql, new { Id = activityId });
            return new ApiResponse
            {
                Success = rows > 0,
                Message = rows > 0 ? "Actividad eliminada" : "Actividad no encontrada"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Success = false,
                Message = $"Error eliminando actividad: {ex.Message}"
            };
        }
    }

    // Respuestas de Estudiantes
    public async Task<ApiResponse> SubmitResponseAsync(Guid userId, SubmitActivityResponseRequest request)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            db.Open();

            using var transaction = db.BeginTransaction();
            try
            {
                var now = DateTime.UtcNow;

                // Insertar todas las respuestas recibidas (incluyendo archivos)
                foreach (var entry in request.Respuestas)
                {
                    if (string.IsNullOrEmpty(entry.Value)) continue;

                    var responseId = Guid.NewGuid();
                    var insertResponseSql = @"
                        INSERT INTO ActivityResponses (IdActivityResponse, IdActividad, IdPregunta, IdUsuario, Respuesta, FechaEnvio, FechaCreacion)
                        VALUES (@Id, @IdActividad, @IdPregunta, @IdUsuario, @Respuesta, @FechaEnvio, @FechaCreacion)";

                    await db.ExecuteAsync(insertResponseSql, new
                    {
                        Id = responseId,
                        request.IdActividad,
                        IdPregunta = entry.Key,
                        IdUsuario = userId,
                        Respuesta = entry.Value,
                        FechaEnvio = now,
                        FechaCreacion = now
                    }, transaction);
                }

                transaction.Commit();

                return new ApiResponse
                {
                    Success = true,
                    Message = "Respuestas enviadas exitosamente"
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
                Message = $"Error enviando respuestas: {ex.Message}"
            };
        }
    }

    public async Task<List<ActivityResponseDto>> GetActivityResponsesAsync(Guid activityId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var sql = @"
            SELECT ar.IdActivityResponse as Id, ar.IdActividad as IdActividad, ar.IdUsuario as IdUsuario, 
                   ar.IdPregunta as IdPregunta, COALESCE(aq.TextoPregunta, 'Archivo de Entrega') as TextoPregunta, 
                   ar.Respuesta as Respuesta, ar.PuntosGanados as PuntosGanados, 
                   ar.FechaEnvio as FechaEnvio, ar.Feedback
            FROM ActivityResponses ar
            LEFT JOIN ActivityQuestions aq ON ar.IdPregunta = aq.IdActivityQuestion
            WHERE ar.IdActividad = @ActivityId
            ORDER BY ar.FechaEnvio DESC";

        var responses = (await db.QueryAsync<ActivityResponseDto>(sql, new { ActivityId = activityId })).ToList();
        return responses;
    }

    public async Task<List<ActivityResponseDto>> GetStudentResponseAsync(Guid userId, Guid activityId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var sql = @"
            SELECT ar.IdActivityResponse as Id, ar.IdActividad as IdActividad, ar.IdUsuario as IdUsuario, 
                   ar.IdPregunta as IdPregunta, COALESCE(aq.TextoPregunta, 'Archivo de Entrega') as TextoPregunta, 
                   ar.Respuesta as Respuesta, ar.PuntosGanados as PuntosGanados, 
                   ar.FechaEnvio as FechaEnvio, ar.Feedback
            FROM ActivityResponses ar
            LEFT JOIN ActivityQuestions aq ON ar.IdPregunta = aq.IdActivityQuestion
            WHERE ar.IdActividad = @ActivityId AND ar.IdUsuario = @UserId
            ORDER BY ar.FechaEnvio DESC";

        var responses = (await db.QueryAsync<ActivityResponseDto>(sql,
            new { ActivityId = activityId, UserId = userId })).ToList();
        return responses;
    }

    public async Task<List<StudentActivityResponseItem>> GetStudentSubmissionsAsync(Guid activityId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var sql = @"
            SELECT
                up.IdUsuario as IdUsuario,
                up.Nombre + ' ' + up.Apellidos as NombreEstudiante,
                up.Email as EmailEstudiante,
                MAX(ar.FechaEnvio) as FechaEnvio,
                SUM(ar.PuntosGanados) as TotalPuntosGanados,
                (SELECT PuntosMaximos FROM Activities WHERE IdActividad = @ActivityId) as PuntosMaximos,
                CASE WHEN MAX(ar.FechaEnvio) IS NOT NULL THEN 1 ELSE 0 END as EstaCompletado
            FROM Usuarios up
            LEFT JOIN ActivityResponses ar ON up.IdUsuario = ar.IdUsuario AND ar.IdActividad = @ActivityId
            WHERE up.IdUsuario IN (SELECT IdUsuario FROM Inscripciones WHERE IdCurso = (
                SELECT l.IdCurso FROM Lecciones l INNER JOIN Temas t ON l.IdLeccion = t.IdLeccion INNER JOIN Activities a ON t.IdTema = a.IdTema WHERE a.IdActividad = @ActivityId))
            GROUP BY up.IdUsuario, up.Nombre, up.Apellidos, up.Email";

        var submissions = (await db.QueryAsync<StudentActivityResponseItem>(sql,
            new { ActivityId = activityId })).ToList();
        return submissions;
    }

    public async Task<ApiResponse> AddFeedbackAsync(AddFeedbackRequest request)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            var sql = @"
                UPDATE ActivityResponses
                SET PuntosGanados = @PuntosGanados, Feedback = @Feedback
                WHERE IdActivityResponse = @IdRespuesta";

            await db.ExecuteAsync(sql, new
            {
                request.IdRespuesta,
                request.PuntosGanados,
                request.Feedback
            });

            return new ApiResponse
            {
                Success = true,
                Message = "Feedback agregado exitosamente"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Success = false,
                Message = $"Error agregando feedback: {ex.Message}"
            };
        }
    }

    public async Task<ActivityStatsDto?> GetActivityStatsAsync(Guid activityId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var activity = await db.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT Titulo FROM Activities WHERE IdActividad = @Id", new { Id = activityId });

        if (activity == null) return null;

        var stats = new ActivityStatsDto
        {
            IdActividad = activityId,
            TituloActividad = activity.Titulo
        };

        // Obtener estadísticas generales
        var generalStatsSql = @"
            SELECT 
                COUNT(DISTINCT ar.IdUsuario) as TotalEstudiantes,
                COUNT(DISTINCT ar.IdUsuario) as RespuestasCompletadas,
                AVG(CAST(ar.PuntosGanados as FLOAT)) as PuntajePromedio
            FROM ActivityResponses ar
            WHERE ar.IdActividad = @ActivityId";

        var generalStats = await db.QueryFirstOrDefaultAsync<dynamic>(generalStatsSql, new { ActivityId = activityId });

        if (generalStats != null && generalStats.TotalEstudiantes > 0)
        {
            stats.TotalEstudiantes = (int)generalStats.TotalEstudiantes;
            stats.RespuestasCompletadas = (int)generalStats.RespuestasCompletadas;
            stats.PorcentajeCompletado = (stats.RespuestasCompletadas / (double)stats.TotalEstudiantes) * 100;
            stats.PuntajePromedio = generalStats.PuntajePromedio ?? 0;
        }

        // Obtener estadísticas por pregunta
        var questionStatsSql = @"
            SELECT 
                aq.IdActivityQuestion as IdPregunta,
                aq.TextoPregunta as TextoPregunta,
                aq.TipoPregunta as TipoPregunta,
                COUNT(ar.IdActivityResponse) as TotalRespuestas,
                AVG(CAST(ar.PuntosGanados as FLOAT)) as PromedioPuntos
            FROM ActivityQuestions aq
            LEFT JOIN ActivityResponses ar ON aq.IdActivityQuestion = ar.IdPregunta
            WHERE aq.IdActividad = @ActivityId
            GROUP BY aq.IdActivityQuestion, aq.TextoPregunta, aq.TipoPregunta";

        var questionStats = (await db.QueryAsync<dynamic>(questionStatsSql, new { ActivityId = activityId })).ToList();

        foreach (var qs in questionStats)
        {
            stats.EstadisticasPreguntas.Add(new QuestionStatsDto
            {
                IdPregunta = qs.IdPregunta,
                TextoPregunta = qs.TextoPregunta,
                TipoPregunta = (ActivityQuestionType)qs.TipoPregunta,
                TotalRespuestas = qs.TotalRespuestas,
                PromedioPuntos = qs.PromedioPuntos ?? 0
            });
        }

        return stats;
    }

    public async Task<bool> HasUngradedActivitiesAsync(Guid userId, Guid courseId)
    {
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            var sql = @"
                SELECT COUNT(*)
                FROM Activities a
                INNER JOIN Temas t ON a.IdTema = t.IdTema
                INNER JOIN Lecciones l ON t.IdLeccion = l.IdLeccion
                WHERE l.IdCurso = @CourseId
                AND a.IdActividad NOT IN (
                    SELECT DISTINCT IdActividad 
                    FROM ActivityResponses 
                    WHERE IdUsuario = @UserId 
                    AND PuntosGanados IS NOT NULL
                )";

            int ungradedCount = await db.ExecuteScalarAsync<int>(sql, new { UserId = userId, CourseId = courseId });
            return ungradedCount > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HasUngradedActivitiesAsync] DATABASE ERROR: {ex.Message}");
            throw;
        }
    }
}
