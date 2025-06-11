using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizApp.Models;

namespace QuizApp.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<UserAnswer> UserAnswers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Quiz - User (one-to-many)
        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Question - Quiz (one-to-many)
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Quiz)
            .WithMany(qz => qz.Questions)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        // Answer - Question (one-to-many)
        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // QuizAttempt - User (one-to-many)
        modelBuilder.Entity<QuizAttempt>()
            .HasOne(qa => qa.User)
            .WithMany()
            .HasForeignKey(qa => qa.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete attempts when user is deleted

        // QuizAttempt - Quiz (one-to-many)
        modelBuilder.Entity<QuizAttempt>()
            .HasOne(qa => qa.Quiz)
            .WithMany(q => q.Attempts)
            .HasForeignKey(qa => qa.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserAnswer relationships
        modelBuilder.Entity<UserAnswer>()
            .HasOne(ua => ua.Question)
            .WithMany(q => q.UserAnswers)
            .HasForeignKey(ua => ua.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserAnswer>()
            .HasOne(ua => ua.Answer)
            .WithMany(a => a.UserAnswers)
            .HasForeignKey(ua => ua.AnswerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserAnswer>()
            .HasOne(ua => ua.QuizAttempt)
            .WithMany(qa => qa.UserAnswers)
            .HasForeignKey(ua => ua.QuizAttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
