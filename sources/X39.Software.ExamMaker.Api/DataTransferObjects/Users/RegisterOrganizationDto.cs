using System.ComponentModel.DataAnnotations;

namespace X39.Software.ExamMaker.Api.DataTransferObjects.Users;

public record RegisterOrganizationDto(
    [Required] string TenantIdentifier,
    [Required] string TenantTitle,
    [Required] string AdminEmail,
    [Required] string AdminUsername,
    [Required] string AdminPassword);
