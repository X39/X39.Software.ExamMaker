using X39.Software.ExamMaker.Api.Services;

namespace X39.Software.ExamMaker.Api.Middleware;

public class TokenValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, JwtService jwtService)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader["Bearer ".Length..];
            if (await jwtService.IsTokenRevokedAsync(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token has been revoked");
                return;
            }
        }

        await next(context);
    }
}
