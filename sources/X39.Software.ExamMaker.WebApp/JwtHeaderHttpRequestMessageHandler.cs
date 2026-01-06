using System.Net.Http.Headers;
using JetBrains.Annotations;
using X39.Software.ExamMaker.WebApp.Services;

namespace X39.Software.ExamMaker.WebApp;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class JwtHeaderHttpRequestMessageHandler(JwtAuthenticationStateProvider authProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var path = request.RequestUri?.LocalPath ?? string.Empty;
        if (path is "/Users/refresh" or "/Users/login") // Short circuit for refresh and login
            return await base.SendAsync(request, cancellationToken);
        
        _ = await authProvider.GetAuthenticationStateAsync(); // Refresh State if needed
        var token = await authProvider.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token) && request.Headers.Authorization is null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
