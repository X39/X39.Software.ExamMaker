using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using JetBrains.Annotations;

namespace X39.Software.ExamMaker.Shared;

[PublicAPI]
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Resolves the organization ID from the specified <see cref="System.Security.Claims.ClaimsPrincipal"/>.
    /// Extracts the claim value for "OrganizationId" from the <see cref="System.Security.Claims.ClaimsPrincipal"/>
    /// and attempts to parse it into a <see langword="long"/> value. Indicates success or failure.
    /// </summary>
    /// <param name="principal">The <see cref="System.Security.Claims.ClaimsPrincipal"/> object containing the claims to evaluate.</param>
    /// <param name="organizationId">
    /// Outputs the resolved organization ID as a <see langword="long"/> value if successful. Defaults to <see langword="0"/> if resolution fails.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the "OrganizationId" claim was found and successfully parsed into a <see langword="long"/> value; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ResolveOrganizationId(this ClaimsPrincipal principal, out long organizationId)
    {
        var organizationIdString = principal.FindFirst(e => e.Type == CustomClaimTypes.OrganizationId)
            ?.Value;
        if (organizationIdString is null)
        {
            organizationId = 0;
            return false;
        }

        if (long.TryParse(organizationIdString, out organizationId) && organizationId > 0)
            return true;
        organizationId = 0;
        return false;
    }

    /// <summary>
    /// Resolves the user ID from the specified <see cref="System.Security.Claims.ClaimsPrincipal"/> object.
    /// </summary>
    /// <param name="principal">
    /// The <see cref="System.Security.Claims.ClaimsPrincipal"/> containing the claims to resolve.
    /// </param>
    /// <param name="userId">
    /// The resolved user ID as a long. Set to 0 if the operation fails.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the user ID was resolved and parsed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ResolveUserId(this ClaimsPrincipal principal, out long userId)
    {
        var userIdString = principal.FindFirst(e => e.Type == ClaimTypes.NameIdentifier)
            ?.Value;
        if (userIdString is null)
        {
            userId = 0;
            return false;
        }

        if (long.TryParse(userIdString, out userId) && userId > 0)
            return true;
        userId = 0;
        return false;
    }

    /// <summary>Resolves the email address from the specified <see cref="System.Security.Claims.ClaimsPrincipal"/> object.</summary>
    /// <param name="principal">The <see cref="System.Security.Claims.ClaimsPrincipal"/> object containing the claim to resolve.</param>
    /// <param name="email">The resolved email address as a string. Set to <see langword="null"/> if the operation fails.</param>
    /// <returns><see langword="true"/> if the email address was resolved successfully; otherwise, <see langword="false"/>.</returns>
    public static bool ResolveEMail(this ClaimsPrincipal principal, [NotNullWhen(true)] out string? email)
    {
        var valueString = principal.FindFirst(e => e.Type == ClaimTypes.Email)
            ?.Value;
        if (valueString is null)
        {
            email = null;
            return false;
        }

        email = valueString;
        return true;
    }

    /// <summary>Resolves the organization name from the specified <see cref="System.Security.Claims.ClaimsPrincipal"/> object.</summary>
    /// <param name="principal">The <see cref="System.Security.Claims.ClaimsPrincipal"/> object containing the claims to resolve.</param>
    /// <param name="organizationName">The resolved organization name as a string. Set to <see langword="null"/> if no organization name is found.</param>
    /// <returns><see langword="true"/> if the organization name was successfully found and resolved; otherwise, <see langword="false"/>.</returns>
    public static bool ResolveOrganizationName(
        this ClaimsPrincipal principal,
        [NotNullWhen(true)] out string? organizationName
    )
    {
        organizationName = principal.FindFirst(e => e.Type == CustomClaimTypes.OrganizationName)
            ?.Value;
        return organizationName is not null;
    }

    /// <summary>Resolves the organization name from the specified <see cref="System.Security.Claims.ClaimsPrincipal"/>.</summary>
    /// <param name="principal">The <see cref="System.Security.Claims.ClaimsPrincipal"/> object containing the claims to evaluate.</param>
    /// <returns>
    /// The resolved organization name as a string if the "OrganizationName" claim is found; otherwise, <see langword="null"/>.
    /// </returns>
    public static string? ResolveOrganizationName(this ClaimsPrincipal principal)
        => ResolveOrganizationName(principal, out var organizationName) ? organizationName : null;
}
