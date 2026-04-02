using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
    private string? _currentToken;

    public string? CurrentToken => _currentToken;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            _currentToken = token;

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonymous);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public void NotifyUserAuthentication(string token)
    {
        _currentToken = token;
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        _currentToken = null;
        var authState = Task.FromResult(new AuthenticationState(_anonymous));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        
        if (keyValuePairs == null) return Enumerable.Empty<Claim>();
        
        var claims = new List<Claim>();
        Console.WriteLine("[CustomAuthState] Parsing JWT claims:");
        foreach (var kvp in keyValuePairs)
        {
            Console.WriteLine($"[CustomAuthState] Raw Key: {kvp.Key}, Value: {kvp.Value}");
            var claimType = kvp.Key == "role" ? ClaimTypes.Role :
                            kvp.Key == "name" ? ClaimTypes.Name :
                            kvp.Key;

            if (kvp.Value is System.Text.Json.JsonElement element && element.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    claims.Add(new Claim(claimType, item.ToString()));
                }
            }
            else
            {
                claims.Add(new Claim(claimType, kvp.Value?.ToString() ?? string.Empty));
            }
        }
        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
