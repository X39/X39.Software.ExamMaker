using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using X39.Software.ExamMaker.Api.Configuration;
using X39.Software.ExamMaker.Api.OpenApiTransformers;
using X39.Software.ExamMaker.Api.Services;
using X39.Software.ExamMaker.Api.Storage.Authority;
using X39.Software.ExamMaker.Api.Storage.Exam;
using X39.Software.ExamMaker.Api.Storage.Meta;
using X39.Solutions.PdfTemplate;

namespace X39.Software.ExamMaker.Api;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.AddServiceDefaults();
        builder.AddAuthorityDb("auth-db");
        builder.AddExamDb("exam-db");
        builder.Services.AddControllers();
        builder.Services.AddPdfTemplateServices();
        
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddOperationTransformer<XAnonymousAnnotationOperationTransformer>();
                options.AddSchemaTransformer<FixEnumsSchemaTransformer>();
            }
        );
        
        builder.Services.Configure<Secrets>(builder.Configuration.GetSection("Secrets"));
        // Add services to the container.
        builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("Jwt"));
        builder.Services.AddScoped<JwtService>();

        // Configure JWT authentication
        var jwtConfig = builder.Configuration
                            .GetSection("Jwt")
                            .Get<JwtConfig>()
                        ?? new JwtConfig();
        builder.Services.AddSingleton(jwtConfig);
        builder.Services
            .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
                }
            )
            .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer           = true,
                        ValidateAudience         = true,
                        ValidateLifetime         = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer              = jwtConfig.Issuer,
                        ValidAudience            = jwtConfig.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                jwtConfig?.Key ?? throw new InvalidOperationException("JWT Key is not configured")
                            )
                        ),
                    };
                }
            );
        
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Setup CORS ignorance in development because all browsers are stupid shitshows
        if (app.Environment.IsDevelopment())
        {
            app.UseCors(cors => cors.SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
            );
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseResponseCaching();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        await app.MigrateAuthorityDbAsync();
        await app.MigrateExamDbAsync();
        await app.RunAsync()
            .ConfigureAwait(false);
    }
}
