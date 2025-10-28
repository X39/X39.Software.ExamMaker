using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using X39.Software.ExamMaker.Api.Storage.Exam;

namespace X39.Software.ExamMaker.Api.Storage.Authority;

public static class HostApplicationBuilderExtensions
{
    public static void AddAuthorityDb(this IHostApplicationBuilder self, string connectionStringName)
    {
        self.Services.AddDbContextPool<AuthorityDbContext>(options => options.UseNpgsql(
                self.Configuration.GetConnectionString(connectionStringName),
                npgsqlOptions => npgsqlOptions.UseNodaTime()
            )
        );
    }
}
