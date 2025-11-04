using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using X39.Software.ExamMaker.Api.Configuration;
using X39.Software.ExamMaker.Api.Storage.Exam;
using X39.Software.ExamMaker.Api.Storage.Exam.Entities;
using X39.Software.ExamMaker.Shared;

namespace X39.Software.ExamMaker.Api.Services;

public class JwtService(JwtConfig jwtConfig, ExamDbContext examDbContext)
{
    public async Task<(string accessToken, string refreshToken, DateTimeOffset expiresAt)> GenerateTokenAsync(
        Storage.Authority.Entities.User user,
        Organization organization
    )
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(jwtConfig.AccessTokenExpirationInMinutes);
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.EMail),
            new Claim(ClaimTypes.Name, string.Concat(user.FirstName, " ", user.LastName)),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(CustomClaimTypes.OrganizationId, organization.Id.ToString()),
            new Claim(CustomClaimTypes.OrganizationName, organization.Title),
            new Claim(ClaimTypes.Expiration, expiresAt.ToString("O")),
        };

        var token = new JwtSecurityToken(
            issuer: jwtConfig.Issuer,
            audience: jwtConfig.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        var now = SystemClock.Instance.GetCurrentInstant();
        var expiresAtUtc = now.Plus(Duration.FromMinutes(jwtConfig.RefreshTokenExpirationInDays));

        var userToken = new UserToken
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt    = expiresAtUtc,
            CreatedAt    = now,
            IsRevoked    = false,
            UserId       = user.Id,
        };

        await examDbContext.UserTokens.AddAsync(userToken);
        await examDbContext.SaveChangesAsync();

        return (accessToken, refreshToken, expiresAt);
    }

    public async Task RevokeTokenAsync(string accessToken)
    {
        await examDbContext.UserTokens
            .Where(ut => ut.AccessToken == accessToken)
            .ExecuteUpdateAsync(calls => calls.SetProperty(e => e.IsRevoked, true));
    }

    public async Task RevokeTokensOfUserAsync(long userId)
    {
        await examDbContext.UserTokens
            .Where(ut => ut.UserId == userId)
            .ExecuteUpdateAsync(calls => calls.SetProperty(e => e.IsRevoked, true));
    }

    public async Task<bool> IsTokenRevokedAsync(string accessToken)
    {
        var userToken = await examDbContext.UserTokens.FirstOrDefaultAsync(ut => ut.AccessToken == accessToken);
        return userToken?.IsRevoked ?? false;
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
