ALTER PROCEDURE usp_DashboardMaestro

    @Accion NVARCHAR(50),

    @IdUsuario UNIQUEIDENTIFIER = NULL,

    @IdCurso UNIQUEIDENTIFIER = NULL,

    @IdLeccion UNIQUEIDENTIFIER = NULL,

    @TituloLeccion NVARCHAR(200) = NULL,

    @DescripcionLeccion NVARCHAR(MAX) = NULL,

    @DuracionMinutos INT = NULL,

    @Orden INT = NULL,

    @TipoLeccion INT = NULL,

    @EsGratis BIT = NULL,

    @NombreDocumento NVARCHAR(200) = NULL,

    @RutaDocumento NVARCHAR(MAX) = NULL,

    @TipoDocumento NVARCHAR(50) = NULL,

    @IdNuevo UNIQUEIDENTIFIER = NULL OUTPUT,

    @Exito BIT = NULL OUTPUT,

    @MensajeError NVARCHAR(200) = NULL OUTPUT

AS

BEGIN

    SET NOCOUNT ON;

    SET @Exito = 0;

    SET @MensajeError = '';



    -- Obtener IdMaestro del usuario

    DECLARE @MaestroId UNIQUEIDENTIFIER;

    SELECT TOP 1 @MaestroId = IdMaestro FROM Maestros WHERE IdUsuario = @IdUsuario;



    -- =====================================================

    -- MetricasMaestro

    -- =====================================================

    IF @Accion = 'MetricasMaestro'

    BEGIN

        IF @MaestroId IS NULL

        BEGIN

            SELECT 0 AS TotalCursos, 0 AS TotalAlumnos, 0 AS TotalLecciones, 0 AS TotalDocumentos, 0 AS InscripcionesRecientes;

            SET @Exito = 1;

            RETURN;

        END



        SELECT

            (SELECT COUNT(*) FROM Cursos WHERE IdMaestro = @MaestroId AND Activo = 1) AS TotalCursos,

            (SELECT COUNT(*) FROM Inscripciones i INNER JOIN Cursos c ON i.IdCurso = c.IdCurso WHERE c.IdMaestro = @MaestroId) AS TotalAlumnos,

            (SELECT COUNT(*) FROM Lecciones l INNER JOIN Cursos c ON l.IdCurso = c.IdCurso WHERE c.IdMaestro = @MaestroId) AS TotalLecciones,

            (SELECT COUNT(*) FROM DocumentosMaestro WHERE IdMaestro = @MaestroId AND Activo = 1) AS TotalDocumentos,

            (SELECT COUNT(*) FROM Inscripciones i INNER JOIN Cursos c ON i.IdCurso = c.IdCurso WHERE c.IdMaestro = @MaestroId AND i.FechaInscripcion >= DATEADD(DAY, -30, GETUTCDATE())) AS InscripcionesRecientes;

        SET @Exito = 1;

    END



    -- =====================================================

    -- MisCursos: Cursos asignados al maestro

    -- =====================================================

    ELSE IF @Accion = 'MisCursos'

    BEGIN

        SELECT 

            c.IdCurso AS Id,

            c.Titulo AS Title,

            c.DescripcionCompleta AS [Description],

            c.DescripcionCorta AS ShortDescription,

            c.UrlImagen AS ImageUrl,

            c.Precio AS Price,

            c.NivelDificultad AS Difficulty,

            c.EsGratis AS IsFree,

            c.Destacado AS IsFeatured,

            c.DuracionHoras AS DurationHours,

            c.FechaCreacion AS CreatedDate,

            c.IdCategoria AS CategoryId,

            cat.Nombre AS CategoryName,

            (SELECT COUNT(*) FROM Lecciones l WHERE l.IdCurso = c.IdCurso) AS LessonsCount,

            (SELECT COUNT(*) FROM Inscripciones i WHERE i.IdCurso = c.IdCurso) AS StudentsEnrolled

        FROM Cursos c

        LEFT JOIN Categorias cat ON c.IdCategoria = cat.IdCategoria

        WHERE c.IdMaestro = @MaestroId AND c.Activo = 1;

        SET @Exito = 1;

    END



    -- =====================================================

    -- LeccionesPorCurso

    -- =====================================================

    ELSE IF @Accion = 'LeccionesPorCurso'

    BEGIN

        SELECT 

            l.IdLeccion AS Id,

            l.IdCurso AS CourseId,

            l.Titulo AS Title,

            l.Descripcion AS [Description],

            l.Orden AS [Order]

        FROM Lecciones l

        WHERE l.IdCurso = @IdCurso

        ORDER BY l.Orden;

        SET @Exito = 1;

    END



    -- =====================================================

    -- CrearLeccion

    -- =====================================================

    ELSE IF @Accion = 'CrearLeccion'

    BEGIN

        -- Verificar que el curso pertenece al maestro

        IF NOT EXISTS (SELECT 1 FROM Cursos WHERE IdCurso = @IdCurso AND IdMaestro = @MaestroId AND Activo = 1)

        BEGIN

            SET @MensajeError = 'No tienes permiso para este curso.';

            RETURN;

        END



        SET @IdNuevo = NEWID();

        INSERT INTO Lecciones (IdLeccion, IdCurso, Titulo, Descripcion, Orden)

        VALUES (@IdNuevo, @IdCurso, @TituloLeccion, @DescripcionLeccion, ISNULL(@Orden, 1));

        SET @Exito = 1;

    END



    -- =====================================================

    -- ActualizarLeccion

    -- =====================================================

    ELSE IF @Accion = 'ActualizarLeccion'

    BEGIN

        UPDATE Lecciones

        SET Titulo = ISNULL(@TituloLeccion, Titulo),

            Descripcion = ISNULL(@DescripcionLeccion, Descripcion),

            Orden = ISNULL(@Orden, Orden)

        WHERE IdLeccion = @IdLeccion;



        IF @@ROWCOUNT = 0

        BEGIN SET @MensajeError = 'Lección no encontrada.'; RETURN; END

        SET @Exito = 1;

    END



    -- =====================================================

    -- EliminarLeccion

    -- =====================================================

    ELSE IF @Accion = 'EliminarLeccion'

    BEGIN

        DELETE FROM Lecciones WHERE IdLeccion = @IdLeccion;

        IF @@ROWCOUNT = 0

        BEGIN SET @MensajeError = 'Lección no encontrada.'; RETURN; END

        SET @Exito = 1;

    END



    -- =====================================================

    -- InscripcionesPorCurso

    -- =====================================================

    ELSE IF @Accion = 'InscripcionesPorCurso'

    BEGIN

        SELECT 

            i.IdInscripcion AS Id,

            i.IdCurso AS CourseId,

            i.IdUsuario AS UserId,

            i.FechaInscripcion AS EnrolledDate,

            i.Progreso AS Progress,

            i.Completado AS IsCompleted,

            c.Titulo AS CourseName,

            (u.Nombre + ' ' + u.Apellidos) AS StudentName,

            u.Email AS StudentEmail,

            '' AS InstructorName

        FROM Inscripciones i

        INNER JOIN Cursos c ON i.IdCurso = c.IdCurso

        INNER JOIN Usuarios u ON i.IdUsuario = u.IdUsuario

        WHERE i.IdCurso = @IdCurso

        ORDER BY i.FechaInscripcion DESC;

        SET @Exito = 1;

    END



    -- =====================================================

    -- TodasMisInscripciones

    -- =====================================================

    ELSE IF @Accion = 'TodasMisInscripciones'

    BEGIN

        SELECT 

            i.IdInscripcion AS Id,

            i.IdCurso AS CourseId,

            i.IdUsuario AS UserId,

            i.FechaInscripcion AS EnrolledDate,

            i.Progreso AS Progress,

            i.Completado AS IsCompleted,

            c.Titulo AS CourseName,

            (u.Nombre + ' ' + u.Apellidos) AS StudentName,

            u.Email AS StudentEmail,

            '' AS InstructorName

        FROM Inscripciones i

        INNER JOIN Cursos c ON i.IdCurso = c.IdCurso

        INNER JOIN Usuarios u ON i.IdUsuario = u.IdUsuario

        WHERE c.IdMaestro = @MaestroId

        ORDER BY i.FechaInscripcion DESC;

        SET @Exito = 1;

    END



    -- =====================================================

    -- MisDocumentos

    -- =====================================================

    ELSE IF @Accion = 'MisDocumentos'

    BEGIN

        SELECT 

            d.IdDocumento AS Id,

           d.IdMaestro,

            d.IdCurso,

            d.NombreArchivo,

            d.UrlArchivo,

            d.TipoDocumento,

            d.FechaSubida,

            ISNULL(c.Titulo, 'Sin curso') AS NombreCursoAsociado

        FROM DocumentosMaestro d

        LEFT JOIN Cursos c ON d.IdCurso = c.IdCurso

        WHERE d.IdMaestro = @MaestroId AND d.Activo = 1

        ORDER BY d.FechaSubida DESC;

        SET @Exito = 1;

    END



    -- =====================================================

    -- SubirDocumento

    -- =====================================================

    ELSE IF @Accion = 'SubirDocumento'

    BEGIN

        SET @IdNuevo = NEWID();

        INSERT INTO DocumentosMaestro (IdDocumento, IdMaestro, IdCurso, NombreArchivo, UrlArchivo, TipoDocumento, FechaSubida, Activo)

        VALUES (@IdNuevo, @MaestroId, @IdCurso, @NombreDocumento, @RutaDocumento, ISNULL(@TipoDocumento, 'Otro'), GETUTCDATE(), 1);

        SET @Exito = 1;

    END

END

