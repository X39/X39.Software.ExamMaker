using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using X39.Software.ExamMaker.Api.Configuration;
using X39.Software.ExamMaker.Api.DataTransferObjects.Users;
using X39.Software.ExamMaker.Api.Services;
using X39.Software.ExamMaker.Api.Storage.Authority;
using X39.Software.ExamMaker.Api.Storage.Authority.Extensions;
using X39.Software.ExamMaker.Api.Storage.Exam;
using X39.Software.ExamMaker.Shared;
using X39.Util;

namespace X39.Software.ExamMaker.Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed partial class UsersController(
    ExamDbContext examDbContext,
    AuthorityDbContext authorityDbContext,
    IOptionsMonitor<Secrets> secretsConfig,
    JwtService jwtService
) : ControllerBase
{
    [GeneratedRegex(
        """
        \A(?=[a-z0-9@.!#$%&'*+/=?^_`{|}~-]{6,254}\z)
          (?=[a-z0-9.!#$%&'*+/=?^_`{|}~-]{1,64}@)[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*
        @ (?:(?=[a-z0-9-]{1,63}\.)[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+
          (?=[a-z0-9-]{1,63}\z)[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\z
        """,
        RegexOptions.Compiled
        | RegexOptions.IgnoreCase
        | RegexOptions.CultureInvariant
        | RegexOptions.ExplicitCapture
        | RegexOptions.Singleline
        | RegexOptions.IgnorePatternWhitespace
    )]
    private partial Regex EmailRegex { get; }

    [AllowAnonymous]
    [HttpPost("register/organization")]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterTenantAsync([FromBody] RegisterOrganizationDto payload)
    {
        if (!EmailRegex.IsMatch(payload.AdminEmail)
            || payload.AdminUsername.IsNullOrWhiteSpace()
            || payload.AdminUsername.Length < 3
            || payload.AdminPassword.IsNullOrWhiteSpace()
            || payload.AdminPassword.Length < 6
            || payload.TenantIdentifier.IsNullOrWhiteSpace()
            || payload.TenantTitle.IsNullOrWhiteSpace())
            return UnprocessableEntity();

        if (await authorityDbContext.Users.AnyAsync(e => e.EMail == payload.AdminEmail))
            return Conflict(new { message = "Email already exists" });

        if (await examDbContext.Organizations.AnyAsync(t => t.Identifier == payload.TenantIdentifier))
            return Conflict(new { message = "Tenant identifier already exists" });

        var now = SystemClock.Instance.GetCurrentInstant();


        var authUser = new Storage.Authority.Entities.User
        {
            EMail     = payload.AdminEmail,
            Title     = payload.AdminUsername,
            CreatedAt = now,
        };
        authUser.SetPassword(payload.AdminPassword, secretsConfig.CurrentValue.GetSalt());

        var authorityExecutionStrategy = authorityDbContext.Database.CreateExecutionStrategy();
        await authorityExecutionStrategy.ExecuteAsync(async () =>
            {
                var examExecutionStrategy = examDbContext.Database.CreateExecutionStrategy();
                await examExecutionStrategy.ExecuteAsync(async () =>
                    {
                        await using var authorityDbTransaction =
                            await authorityDbContext.Database.BeginTransactionAsync();
                        await using var examDbTransaction = await examDbContext.Database.BeginTransactionAsync();
                        try
                        {
                            var authUserEntry = await authorityDbContext.Users.AddAsync(authUser);
                            await authorityDbContext.SaveChangesAsync();

                            var organizationEntry = await examDbContext.Organizations.AddAsync(
                                new Storage.Exam.Entities.Organization
                                {
                                    Title      = payload.TenantTitle,
                                    Identifier = payload.TenantIdentifier,
                                    CreatedAt  = now,
                                }
                            );
                            await examDbContext.Users.AddAsync(
                                new Storage.Exam.Entities.User
                                {
                                    Id             = authUserEntry.Entity.Id,
                                    OrganizationId = organizationEntry.Entity.Id,
                                }
                            );
                            await authorityDbTransaction.CommitAsync();
                            await examDbTransaction.CommitAsync();
                        }
                        catch
                        {
                            await authorityDbTransaction.RollbackAsync();
                            await examDbTransaction.RollbackAsync();
                            throw;
                        }
                    }
                );
            }
        );

        return NoContent();
    }

    [Authorize]
    [HttpPost("create-registration-link")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<string>> CreateRegistrationLinkAsync([FromBody] CreateRegistrationLinkDto payload)
    {
        var userEmail = User.Identity?.Name;
        if (userEmail == null || !User.ResolveUserId(out var userId))
            return Unauthorized();

        var user = await examDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return Unauthorized();

        var now = SystemClock.Instance.GetCurrentInstant();
        var expiresAt = now.Plus(Duration.FromTimeSpan(payload.ExpiresIn));
        using var crypto = System.Security.Cryptography.RandomNumberGenerator.Create();
        var bytes = new byte[255];
        var registrationLink = new Storage.Exam.Entities.OrganizationRegistrationToken
        {
            Token          = Convert.ToBase64String(bytes),
            ExpiresAt      = expiresAt,
            CreatedAt      = now,
            OrganizationId = user.OrganizationId,
            CreatedBy      = user,
        };

        await examDbContext.OrganizationRegistrationTokens.AddAsync(registrationLink);
        await authorityDbContext.SaveChangesAsync();

        return Ok(registrationLink.Token);
    }

    [AllowAnonymous]
    [HttpPost("register/user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterWithLinkAsync([FromBody] RegisterWithLinkDto payload)
    {
        if (!EmailRegex.IsMatch(payload.EMail)
            || payload.Username.IsNullOrWhiteSpace()
            || payload.Username.Length < 3
            || payload.Password.IsNullOrWhiteSpace()
            || payload.Password.Length < 8)
            return UnprocessableEntity();
        var now = SystemClock.Instance.GetCurrentInstant();

        var registrationLink =
            await examDbContext.OrganizationRegistrationTokens.FirstOrDefaultAsync(r => r.Token == payload.Token);

        #if DEBUG
        if (registrationLink == null)
            return BadRequest(new { message = "Invalid registration token" });
        if (registrationLink.ExpiresAt < now)
            return BadRequest(new { message = "Registration token has expired" });
        #else
        if (registrationLink == null || registrationLink.ExpiresAt < now)
            return BadRequest(new { message = "Registration token has expired or is invalid" });
        #endif


        var authUser = new Storage.Authority.Entities.User
        {
            EMail     = payload.EMail,
            Title     = payload.Username,
            CreatedAt = now,
        };
        authUser.SetPassword(payload.Password, secretsConfig.CurrentValue.GetSalt());
        var authorityExecutionStrategy = authorityDbContext.Database.CreateExecutionStrategy();
        return await authorityExecutionStrategy.ExecuteAsync(async () =>
            {
                var examExecutionStrategy = examDbContext.Database.CreateExecutionStrategy();
                return await examExecutionStrategy.ExecuteAsync<IActionResult>(async () =>
                    {
                        await using var authorityDbTransaction =
                            await authorityDbContext.Database.BeginTransactionAsync();
                        await using var examDbTransaction = await examDbContext.Database.BeginTransactionAsync();
                        try
                        {
                            if (await authorityDbContext.Users.AnyAsync(e => e.EMail == payload.EMail))
                            {
                                return Conflict(new { message = "Email already exists" });
                            }

                            registrationLink.UsedById = authUser.Id;
                            registrationLink.UsedAt   = now;

                            var authUserEntry = await authorityDbContext.Users.AddAsync(authUser);
                            await authorityDbContext.SaveChangesAsync();

                            await examDbContext.Users.AddAsync(
                                new Storage.Exam.Entities.User
                                {
                                    Id             = authUserEntry.Entity.Id,
                                    OrganizationId = registrationLink.OrganizationId,
                                }
                            );
                            await authorityDbTransaction.CommitAsync();
                            await examDbTransaction.CommitAsync();
                            return NoContent();
                        }
                        catch
                        {
                            await authorityDbTransaction.RollbackAsync();
                            await examDbTransaction.RollbackAsync();
                            throw;
                        }
                    }
                );
            }
        );
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> LoginAsyncAsync([FromBody] LoginUserDto payload)
    {
        var authUser = await authorityDbContext.Users
            .FirstOrDefaultAsync(u => u.EMail == payload.EMail);

        if (authUser == null)
            return Unauthorized();

        if (!authUser.VerifyPassword(payload.Password, secretsConfig.CurrentValue.GetSalt()))
            return Unauthorized();

        var authUserId = authUser.Id;
        var organization = await examDbContext.Organizations
            .FirstOrDefaultAsync(e => e.Users!.Any(u => u.Id == authUserId));
        if (organization == null)
            return BadRequest();

        var (accessToken, refreshToken, expiresAt) = await jwtService.GenerateTokenAsync(authUser, organization);

        return Ok(new LoginResponseDto(accessToken, refreshToken, expiresAt, authUser.Title, authUser.EMail));
    }

    [Authorize]
    [HttpPost("refresh")]
    [ProducesResponseType<TokenDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenDto>> RefreshTokenAsync([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (!User.ResolveUserId(out var userId) || await jwtService.IsTokenRevokedAsync(refreshTokenDto.RefreshToken))
            return Unauthorized();
        await jwtService.RevokeTokenAsync(refreshTokenDto.RefreshToken);
        var authUser = await authorityDbContext.Users.FindAsync([userId]);
        if (authUser == null)
            return BadRequest();
        var organization = await examDbContext.Organizations
            .FirstOrDefaultAsync(e => e.Users!.Any(u => u.Id == userId));
        if (organization == null)
            return BadRequest();

        var (accessToken, refreshToken, expiresAt) = await jwtService.GenerateTokenAsync(authUser, organization);
        return Ok(new TokenDto(accessToken, refreshToken, expiresAt));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync()
    {
        if (!User.ResolveUserId(out var userId))
            return Unauthorized();
        await jwtService.RevokeTokensOfUserAsync(userId);
        return NoContent();
    }
}
