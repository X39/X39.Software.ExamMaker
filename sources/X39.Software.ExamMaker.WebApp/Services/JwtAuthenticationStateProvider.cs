using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace X39.Software.ExamMaker.WebApp.Services;

public sealed class JwtAuthenticationStateProvider(LocalStorage localStorage) : AuthenticationStateProvider
{
    private const string JwtTokenKey        = nameof(JwtAuthenticationStateProvider) + "." + nameof(JwtTokenKey);
    private const string JwtRefreshTokenKey = nameof(JwtAuthenticationStateProvider) + "." + nameof(JwtRefreshTokenKey);

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // ToDo: Validate token is not revoked
        var token = await localStorage.GetAsync<string>(JwtTokenKey);
        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var principal = CreateClaimsPrincipalFromJwt(token, out var expired);
        if (expired)
        {
            await localStorage.RemoveAsync(JwtTokenKey);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        return new AuthenticationState(principal);
    }

    public async Task SetTokenAsync(string? token, string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            await localStorage.RemoveAsync(JwtTokenKey);
            await localStorage.RemoveAsync(JwtRefreshTokenKey);
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())))
            );
            return;
        }

        await localStorage.SetAsync(JwtTokenKey, token);
        if (refreshToken is not null)
            await localStorage.SetAsync(JwtRefreshTokenKey, refreshToken);
        var principal = CreateClaimsPrincipalFromJwt(token, out _);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task<string?> GetTokenAsync() => await localStorage.GetAsync<string>(JwtTokenKey);

    private static ClaimsPrincipal CreateClaimsPrincipalFromJwt(string jwt, out bool expired)
    {
        expired = false;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            // Determine expiration from "exp" claim if present
            var expClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp || c.Type == "exp");
            if (expClaim != null && long.TryParse(expClaim.Value, out var seconds))
            {
                var exp = DateTimeOffset.FromUnixTimeSeconds(seconds);
                if (exp < DateTimeOffset.UtcNow)
                {
                    expired = true;
                    return new ClaimsPrincipal(new ClaimsIdentity());
                }
            }

            var identity = new ClaimsIdentity(token.Claims, authenticationType: "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            // If token is malformed, treat as not authenticated
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
