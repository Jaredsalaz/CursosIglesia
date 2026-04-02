namespace CursosIglesia.Services;

/// <summary>
/// Singleton in-memory token store that bridges the gap between
/// ApiAuthService (which stores the token after login) and
/// JwtAuthorizationHandler (which needs it for HTTP requests).
/// 
/// Blazored.LocalStorage uses JS interop which is NOT available
/// inside HttpClient DelegatingHandler pipelines in Blazor Server.
/// This store avoids that limitation entirely.
/// </summary>
public class InMemoryTokenStore
{
    private volatile string? _token;

    public string? Token
    {
        get => _token;
        set => _token = value;
    }

    public void Clear() => _token = null;
}
