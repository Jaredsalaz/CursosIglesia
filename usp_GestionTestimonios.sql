USE [CursosIglesia]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_GestionTestimonios]
    @Accion NVARCHAR(50),
    @IdTestimonio UNIQUEIDENTIFIER = NULL,
    @IdUsuario UNIQUEIDENTIFIER = NULL,
    @IdCurso UNIQUEIDENTIFIER = NULL,
    @Comentario NVARCHAR(MAX) = NULL,
    @Calificacion INT = NULL,
    @Top INT = NULL,
    @Exito BIT = 0 OUTPUT,
    @MensajeError NVARCHAR(MAX) = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Exito = 0;
    SET @MensajeError = '';

    -- 1. Listar testimonios aprobados de forma global (para el Home)
    IF @Accion = 'ListarAprobados'
    BEGIN
        SELECT TOP (ISNULL(@Top, 6))
            t.IdTestimonio AS Id, 
            t.IdUsuario AS UserId,
            t.IdCurso AS CourseId,
            u.Nombre + ' ' + u.Apellidos AS StudentName, 
            u.UrlAvatar AS StudentImageUrl, 
            c.Titulo AS CourseName, 
            t.Comentario AS Comment, 
            t.Calificacion AS Rating, 
            t.Aprobado AS IsApproved,
            t.FechaTestimonio AS Date 
        FROM Testimonios t
        JOIN Usuarios u ON t.IdUsuario = u.IdUsuario
        JOIN Cursos c ON t.IdCurso = c.IdCurso
        WHERE t.Aprobado = 1
        ORDER BY t.FechaTestimonio DESC;
        
        SET @Exito = 1;
    END

    -- 2. Listar testimonios aprobados por curso
    ELSE IF @Accion = 'ListarPorCurso'
    BEGIN
        SELECT 
            t.IdTestimonio AS Id, 
            t.IdUsuario AS UserId,
            t.IdCurso AS CourseId,
            u.Nombre + ' ' + u.Apellidos AS StudentName, 
            u.UrlAvatar AS StudentImageUrl, 
            c.Titulo AS CourseName, 
            t.Comentario AS Comment, 
            t.Calificacion AS Rating, 
            t.Aprobado AS IsApproved,
            t.FechaTestimonio AS Date 
        FROM Testimonios t
        JOIN Usuarios u ON t.IdUsuario = u.IdUsuario
        JOIN Cursos c ON t.IdCurso = c.IdCurso
        WHERE t.IdCurso = @IdCurso AND t.Aprobado = 1
        ORDER BY t.FechaTestimonio DESC;
        
        SET @Exito = 1;
    END

    -- 3. Insertar o Actualizar testimonio (UPSERT)
    ELSE IF @Accion = 'InsertarOActualizar'
    BEGIN
        BEGIN TRY
            IF EXISTS (SELECT 1 FROM Testimonios WHERE IdUsuario = @IdUsuario AND IdCurso = @IdCurso)
            BEGIN
                UPDATE Testimonios 
                SET Comentario = @Comentario, 
                    Calificacion = @Calificacion, 
                    Aprobado = 0, -- Vuelve a requerir aprobación si se edita
                    FechaTestimonio = GETUTCDATE()
                WHERE IdUsuario = @IdUsuario AND IdCurso = @IdCurso;
            END
            ELSE
            BEGIN
                INSERT INTO Testimonios (IdTestimonio, IdUsuario, IdCurso, Comentario, Calificacion, Aprobado, FechaTestimonio)
                VALUES (NEWID(), @IdUsuario, @IdCurso, @Comentario, @Calificacion, 0, GETUTCDATE());
            END
            
            SET @Exito = 1;
        END TRY
        BEGIN CATCH
            SET @Exito = 0;
            SET @MensajeError = ERROR_MESSAGE();
        END CATCH
    END

    -- 4. Aprobar testimonio (Acción administrativa)
    ELSE IF @Accion = 'Aprobar'
    BEGIN
        IF EXISTS (SELECT 1 FROM Testimonios WHERE IdTestimonio = @IdTestimonio)
        BEGIN
            UPDATE Testimonios SET Aprobado = 1 WHERE IdTestimonio = @IdTestimonio;
            SET @Exito = 1;
        END
        ELSE
        BEGIN
            SET @Exito = 0;
            SET @MensajeError = 'Testimonio no encontrado.';
        END
    END

    -- 5. Listar testimonios pendientes (Para administración)
    ELSE IF @Accion = 'ListarPendientes'
    BEGIN
        SELECT 
            t.IdTestimonio AS Id, 
            t.IdUsuario AS UserId,
            t.IdCurso AS CourseId,
            u.Nombre + ' ' + u.Apellidos AS StudentName, 
            u.UrlAvatar AS StudentImageUrl, 
            c.Titulo AS CourseName, 
            t.Comentario AS Comment, 
            t.Calificacion AS Rating, 
            t.Aprobado AS IsApproved,
            t.FechaTestimonio AS Date 
        FROM Testimonios t
        JOIN Usuarios u ON t.IdUsuario = u.IdUsuario
        JOIN Cursos c ON t.IdCurso = c.IdCurso
        WHERE t.Aprobado = 0
        ORDER BY t.FechaTestimonio ASC; -- Los más antiguos primero para moderación
        
        SET @Exito = 1;
    END
END
GO
