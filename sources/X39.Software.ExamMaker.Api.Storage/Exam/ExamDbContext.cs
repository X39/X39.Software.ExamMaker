using Microsoft.EntityFrameworkCore;
using X39.Software.ExamMaker.Api.Storage.Exam.Entities;

namespace X39.Software.ExamMaker.Api.Storage.Exam;

public class ExamDbContext(DbContextOptions<ExamDbContext> options) : DbContext(options)
{
    public DbSet<OrganizationRegistrationToken> OrganizationRegistrationTokens { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }
    public DbSet<Entities.Exam> Exams { get; set; }
    public DbSet<ExamTopic> ExamTopics { get; set; }
    public DbSet<ExamAnswer> ExamAnswers { get; set; }
    public DbSet<ExamQuestion> ExamQuestions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
        base.OnModelCreating(modelBuilder);
    }
}
