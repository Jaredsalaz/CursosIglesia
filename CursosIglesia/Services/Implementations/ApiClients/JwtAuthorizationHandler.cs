using System.Net.Http.Headers;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class JwtAuthorizationHandler : DelegatingHandler
{
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public JwtAuthorizationHandler(CustomAuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[JwtAuthorizationHandler] Attempting to retrieve token for: {request.RequestUri}");
            var token = _authStateProvider.CurrentToken;
            Console.WriteLine($"[JwtAuthorizationHandler] Token retrieval completed. Token exists: {!string.IsNullOrWhiteSpace(token)}");
            
            if (!string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine($"[JwtAuthorizationHandler] ✅ Adding Bearer token to request");
                Console.WriteLine($"[JwtAuthorizationHandler]    Preview: {token.Substring(0, Math.Min(30, token.Length))}...");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine($"[JwtAuthorizationHandler]    Authorization header set");
            }
            else
            {
                Console.WriteLine($"[JwtAuthorizationHandler] ❌ No token found in localStorage");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JwtAuthorizationHandler] ❌ Error: {ex.GetType().Name} - {ex.Message}");
        }

        var response = await base.SendAsync(request, cancellationToken);
        Console.WriteLine($"[JwtAuthorizationHandler] Response: {response.StatusCode} for {request.RequestUri}");
        return response;
    }
}
