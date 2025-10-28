namespace X39.Software.ExamMaker.Api.DataTransferObjects.Users;

public record TokenDto(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
