namespace X39.Software.ExamMaker.Api.DataTransferObjects.Users;

public record LoginResponseDto(string Token, string RefreshToken, DateTimeOffset ExpiresAt, string FirstName, string LastName, string EMail);
