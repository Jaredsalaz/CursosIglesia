USE CursosIglesia;
GO

-- =====================================================
-- 1. Crear tabla ArchivosTema
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ArchivosTema')
BEGIN
    CREATE TABLE ArchivosTema (
        IdArchivo      UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        IdTema         UNIQUEIDENTIFIER NOT NULL,
        NombreOriginal NVARCHAR(500)    NOT NULL,
        NombreServidor NVARCHAR(500)    NOT NULL,
        RutaArchivo    NVARCHAR(MAX)    NOT NULL,
        TipoArchivo    NVARCHAR(50)     NOT NULL,
        TamanoBytes    BIGINT           NOT NULL DEFAULT 0,
        FechaSubida    DATETIME2        NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_ArchivosTema_Temas
            FOREIGN KEY (IdTema) REFERENCES Temas(IdTema) ON DELETE CASCADE
    );
    PRINT 'OK Tabla ArchivosTema creada.';
END
ELSE
    PRINT 'SKIP Tabla ArchivosTema ya existe.';
GO

-- =====================================================
-- 2. Recrear usp_DashboardTemas con soporte de archivos
-- =====================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'usp_DashboardTemas')
    DROP PROCEDURE usp_DashboardTemas;
GO

CREATE PROCEDURE usp_DashboardTemas
    @Accion           NVARCHAR(50),
    -- Parametros temas
    @IdUsuario        UNIQUEIDENTIFIER = NULL,
    @IdLeccion        UNIQUEIDENTIFIER = NULL,
    @IdTema           UNIQUEIDENTIFIER = NULL,
    @Titulo           NVARCHAR(200)    = NULL,
    @DescripcionCorta NVARCHAR(MAX)    = NULL,
    @ContenidoTexto   NVARCHAR(MAX)    = NULL,
    @UrlContenido     NVARCHAR(MAX)    = NULL,
    @TipoContenido    INT              = NULL,
    @DuracionMinutos  INT              = NULL,
    @Orden            INT              = NULL,
    @EsGratis         BIT              = NULL,
    -- Parametros archivos
    @IdArchivo        UNIQUEIDENTIFIER = NULL,
    @NombreOriginal   NVARCHAR(500)    = NULL,
    @NombreServidor   NVARCHAR(500)    = NULL,
    @RutaArchivo      NVARCHAR(MAX)    = NULL,
    @TipoArchivo      NVARCHAR(50)     = NULL,
    @TamanoBytes      BIGINT           = NULL,
    -- Output
    @IdNuevo          UNIQUEIDENTIFIER = NULL OUTPUT,
    @Exito            BIT              = NULL OUTPUT,
    @MensajeError     NVARCHAR(200)    = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Exito = 0;
    SET @MensajeError = '';

    -- Verificar que el usuario sea maestro
    DECLARE @MaestroId UNIQUEIDENTIFIER;
    IF @IdUsuario IS NOT NULL
    BEGIN
        SELECT TOP 1 @MaestroId = IdMaestro
        FROM Maestros WHERE IdUsuario = @IdUsuario;

        IF @MaestroId IS NULL
        BEGIN
            SET @MensajeError = 'El usuario no es un maestro.';
            RETURN;
        END
    END

    -- Seguridad: verificar que el maestro sea dueno de la leccion
    IF @IdLeccion IS NOT NULL AND @IdUsuario IS NOT NULL
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM Lecciones l
            INNER JOIN Cursos c ON l.IdCurso = c.IdCurso
            WHERE l.IdLeccion = @IdLeccion AND c.IdMaestro = @MaestroId
        )
        BEGIN
            SET @MensajeError = 'No tienes permiso para modificar este modulo.';
            RETURN;
        END
    END

    -- ListarTemasPorLeccion
    IF @Accion = 'ListarTemasPorLeccion'
    BEGIN
        SELECT
            IdTema           AS Id,
            IdLeccion        AS LessonId,
            Orden            AS [Order],
            Titulo           AS Title,
            DescripcionCorta AS [Description],
            TipoContenido    AS ContentType,
            ContenidoTexto   AS TextContent,
            UrlContenido     AS ContentUrl,
            DuracionMinutos  AS DurationMinutes,
            EsGratis         AS IsFree
        FROM Temas
        WHERE IdLeccion = @IdLeccion
        ORDER BY Orden;
        SET @Exito = 1;
    END

    -- CrearTema
    ELSE IF @Accion = 'CrearTema'
    BEGIN
        SET @IdNuevo = NEWID();
        INSERT INTO Temas
            (IdTema, IdLeccion, Orden, Titulo, DescripcionCorta,
             TipoContenido, ContenidoTexto, UrlContenido, DuracionMinutos, EsGratis)
        VALUES
            (@IdNuevo, @IdLeccion, ISNULL(@Orden,1), @Titulo, @DescripcionCorta,
             ISNULL(@TipoContenido,0), @ContenidoTexto, @UrlContenido,
             ISNULL(@DuracionMinutos,0), ISNULL(@EsGratis,0));
        SET @Exito = 1;
    END

    -- ActualizarTema
    ELSE IF @Accion = 'ActualizarTema'
    BEGIN
        UPDATE Temas SET
            Titulo           = ISNULL(@Titulo,           Titulo),
            DescripcionCorta = ISNULL(@DescripcionCorta, DescripcionCorta),
            TipoContenido    = ISNULL(@TipoContenido,    TipoContenido),
            ContenidoTexto   = ISNULL(@ContenidoTexto,   ContenidoTexto),
            UrlContenido     = ISNULL(@UrlContenido,     UrlContenido),
            DuracionMinutos  = ISNULL(@DuracionMinutos,  DuracionMinutos),
            Orden            = ISNULL(@Orden,            Orden),
            EsGratis         = ISNULL(@EsGratis,         EsGratis)
        WHERE IdTema = @IdTema;

        IF @@ROWCOUNT = 0
        BEGIN SET @MensajeError = 'Tema no encontrado.'; RETURN; END
        SET @Exito = 1;
    END

    -- EliminarTema
    ELSE IF @Accion = 'EliminarTema'
    BEGIN
        DELETE FROM Temas WHERE IdTema = @IdTema;
        IF @@ROWCOUNT = 0
        BEGIN SET @MensajeError = 'Tema no encontrado.'; RETURN; END
        SET @Exito = 1;
    END

    -- ListarArchivosPorTema
    ELSE IF @Accion = 'ListarArchivosPorTema'
    BEGIN
        SELECT
            IdArchivo      AS Id,
            IdTema         AS TemaId,
            NombreOriginal,
            NombreServidor,
            RutaArchivo,
            TipoArchivo,
            TamanoBytes,
            FechaSubida
        FROM ArchivosTema
        WHERE IdTema = @IdTema
        ORDER BY FechaSubida;
        SET @Exito = 1;
    END

    -- AgregarArchivoTema  (maximo 3 por tema)
    ELSE IF @Accion = 'AgregarArchivoTema'
    BEGIN
        DECLARE @Total INT;
        SELECT @Total = COUNT(*) FROM ArchivosTema WHERE IdTema = @IdTema;

        IF @Total >= 3
        BEGIN
            SET @MensajeError = 'Maximo 3 archivos por tema.';
            RETURN;
        END

        SET @IdNuevo = NEWID();
        INSERT INTO ArchivosTema
            (IdArchivo, IdTema, NombreOriginal, NombreServidor,
             RutaArchivo, TipoArchivo, TamanoBytes)
        VALUES
            (@IdNuevo, @IdTema, @NombreOriginal, @NombreServidor,
             @RutaArchivo, @TipoArchivo, ISNULL(@TamanoBytes,0));
        SET @Exito = 1;
    END

    -- EliminarArchivoTema
    -- Retorna RutaArchivo ANTES de borrar para que el API elimine el fichero del disco
    ELSE IF @Accion = 'EliminarArchivoTema'
    BEGIN
        SELECT RutaArchivo FROM ArchivosTema WHERE IdArchivo = @IdArchivo;

        DELETE FROM ArchivosTema WHERE IdArchivo = @IdArchivo;

        IF @@ROWCOUNT = 0
        BEGIN SET @MensajeError = 'Archivo no encontrado.'; RETURN; END
        SET @Exito = 1;
    END
END
GO

PRINT 'OK usp_DashboardTemas recreado con soporte de archivos de apoyo.';
GO
