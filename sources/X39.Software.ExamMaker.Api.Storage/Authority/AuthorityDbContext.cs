using Microsoft.EntityFrameworkCore;
using X39.Software.ExamMaker.Api.Storage.Authority.Entities;
using X39.Software.ExamMaker.Api.Storage.Exam;

namespace X39.Software.ExamMaker.Api.Storage.Authority;

public class AuthorityDbContext(DbContextOptions<AuthorityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
        base.OnModelCreating(modelBuilder);
    }
}
