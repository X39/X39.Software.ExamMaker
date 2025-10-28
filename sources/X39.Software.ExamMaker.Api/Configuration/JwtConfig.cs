namespace X39.Software.ExamMaker.Api.Configuration;

public sealed class JwtConfig
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationInMinutes { get; set; } = 15;
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}
