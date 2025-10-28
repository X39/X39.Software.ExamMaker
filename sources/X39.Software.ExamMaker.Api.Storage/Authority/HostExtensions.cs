using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using X39.Software.ExamMaker.Api.Storage.Exam;

namespace X39.Software.ExamMaker.Api.Storage.Authority;

public static class HostExtensions
{
    public static async Task MigrateAuthorityDbAsync(this IHost self)
    {
        await using var serviceScope = self.Services.CreateAsyncScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AuthorityDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
