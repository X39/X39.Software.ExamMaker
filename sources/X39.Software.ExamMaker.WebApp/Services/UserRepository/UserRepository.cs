using X39.Software.ExamMaker.Models;

namespace X39.Software.ExamMaker.WebApp.Services.UserRepository;

internal sealed class UserRepository(
    IHttpClientFactory httpClientFactory,
    BaseUrl baseUrl,
    JwtAuthenticationStateProvider jwtAuthenticationStateProvider
) : RepositoryBase(httpClientFactory, baseUrl), IUserRepository
{
    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await Client.Users.Logout.PostAsync(cancellationToken: cancellationToken);
        await jwtAuthenticationStateProvider.SetTokenAsync(null, null);
    }

    public async Task<bool> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var response = await Client.Users.Login.PostAsync(
            new LoginUserDto
            {
                EMail    = email,
                Password = password,
            },
            cancellationToken: cancellationToken
        );
        if (response is null)
            return false;
        await jwtAuthenticationStateProvider.SetTokenAsync(response.Token, response.RefreshToken);
        return true;
    }

    public async Task RegisterOrganizationAsync(
        string organizationName,
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default
    )
    {
        await Client.Users.Register.Organization.PostAsync(
            new RegisterOrganizationDto
            {
                AdminEmail        = email,
                AdminPassword     = password,
                OrganizationTitle = organizationName,
                AdminFirstName    = firstName,
                AdminLastName     = lastName,
            },
            cancellationToken: cancellationToken
        );
    }

    public async Task RegisterUserAsync(
        string email,
        string password,
        string registrationToken,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default
    )
    {
        await Client.Users.Register.User.PostAsync(
            new RegisterWithLinkDto
            {
                EMail     = email,
                FirstName = firstName,
                LastName  = lastName,
                Password  = password,
                Token     = registrationToken,
            },
            cancellationToken: cancellationToken
        );
    }
}
