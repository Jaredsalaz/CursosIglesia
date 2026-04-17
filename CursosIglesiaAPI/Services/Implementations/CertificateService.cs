using CursosIglesiaAPI.Models;
using CursosIglesiaAPI.Models.DTOs;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CursosIglesiaAPI.Services.Implementations;

public class CertificateService : ICertificateService
{
    private readonly string _connectionString;

    public CertificateService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    /// <summary>
    /// Genera un certificado para el usuario si ha completado el curso
    /// </summary>
    public async Task<CertificateResponse> GenerateCertificateAsync(Guid userId, Guid courseId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "GenerarCertificado");
        parameters.Add("@IdUsuario", userId);
        parameters.Add("@IdCurso", courseId);
        parameters.Add("@CodigoQR", (string?)null); // Se genera en el controlador
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parameters.Add("@MensajeError", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);

        await db.ExecuteAsync("usp_GestionCertificados", parameters, commandType: CommandType.StoredProcedure);

        bool exito = parameters.Get<bool>("@Exito");
        string? mensajeError = parameters.Get<string>("@MensajeError");

        if (!exito)
        {
            throw new InvalidOperationException(mensajeError ?? "Error al generar certificado");
        }

        // Obtener detalles del certificado recién generado
        var certificateDetails = await GetCertificateDetailsAsync(userId, courseId);

        return new CertificateResponse
        {
            IdCertificado = certificateDetails?.IdCertificado ?? Guid.Empty,
            NumeroCertificado = certificateDetails?.NumeroCertificado ?? string.Empty,
            NombreCurso = certificateDetails?.NombreCurso ?? string.Empty,
            NombreEstudiante = certificateDetails?.NombreEstudiante ?? string.Empty,
            NombreInstructor = certificateDetails?.NombreInstructor,
            FechaOtorgamiento = certificateDetails?.FechaOtorgamiento ?? DateTime.UtcNow,
            YaGenerado = true
        };
    }

    /// <summary>
    /// Obtiene todos los certificados del usuario
    /// </summary>
    public async Task<List<CertificateResponse>> GetUserCertificatesAsync(Guid userId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "ObtenerCertificados");
        parameters.Add("@IdUsuario", userId);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);

        var certificates = await db.QueryAsync<CertificateResponse>(
            "usp_GestionCertificados",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        return certificates.ToList();
    }

    /// <summary>
    /// Obtiene un certificado específico (para descarga de PDF)
    /// </summary>
    private async Task<CertificateResponse?> GetCertificateDetailsAsync(Guid userId, Guid courseId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        // Query para obtener los detalles del certificado más reciente del usuario para este curso
        var certificate = await db.QueryFirstOrDefaultAsync<CertificateResponse>(
            @"SELECT TOP 1
                c.IdCertificado,
                c.NumeroCertificado,
                cur.Titulo AS NombreCurso,
                u.Nombre + ' ' + u.Apellidos AS NombreEstudiante,
                CONCAT(m.Nombre, ' ', m.Apellidos) AS NombreInstructor,
                c.FechaOtorgamiento,
                c.CodigoQR
            FROM Certificados c
            INNER JOIN Usuarios u ON c.IdUsuario = u.IdUsuario
            INNER JOIN Cursos cur ON c.IdCurso = cur.IdCurso
            INNER JOIN Usuarios m ON cur.IdMaestro = m.IdUsuario
            WHERE c.IdUsuario = @IdUsuario AND c.IdCurso = @IdCurso
            ORDER BY c.FechaOtorgamiento DESC",
            new { IdUsuario = userId, IdCurso = courseId }
        );

        return certificate;
    }

    /// <summary>
    /// Obtiene detalles de un certificado específico por ID
    /// </summary>
    public async Task<CertificateResponse?> GetCertificateDetailsAsync(Guid certificateId, Guid userId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "ObtenerPorId");
        parameters.Add("@IdCertificado", certificateId);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);

        var certificate = await db.QueryFirstOrDefaultAsync<dynamic>(
            "usp_GestionCertificados",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        if (certificate == null)
            return null;

        // Validar que el certificado pertenece al usuario
        if ((Guid)certificate.IdUsuario != userId)
            throw new UnauthorizedAccessException("No tienes permiso para acceder a este certificado");

        return new CertificateResponse
        {
            IdCertificado = certificate.IdCertificado,
            NumeroCertificado = certificate.NumeroCertificado,
            NombreCurso = certificate.NombreCurso,
            NombreEstudiante = certificate.NombreEstudiante,
            NombreInstructor = certificate.NombreInstructor,
            FechaOtorgamiento = certificate.FechaOtorgamiento,
            CodigoQR = certificate.CodigoQR,
            YaGenerado = true
        };
    }

    /// <summary>
    /// Verifica un certificado (endpoint público)
    /// </summary>
    public async Task<VerifyCertificateResponse?> VerifyCertificateAsync(string certificateNumber)
    {
        using IDbConnection db = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@Accion", "Verificar");
        parameters.Add("@NumeroCertificado", certificateNumber);
        parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);

        var result = await db.QueryFirstOrDefaultAsync<dynamic>(
            "usp_GestionCertificados",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        if (result == null)
            return null;

        return new VerifyCertificateResponse
        {
            Valido = result.Valido == 1,
            NombreEstudiante = result.NombreEstudiante,
            NombreCurso = result.NombreCurso,
            FechaOtorgamiento = result.FechaOtorgamiento,
            Estado = result.Valido == 1 ? "Válido" : "Inválido"
        };
    }

    /// <summary>
    /// Genera PDF del certificado
    /// </summary>
    public async Task<byte[]> GetCertificatePdfAsync(Guid certificateId, Guid userId)
    {
        // Obtener detalles del certificado
        var certificateDetails = await GetCertificateDetailsAsync(certificateId, userId);

        if (certificateDetails == null)
            throw new InvalidOperationException("Certificado no encontrado");

        // Generar PDF con CertificatePdfGenerator
        byte[] pdfBytes = CertificatePdfGenerator.GenerateCertificatePdf(certificateDetails);

        return pdfBytes;
    }
}
