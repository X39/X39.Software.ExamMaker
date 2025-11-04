using System.ComponentModel.DataAnnotations;

namespace X39.Software.ExamMaker.Api.DataTransferObjects.Users;

public record RegisterOrganizationDto(
    [Required] string OrganizationIdentifier,
    [Required] string OrganizationTitle,
    [Required] string AdminEmail,
    [Required] string AdminFirstName,
    [Required] string AdminLastName,
    [Required] string AdminPassword);
