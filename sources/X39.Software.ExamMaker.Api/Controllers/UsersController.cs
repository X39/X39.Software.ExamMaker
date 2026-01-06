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

    /// <summary>Registers a new tenant organization with the provided details.</summary>
    /// <param name="payload">
    /// The payload containing the organization's identifier, title, and the administrator's details,
    /// including email, first name, last name, and password.
    /// </param>
    /// <returns>
    /// An <see cref="Microsoft.AspNetCore.Mvc.IActionResult"/> indicating the result of the operation.
    /// Returns <see cref="Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent"/> if successful,
    /// <see cref="Microsoft.AspNetCore.Http.StatusCodes.Status422UnprocessableEntity"/> for invalid input,
    /// or <see cref="Microsoft.AspNetCore.Http.StatusCodes.Status409Conflict"/> if a conflict is detected (e.g., duplicate email or identifier).
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="payload"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method validates the input, ensuring all required fields are populated and meet the required
    /// constraints (e.g., non-empty strings, proper formats). It also ensures that the admin email and organization identifier
    /// are unique across the system before proceeding with the registration process.
    /// </para>
    /// <para>
    /// If the provided input is invalid or conflicts exist, the appropriate HTTP status code is returned with a descriptive
    /// error message.
    /// </para>
    /// </remarks>
    /// <seealso cref="X39.Software.ExamMaker.Api.DataTransferObjects.Users.RegisterOrganizationDto"/>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase.UnprocessableEntity()"/>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase.Conflict(object)"/>
    [AllowAnonymous]
    [HttpPost("register/organization")]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterOrganizationAsync([FromBody] RegisterOrganizationDto payload)
    {
        if (!EmailRegex.IsMatch(payload.AdminEmail)
            || payload.AdminFirstName.IsNullOrWhiteSpace()
            || payload.AdminFirstName.Length < 3
            || payload.AdminLastName.IsNullOrWhiteSpace()
            || payload.AdminLastName.Length < 3
            || payload.AdminPassword.IsNullOrWhiteSpace()
            || payload.AdminPassword.Length < 6
            || payload.OrganizationIdentifier.IsNullOrWhiteSpace()
            || payload.OrganizationTitle.IsNullOrWhiteSpace())
            return UnprocessableEntity();

        if (await authorityDbContext.Users.AnyAsync(e => e.EMail == payload.AdminEmail))
            return Conflict(new { message = "Email already exists" });

        if (await examDbContext.Organizations.AnyAsync(t => t.Identifier == payload.OrganizationIdentifier))
            return Conflict(new { message = "Tenant identifier already exists" });

        var now = SystemClock.Instance.GetCurrentInstant();


        var authUser = new Storage.Authority.Entities.User
        {
            EMail     = payload.AdminEmail,
            FirstName = payload.AdminFirstName,
            LastName  = payload.AdminLastName,
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
                                    Title      = payload.OrganizationTitle,
                                    Identifier = payload.OrganizationIdentifier,
                                    CreatedAt  = now,
                                }
                            );
                            await examDbContext.SaveChangesAsync();
                            await examDbContext.Users.AddAsync(
                                new Storage.Exam.Entities.User
                                {
                                    Id             = authUserEntry.Entity.Id,
                                    OrganizationFk = organizationEntry.Entity.Id,
                                }
                            );
                            await examDbContext.SaveChangesAsync();
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

    /// <summary>Creates a registration link for an organization using the provided expiration details.</summary>
    /// <param name="payload">The payload containing the expiration duration for the registration link.</param>
    /// <returns>
    /// A <see cref="Microsoft.AspNetCore.Mvc.ActionResult{TValue}"/> containing the registration token as a <see cref="string"/> if successful,
    /// or an appropriate HTTP status code such as <see cref="Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized"/> if unauthorized.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="payload"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method generates a registration link for an organization. The link is valid until the expiration date
    /// specified in the <paramref name="payload"/>.
    /// </para>
    /// <para>
    /// The method ensures that the current user is authenticated and belongs to an organization. If the user is not
    /// authenticated or does not belong to an organization, an unauthorized response is returned.
    /// </para>
    /// </remarks>
    /// <seealso cref="X39.Software.ExamMaker.Api.Storage.Exam.Entities.OrganizationRegistrationToken" />
    /// <seealso cref="X39.Software.ExamMaker.Api.DataTransferObjects.Users.CreateRegistrationLinkDto" />
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
            OrganizationFk = user.OrganizationFk,
            CreatedBy      = user,
        };

        await examDbContext.OrganizationRegistrationTokens.AddAsync(registrationLink);
        await authorityDbContext.SaveChangesAsync();

        return Ok(registrationLink.Token);
    }

    /// <summary>Registers a new user using the provided registration token and user details.</summary>
    /// <param name="payload">The registration details containing the token, email, first name, last name, and password of the user.</param>
    /// <returns>
    /// An <see cref="Microsoft.AspNetCore.Mvc.IActionResult"/> indicating the result of the registration process.
    /// This can be a <see cref="Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent"/> if the registration is successful,
    /// <see cref="Microsoft.AspNetCore.Http.StatusCodes.Status409Conflict"/> if the user already exists,
    /// <see cref="Microsoft.AspNetCore.Http.StatusCodes.Status422UnprocessableEntity"/> if the registration details are invalid,
    /// <see cref="Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized"/> if the token is unauthorized, or
    /// <see cref="Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest"/> if the token is invalid or expired.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="payload"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method validates the provided registration details and the associated token. If valid, it creates a new user
    /// in the system and returns the appropriate status code.
    /// </para>
    /// <para>
    /// The token must be valid and not expired. The registration details, including email, names, and password,
    /// must meet validation criteria, such as length and format constraints.
    /// </para>
    /// </remarks>
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
            || payload.FirstName.IsNullOrWhiteSpace()
            || payload.FirstName.Length < 3
            || payload.LastName.IsNullOrWhiteSpace()
            || payload.LastName.Length < 3
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
            FirstName = payload.FirstName,
            LastName  = payload.LastName,
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

                            registrationLink.UsedByFk = authUser.Id;
                            registrationLink.UsedAt   = now;

                            var authUserEntry = await authorityDbContext.Users.AddAsync(authUser);
                            await authorityDbContext.SaveChangesAsync();

                            await examDbContext.Users.AddAsync(
                                new Storage.Exam.Entities.User
                                {
                                    Id             = authUserEntry.Entity.Id,
                                    OrganizationFk = registrationLink.OrganizationFk,
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

    /// <summary>Authenticates a user with the provided credentials and returns authentication tokens on success.</summary>
    /// <param name="payload">The login credentials containing the user's email and password.</param>
    /// <returns>
    /// An <see cref="Microsoft.AspNetCore.Mvc.ActionResult{T}" /> containing a <see cref="X39.Software.ExamMaker.Api.DataTransferObjects.Users.LoginResponseDto" />
    /// with authentication tokens and user details if the login is successful, or an appropriate error response.
    /// </returns>
    /// <exception cref="Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized">Thrown when the email or password is invalid or does not match any user.</exception>
    /// <exception cref="Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest">Thrown when the user is not associated with a valid organization.</exception>
    /// <remarks>
    /// <para>
    /// This method validates the user's credentials. If valid, it retrieves the user's details and their associated organization
    /// to generate access and refresh tokens. Invalid credentials or disconnected users result in appropriate error responses.
    /// </para>
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> LoginAsyncAsync([FromBody] LoginUserDto payload)
    {
        var authUser = await authorityDbContext.Users.FirstOrDefaultAsync(u => u.EMail == payload.EMail);

        if (authUser == null)
            return Unauthorized();

        if (!authUser.VerifyPassword(payload.Password, secretsConfig.CurrentValue.GetSalt()))
            return Unauthorized();

        var authUserId = authUser.Id;
        var organization =
            await examDbContext.Organizations.FirstOrDefaultAsync(e => e.Users!.Any(u => u.Id == authUserId));
        if (organization == null)
            return BadRequest();

        var (accessToken, refreshToken, expiresAt) = await jwtService.GenerateTokenAsync(authUser, organization);

        return Ok(
            new LoginResponseDto(
                accessToken,
                refreshToken,
                expiresAt,
                authUser.FirstName,
                authUser.LastName,
                authUser.EMail
            )
        );
    }

    /// <summary>Refreshes the user's access and refresh tokens using a valid refresh token.</summary>
    /// <param name="refreshTokenDto">An object containing the refresh token provided by the client.</param>
    /// <returns>
    /// An <see cref="Microsoft.AspNetCore.Mvc.ActionResult{T}" /> containing a <see cref="X39.Software.ExamMaker.Api.DataTransferObjects.Users.TokenDto" />
    /// with the newly generated tokens if successful, or an appropriate error response.
    /// </returns>
    /// <exception cref="Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized">
    /// Thrown when the provided refresh token cannot be associated with an existing user.
    /// </exception>
    /// <exception cref="Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest">
    /// Thrown when the user's authenticated account or associated organization cannot be resolved.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method verifies the provided refresh token, revokes the existing token, and generates a new pair of access
    /// and refresh tokens. It ensures the tokens are valid and linked to an authenticated user and organization.
    /// </para>
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType<TokenDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenDto>> RefreshTokenAsync([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var user = await examDbContext.Users
            .Where(e => e.UserTokens!.Any(t => t.RefreshToken == refreshTokenDto.RefreshToken))
            .SingleOrDefaultAsync();
        if (user == null)
            return Unauthorized();
        var userId = user.Id;
        await jwtService.RevokeTokenAsync(refreshTokenDto.RefreshToken);
        var authUser = await authorityDbContext.Users.FindAsync([userId]);
        if (authUser == null)
            return BadRequest();
        var organization =
            await examDbContext.Organizations.FirstOrDefaultAsync(e => e.Users!.Any(u => u.Id == userId));
        if (organization == null)
            return BadRequest();

        var (accessToken, refreshToken, expiresAt) = await jwtService.GenerateTokenAsync(authUser, organization);
        return Ok(new TokenDto(accessToken, refreshToken, expiresAt));
    }

    /// <summary>Logs out the current user and revokes all associated tokens.</summary>
    /// <returns>A <see cref="Microsoft.AspNetCore.Mvc.IActionResult"/> indicating the result of the operation.</returns>
    /// <exception cref="System.UnauthorizedAccessException">Thrown when the current user's ID cannot be resolved.</exception>
    /// <remarks>
    /// <para>The method revokes all active tokens associated with the user identified by the current <see cref="System.Security.Claims.ClaimsPrincipal"/>.</para>
    /// </remarks>
    /// <seealso cref="X39.Software.ExamMaker.Shared.ClaimsPrincipalExtensions.ResolveUserId" />
    /// <seealso cref="X39.Software.ExamMaker.Api.Services.JwtService.RevokeTokensOfUserAsync" />
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
