using Microsoft.Extensions.Caching.Memory;
using CursosIglesia.Services.Interfaces;

namespace CursosIglesia.Services.Implementations;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<OtpService> _logger;

    public OtpService(IMemoryCache cache, ILogger<OtpService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public string GenerateOtp(string email)
    {
        var random = new Random();
        var otp = random.Next(100000, 999999).ToString();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        
        _cache.Set($"OTP_{email.ToLower()}", otp, cacheOptions);

        // AQUÍ LOGRAMOS que tú puedas ver el OTP en la terminal del servidor si el cliente no lo recibe
        _logger.LogWarning($"[DEBUG - SOLO DESARROLLO] El OTP generado para {email} es: {otp}");

        return otp;
    }

    public bool VerifyOtp(string email, string inputOtp)
    {
        var cacheKey = $"OTP_{email.ToLower()}";
        if (_cache.TryGetValue(cacheKey, out string? cachedOtp))
        {
            if (cachedOtp == inputOtp)
            {
                _cache.Remove(cacheKey);
                return true;
            }
        }
        return false;
    }
}
