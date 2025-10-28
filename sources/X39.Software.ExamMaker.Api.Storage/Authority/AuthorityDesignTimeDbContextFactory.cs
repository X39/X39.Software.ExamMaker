using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using X39.Software.ExamMaker.Api.Storage.Exam;

namespace X39.Software.ExamMaker.Api.Storage.Authority;

public class AuthorityDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthorityDbContext>
{
    public AuthorityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthorityDbContext>();
        optionsBuilder.UseNpgsql(o => o.UseNodaTime());

        return new AuthorityDbContext(optionsBuilder.Options);
    }
}
