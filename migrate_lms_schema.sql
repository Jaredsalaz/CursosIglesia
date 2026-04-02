USE CursosIglesia;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Create Temas table
    CREATE TABLE Temas (
        IdTema UNIQUEIDENTIFIER PRIMARY KEY,
        IdLeccion UNIQUEIDENTIFIER NOT NULL,
        Orden INT NOT NULL DEFAULT 1,
        Titulo NVARCHAR(200) NOT NULL,
        DescripcionCorta NVARCHAR(MAX) NULL,
        TipoContenido INT NOT NULL DEFAULT 0,
        ContenidoTexto NVARCHAR(MAX) NULL,
        UrlContenido NVARCHAR(MAX) NULL,
        DuracionMinutos INT NOT NULL DEFAULT 0,
        EsGratis BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Temas_Lecciones FOREIGN KEY (IdLeccion) REFERENCES Lecciones(IdLeccion) ON DELETE CASCADE
    );

    PRINT 'Tabla Temas creada.';

    -- 2. Migrate existing data from Lecciones to Temas
    -- We assume the old Lecciones had TipoLeccion, UrlContenido, DuracionMinutos, EsGratis
    INSERT INTO Temas (IdTema, IdLeccion, Orden, Titulo, DescripcionCorta, TipoContenido, UrlContenido, DuracionMinutos, EsGratis)
    SELECT 
        NEWID(), 
        IdLeccion, 
        1, 
        Titulo, 
        Descripcion, 
        ISNULL(TipoLeccion, 0), 
        UrlContenido, 
        ISNULL(DuracionMinutos, 0), 
        ISNULL(EsGratis, 0)
    FROM Lecciones;

    PRINT 'Datos migrados a Temas.';

    -- 3. Modify Lecciones Table (Modules)
    ALTER TABLE Lecciones DROP COLUMN TipoLeccion;
    ALTER TABLE Lecciones DROP COLUMN UrlContenido;
    ALTER TABLE Lecciones DROP COLUMN DuracionMinutos;
    ALTER TABLE Lecciones DROP COLUMN EsGratis;

    PRINT 'Tabla Lecciones actualizada (columnas removidas).';

    COMMIT TRANSACTION;
    PRINT 'Migración completada exitosamente.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    PRINT 'ERROR en la migración: ' + ERROR_MESSAGE();
END CATCH
GO
