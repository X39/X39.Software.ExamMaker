namespace X39.Software.ExamMaker.WebApp.Services.UserRepository;

public interface IUserRepository
{
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<bool> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task RegisterOrganizationAsync(
        string organizationName,
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default
    );

    Task RegisterUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string registrationToken,
        CancellationToken cancellationToken = default
    );
}
