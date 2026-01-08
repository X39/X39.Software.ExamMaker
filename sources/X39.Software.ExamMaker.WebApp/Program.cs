using ApexCharts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using X39.Software.ExamMaker.WebApp.Services;
using X39.Software.ExamMaker.WebApp.Services.ExamAnswerRepository;
using X39.Software.ExamMaker.WebApp.Services.ExamQuestionRepository;
using X39.Software.ExamMaker.WebApp.Services.ExamRepository;
using X39.Software.ExamMaker.WebApp.Services.ExamTopicRepository;
using X39.Software.ExamMaker.WebApp.Services.UserRepository;

namespace X39.Software.ExamMaker.WebApp;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Auth + API client services
        builder.UseKiotaRepositoryBase();
        
        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddApexCharts();
        builder.Services.AddLocalization();
        builder.Services.AddAuthorizationCore();
        builder.Services.AddFluentUIComponents(options => options.ValidateClassNames = false);
        builder.Services.AddFluentUIComponents();
        builder.Services.AddScoped<LocalStorage>();
        builder.Services.AddScoped<JsUtil>();
        builder.Services.AddScoped<JwtAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp
            => sp.GetRequiredService<JwtAuthenticationStateProvider>()
        );
        builder.Services.AddSingleton<BaseUrl>(_ =>
            {
                var selfUrl = builder.HostEnvironment.BaseAddress;
                var apiUrl = builder.Configuration[ConfigConstants.ApiBaseUrl];
                if (apiUrl.IsNullOrWhiteSpace())
                    apiUrl = selfUrl;
                return new BaseUrl(apiUrl, selfUrl);
            }
        );
        builder.Services.AddScoped<IExamAnswerRepository, ExamAnswerRepository>();
        builder.Services.AddScoped<IExamQuestionRepository, ExamQuestionRepository>();
        builder.Services.AddScoped<IExamRepository, ExamRepository>();
        builder.Services.AddScoped<IExamTopicRepository, ExamTopicRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<CsvService>();


        await builder.Build()
            .RunAsync();
    }
}
