namespace X39.Software.ExamMaker.Api.DataTransferObjects.Users;

public record RegisterWithLinkDto(
    string Token,
    string EMail,
    string FirstName,
    string LastName,
    string Password);
