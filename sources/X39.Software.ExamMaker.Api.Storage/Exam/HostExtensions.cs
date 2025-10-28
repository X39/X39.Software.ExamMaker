using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace X39.Software.ExamMaker.Api.Storage.Exam;

public static class HostExtensions
{
    public static async Task MigrateExamDbAsync(this IHost self)
    {
        await using var serviceScope = self.Services.CreateAsyncScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<ExamDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
