using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace X39.Software.ExamMaker.Api.Storage.Exam;

public class ExamDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ExamDbContext>
{
    public ExamDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ExamDbContext>();
        optionsBuilder.UseNpgsql(o => o.UseNodaTime());

        return new ExamDbContext(optionsBuilder.Options);
    }
}
