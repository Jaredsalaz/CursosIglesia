using System.Data;
using CursosIglesiaAPI.Models.DTOs;
using CursosIglesiaAPI.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using QRCoder;

namespace CursosIglesiaAPI.Services.Implementations
{
    public class CertificateService : ICertificateService
    {
        private readonly string _connectionString;

        public CertificateService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        public async Task<CertificateResponse> GenerateCertificateAsync(Guid userId, Guid courseId, string verificationUrlPattern)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Accion", "GenerarCertificado");
            parameters.Add("@IdUsuario", userId);
            parameters.Add("@IdCurso", courseId);
            parameters.Add("@Exito", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@MensajeError", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);

            // Generamos preliminarmente el certificado sin QR, pero necesitamos el número único.
            // Para poder insertar el QR generado con el número único dentro del QR.
            // Daremos el código QR base64 en la base de datos si queremos guardarlo publicamente.

            // Ejecutamos el SP y capturamos el resultado del SELECT interno
            var result = await db.QueryFirstOrDefaultAsync<dynamic>("usp_GestionCertificados", parameters, commandType: CommandType.StoredProcedure);

            bool success = parameters.Get<bool>("@Exito");
            string error = parameters.Get<string>("@MensajeError");

            if (!success)
            {
                return new CertificateResponse { Success = false, Message = error ?? "Error desconocido generando certificado." };
            }

            if (result != null && string.IsNullOrEmpty(result.CodigoQR))
            {
                // Generar QR en base64 con la URL de validación pública
                string validationUrl = verificationUrlPattern.Replace("{number}", result.NumeroCertificado);
                string qrBase64 = GenerateQRCodeBase64(validationUrl);

                // Actualizarlo en DB directamente usando el IdCertificado devuelto
                await db.ExecuteAsync("UPDATE Certificados SET CodigoQR = @CodigoQR WHERE IdCertificado = @IdCertificado",
                    new { CodigoQR = qrBase64, IdCertificado = result.IdCertificado });
            }

            return new CertificateResponse { Success = true, Message = "Certificado generado exitosamente." };
        }

        public async Task<IEnumerable<CertificateListDto>> GetMyCertificatesAsync(Guid userId)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            return await db.QueryAsync<CertificateListDto>("usp_GestionCertificados", 
                new { Accion = "ObtenerCertificados", IdUsuario = userId }, 
                commandType: CommandType.StoredProcedure);
        }

        public async Task<CertificateListDto?> GetCertificateByIdAsync(Guid certificateId)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            return await db.QueryFirstOrDefaultAsync<CertificateListDto>("usp_GestionCertificados", 
                new { Accion = "ObtenerPorId", IdCertificado = certificateId }, 
                commandType: CommandType.StoredProcedure);
        }

        public async Task<VerifyCertificateResponse> VerifyCertificateAsync(string numeroCertificado)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            var result = await db.QueryFirstOrDefaultAsync<VerifyCertificateResponse>("usp_GestionCertificados", 
                new { Accion = "Verificar", NumeroCertificado = numeroCertificado }, 
                commandType: CommandType.StoredProcedure);

            if (result == null)
            {
                return new VerifyCertificateResponse { Valido = false, Message = "El certificado no existe o el código es inválido." };
            }

            if (!result.Valido)
            {
                result.Message = "El certificado no es válido.";
            }

            return result;
        }

        private string GenerateQRCodeBase64(string text)
        {
            using QRCodeGenerator qrGenerator = new QRCodeGenerator();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeImage);
        }
    }
}
