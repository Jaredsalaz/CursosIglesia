USE [CursosIglesia]
GO

ALTER PROCEDURE [dbo].[usp_AutenticacionYUsuarios]
    @Accion NVARCHAR(50),  -- Ej: 'RegistrarUsuario', 'Login', 'ObtenerPerfil', 'ActualizarPerfil', 'CambiarPassword'
    @IdUsuario UNIQUEIDENTIFIER = NULL,
    @Nombre NVARCHAR(100) = NULL,
    @Apellidos NVARCHAR(100) = NULL,
    @Email NVARCHAR(150) = NULL,
    @PasswordHash NVARCHAR(MAX) = NULL,
    @Parroquia NVARCHAR(200) = NULL,
    @Pais NVARCHAR(100) = NULL,
    @UrlAvatar NVARCHAR(MAX) = NULL,
    @Telefono NVARCHAR(20) = NULL,
    @Biografia NVARCHAR(MAX) = NULL,
    @Ciudad NVARCHAR(100) = NULL,
    @FechaNacimiento DATETIME = NULL,
    @IdUsuarioNuevo UNIQUEIDENTIFIER = NULL OUTPUT,
    @Exito BIT = NULL OUTPUT,
    @MensajeError NVARCHAR(200) = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Exito = 0;
    SET @MensajeError = '';

    -- =====================================================
    IF @Accion = 'RegistrarUsuario'
    BEGIN
        IF EXISTS(SELECT 1 FROM Usuarios WHERE Email = @Email)
        BEGIN
            SET @MensajeError = 'El correo electrónico ya está registrado.';
            RETURN;
        END

        DECLARE @RolBasicoId UNIQUEIDENTIFIER;
        SELECT TOP 1 @RolBasicoId = IdRol FROM Roles WHERE NombreRol = 'Estudiante';
        IF @RolBasicoId IS NULL 
        BEGIN
            SET @RolBasicoId = NEWID();
            INSERT INTO Roles (IdRol, NombreRol, Descripcion) VALUES (@RolBasicoId, 'Estudiante', 'Rol básico');
        END

        SET @IdUsuarioNuevo = NEWID();

        INSERT INTO Usuarios (IdUsuario, IdRol, Nombre, Apellidos, Email, PasswordHash, Parroquia, Pais, UrlAvatar, FechaRegistro, Activo)
        VALUES (@IdUsuarioNuevo, @RolBasicoId, @Nombre, @Apellidos, @Email, @PasswordHash, @Parroquia, @Pais, @UrlAvatar, GETUTCDATE(), 1);

        SET @Exito = 1;
    END

    -- =====================================================
    ELSE IF @Accion = 'Login'
    BEGIN
        SELECT IdUsuario, IdRol, Nombre, Apellidos, Email, PasswordHash, UrlAvatar, Activo
        FROM Usuarios
        WHERE Email = @Email AND Activo = 1;
        SET @Exito = 1;
    END

    -- =====================================================
    ELSE IF @Accion = 'ObtenerPerfil'
    BEGIN
        SELECT IdUsuario AS Id, IdRol AS RoleId, Nombre AS FirstName, Apellidos AS LastName, Email, Telefono AS Phone, UrlAvatar AS AvatarUrl, Biografia AS Bio, 
               FechaNacimiento AS BirthDate, Parroquia, Ciudad, Pais AS Country, FechaRegistro AS JoinedDate
        FROM Usuarios
        WHERE IdUsuario = @IdUsuario AND Activo = 1;
        SET @Exito = 1;
    END

    -- =====================================================
    ELSE IF @Accion = 'ActualizarPerfil'
    BEGIN
        UPDATE Usuarios
        SET Nombre = ISNULL(@Nombre, Nombre),
            Apellidos = ISNULL(@Apellidos, Apellidos),
            Telefono = ISNULL(@Telefono, Telefono),
            Biografia = ISNULL(@Biografia, Biografia),
            Parroquia = ISNULL(@Parroquia, Parroquia),
            Ciudad = ISNULL(@Ciudad, Ciudad),
            Pais = ISNULL(@Pais, Pais),
            UrlAvatar = ISNULL(@UrlAvatar, UrlAvatar),
            FechaNacimiento = ISNULL(@FechaNacimiento, FechaNacimiento)
        WHERE IdUsuario = @IdUsuario;

        SET @Exito = 1;
    END

    -- =====================================================
    ELSE IF @Accion = 'CambiarPassword'
    BEGIN
        -- Seguridad: el hash BCrypt debe venir desde la API.
        -- Evita guardar contraseñas en texto plano por error.
        IF @PasswordHash IS NULL
           OR LEN(@PasswordHash) <> 60
           OR LEFT(@PasswordHash, 4) NOT IN ('$2a$', '$2b$', '$2y$')
        BEGIN
            SET @MensajeError = 'PasswordHash inválido. Debe enviarse un hash BCrypt desde la API.';
            SET @Exito = 0;
            RETURN;
        END

        UPDATE Usuarios
        SET PasswordHash = @PasswordHash
        WHERE IdUsuario = @IdUsuario;

        IF @@ROWCOUNT = 0
        BEGIN
            SET @MensajeError = 'No se encontró el usuario para actualizar contraseña.';
            SET @Exito = 0;
            RETURN;
        END

        SET @Exito = 1;
    END
END
GO
