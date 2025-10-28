using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace X39.Software.ExamMaker.Api.Storage.Exam;

public static class HostApplicationBuilderExtensions
{
    public static void AddExamDb(this IHostApplicationBuilder self, string connectionStringName)
    {
        self.Services.AddDbContextPool<ExamDbContext>(options => options.UseNpgsql(
                self.Configuration.GetConnectionString(connectionStringName),
                npgsqlOptions => npgsqlOptions.UseNodaTime()
            )
        );
    }
}
