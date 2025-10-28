namespace X39.Software.ExamMaker.Shared;

/// <summary>
/// Provides a static collection of custom claim types used throughout the application.
/// </summary>
/// <remarks>
/// This class defines constants representing specific claims utilized within the system.
/// These claims are typically employed for identity management and authorization scenarios.
/// </remarks>
public static class CustomClaimTypes
{
    /// <summary>
    /// Represents the unique identifier for a organization in the application.
    /// This claim is used to associate requests or actions with a specific organization.
    /// </summary>
    /// <remarks>
    /// The value of this constant is included in JWT tokens and can be used within claims to resolve organization-specific context.
    /// Commonly utilized in user authentication and authorization workflows.
    /// </remarks>
    public const string OrganizationId       = "org-id";

    /// <summary>
    /// Represents the name or descriptive label for a organization in the application.
    /// This claim is used to provide a human-readable identifier associated with a specific organization.
    /// </summary>
    /// <remarks>
    /// The value of this constant is typically included in JWT tokens and can be utilized within claims
    /// to resolve organization-specific context. Frequently employed in user authentication workflows
    /// and for displaying organization-related information in the UI or logs.
    /// </remarks>
    public const string OrganizationName       = "org-name";
}
