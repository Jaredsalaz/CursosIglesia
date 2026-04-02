using System.Net.Http.Headers;

namespace CursosIglesia.Services.Implementations.ApiClients;

public class JwtAuthorizationHandler : DelegatingHandler
{
    private readonly InMemoryTokenStore _tokenStore;

    public JwtAuthorizationHandler(InMemoryTokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _tokenStore.Token;

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
