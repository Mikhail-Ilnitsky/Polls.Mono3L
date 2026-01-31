using Ilnitsky.Polls.DataAccess.Entities.Answers;
using Ilnitsky.Polls.DataAccess.Entities.Polls;
using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.DataAccess;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Poll> Polls { get; set; }

    public DbSet<Respondent> Respondents { get; set; }
    public DbSet<RespondentSession> RespondentSessions { get; set; }
    public DbSet<RespondentAnswer> RespondentAnswers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Poll>(b =>
        {
            b.Property(p => p.Id)
                .HasColumnType("UUID");
            b.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();
            b.Property(p => p.Html)
                .IsRequired(false);

            b.HasMany(p => p.Questions)
                .WithOne(q => q.Poll)
                .HasForeignKey(q => q.PollId)
                .IsRequired();
            b.HasMany(p => p.RespondentAnswers)
                .WithOne(a => a.Poll)
                .HasForeignKey(a => a.PollId)
                .IsRequired();
        });

        modelBuilder.Entity<Question>(b =>
        {
            b.Property(p => p.Id)
                .HasColumnType("UUID");
            b.Property(q => q.Text)
                .HasMaxLength(100)
                .IsRequired();
            b.Property(q => q.TargetAnswer)
                .IsRequired(false);
            b.Property(q => q.MatchNextNumber)
                .IsRequired(false);
            b.Property(q => q.DefaultNextNumber)
                .IsRequired(false);

            b.HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .IsRequired();
            b.HasMany(q => q.RespondentAnswers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .IsRequired();
        });

        modelBuilder.Entity<Answer>(b =>
        {
            b.Property(e => e.Id)
                .HasColumnType("UUID");
            b.Property(e => e.Text)
                .HasMaxLength(100)
                .IsRequired();
        });

        modelBuilder.Entity<Respondent>(b =>
        {
            b.Property(e => e.Id)
                .HasColumnType("UUID");

            b.HasMany(r => r.RespondentSessions)
                .WithOne(s => s.Respondent)
                .HasForeignKey(s => s.RespondentId)
                .IsRequired();
            b.HasMany(r => r.RespondentAnswers)
                .WithOne(a => a.Respondent)
                .HasForeignKey(a => a.RespondentId)
                .IsRequired();
        });

        modelBuilder.Entity<RespondentSession>(b =>
        {
            b.Property(e => e.Id)
                .HasColumnType("UUID");

            b.HasMany(s => s.RespondentAnswers)
                .WithOne(a => a.RespondentSession)
                .HasForeignKey(a => a.RespondentSessionId)
                .IsRequired();
        });

        modelBuilder.Entity<RespondentAnswer>(b =>
        {
            b.Property(e => e.Id)
                .HasColumnType("UUID");
            b.Property(e => e.Text)
                .HasMaxLength(100)
                .IsRequired();
        });
    }
}
