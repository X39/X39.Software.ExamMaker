using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace X39.Software.ExamMaker.Api;

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Bearer"] = new()
                {
                    Type         = SecuritySchemeType.Http,
                    Scheme       = "bearer",
                    BearerFormat = "Json Web Token",
                    In           = ParameterLocation.Header,
                    Name         = "Authorization",
                    Description =
                        "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                },
            };
            document.Components                 ??= new OpenApiComponents();
            document.Components.SecuritySchemes =   requirements;

            // Apply it as a requirement for all operations
            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                if (!operation.Value.Annotations.TryGetValue("x-anonymous", out var obj) || obj is not true)
                {
                    operation.Value.Security.Add(
                        new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme },
                            }] = Array.Empty<string>(),
                        }
                    );
                }
            }
        }
    }
}
