using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace X39.Software.ExamMaker.WebApp.Services;

public abstract class RepositoryBase
{
    protected BaseUrl BaseUrl { get; }
    protected ApiClient Client { get; }

    public RepositoryBase(IHttpClientFactory httpClientFactory, BaseUrl baseUrl)
    {
        BaseUrl = baseUrl;
        var authenticationProvider = new AnonymousAuthenticationProvider();
        var requestAdapter = new HttpClientRequestAdapter(authenticationProvider, httpClient: httpClientFactory.CreateClient("API"));
        requestAdapter.BaseUrl = baseUrl.ApiUrl;
        Client                 = new ApiClient(requestAdapter);
    }
}
