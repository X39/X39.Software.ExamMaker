using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using X39.Software.ExamMaker.WebApp.Services;

namespace X39.Software.ExamMaker.WebApp;

public static class KiotaServiceCollectionExtensions
{
    public static void UseKiotaRepositoryBase(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddScoped<JwtHeaderHttpRequestMessageHandler>();
        builder.Services
            .AddHttpClient(
                "API",
                (serviceProvider, conf) => conf.BaseAddress = serviceProvider.GetRequiredService<BaseUrl>()
                    .ApiUri
            )
            .AddHttpMessageHandler<JwtHeaderHttpRequestMessageHandler>();
    }
}
