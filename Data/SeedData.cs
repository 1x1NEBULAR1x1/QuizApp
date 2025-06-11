using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizApp.Models;

namespace QuizApp.Data
{
  public static class SeedData
  {
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
      using var context = new ApplicationDbContext(
          serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

      var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

      await ClearDatabase(context);

      var users = await CreateTestUsers(userManager);

      await CreateTestData(context, users);
    }

    private static async Task ClearDatabase(ApplicationDbContext context)
    {
      context.UserAnswers.RemoveRange(context.UserAnswers);
      await context.SaveChangesAsync();

      context.QuizAttempts.RemoveRange(context.QuizAttempts);
      await context.SaveChangesAsync();

      context.Answers.RemoveRange(context.Answers);
      await context.SaveChangesAsync();

      context.Questions.RemoveRange(context.Questions);
      await context.SaveChangesAsync();

      context.Quizzes.RemoveRange(context.Quizzes);
      await context.SaveChangesAsync();
    }

    private static async Task<List<IdentityUser>> CreateTestUsers(UserManager<IdentityUser> userManager)
    {
      var users = new List<IdentityUser>();

      string[] testUsers =
      {
        "test1@example.com",
        "test2@example.com",
        "teacher@example.com"
      };

      foreach (var email in testUsers)
      {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
          user = new IdentityUser
          {
            UserName = email,
            Email = email,
            EmailConfirmed = true
          };

          await userManager.CreateAsync(user, "Pass123$");
          user = await userManager.FindByEmailAsync(email);
        }

        if (user != null)
        {
          users.Add(user);
        }
        else
        {
          Console.WriteLine($"Warning: Failed to create or retrieve user with email {email}");
        }
      }

      return users;
    }

    private static async Task CreateTestData(ApplicationDbContext context, List<IdentityUser> users)
    {
      var quizzes = new List<Quiz>
      {
        new Quiz
        {
          Title = "Podstawy matematyki",
          Description = "Quiz z podstawowych zagadnień matematycznych",
          UserId = users[0].Id,
          CreatedAt = DateTime.Now.AddDays(-10),
          Questions = new List<Question>
          {
            new Question
            {
              Text = "Ile wynosi 2 + 2?",
              Points = 1,
              Answers = new List<Answer>
              {
                new Answer { Text = "3", IsCorrect = false },
                new Answer { Text = "4", IsCorrect = true },
                new Answer { Text = "5", IsCorrect = false }
              }
            },
            new Question
            {
              Text = "Ile wynosi pierwiastek kwadratowy z 16?",
              Points = 2,
              Answers = new List<Answer>
              {
                new Answer { Text = "2", IsCorrect = false },
                new Answer { Text = "4", IsCorrect = true },
                new Answer { Text = "8", IsCorrect = false },
                new Answer { Text = "16", IsCorrect = false }
              }
            },
            new Question
            {
              Text = "Jaka jest wartość liczby Pi (zaokrąglona do dwóch miejsc po przecinku)?",
              Points = 3,
              Answers = new List<Answer>
              {
                new Answer { Text = "3.14", IsCorrect = true },
                new Answer { Text = "3.41", IsCorrect = false },
                new Answer { Text = "3.12", IsCorrect = false }
              }
            }
          }
        },
        new Quiz
        {
          Title = "Wiedza ogólna",
          Description = "Quiz sprawdzający wiedzę ogólną",
          UserId = users[1].Id,
          CreatedAt = DateTime.Now.AddDays(-5),
          Questions = new List<Question>
          {
            new Question
            {
              Text = "Która planeta jest najbliżej Słońca?",
              Points = 2,
              Answers = new List<Answer>
              {
                new Answer { Text = "Wenus", IsCorrect = false },
                new Answer { Text = "Merkury", IsCorrect = true },
                new Answer { Text = "Mars", IsCorrect = false },
                new Answer { Text = "Ziemia", IsCorrect = false }
              }
            },
            new Question
            {
              Text = "Kto napisał 'Pan Tadeusz'?",
              Points = 3,
              Answers = new List<Answer>
              {
                new Answer { Text = "Juliusz Słowacki", IsCorrect = false },
                new Answer { Text = "Henryk Sienkiewicz", IsCorrect = false },
                new Answer { Text = "Adam Mickiewicz", IsCorrect = true },
                new Answer { Text = "Bolesław Prus", IsCorrect = false }
              }
            }
          }
        },
        new Quiz
        {
          Title = "Podstawy programowania",
          Description = "Quiz o podstawach programowania w C#",
          UserId = users[2].Id,
          CreatedAt = DateTime.Now.AddDays(-2),
          Questions = new List<Question>
          {
            new Question
            {
              Text = "Jaki operator służy do porównania wartości w C#?",
              Points = 1,
              Answers = new List<Answer>
              {
                new Answer { Text = "=", IsCorrect = false },
                new Answer { Text = "==", IsCorrect = true },
                new Answer { Text = "===", IsCorrect = false },
                new Answer { Text = "!=", IsCorrect = false }
              }
            },
            new Question
            {
              Text = "Jaki typ danych przechowuje wartość true lub false?",
              Points = 1,
              Answers = new List<Answer>
              {
                new Answer { Text = "int", IsCorrect = false },
                new Answer { Text = "string", IsCorrect = false },
                new Answer { Text = "bool", IsCorrect = true },
                new Answer { Text = "float", IsCorrect = false }
              }
            },
            new Question
            {
              Text = "Co to jest dziedziczenie w programowaniu obiektowym?",
              Points = 3,
              Answers = new List<Answer>
              {
                new Answer { Text = "Mechanizm umożliwiający tworzenie nowej klasy na bazie istniejącej", IsCorrect = true },
                new Answer { Text = "Proces przypisywania wartości do zmiennej", IsCorrect = false },
                new Answer { Text = "Sposób definiowania metod w klasie", IsCorrect = false },
                new Answer { Text = "Typ pętli w programowaniu", IsCorrect = false }
              }
            },
            new Question
            {
              Text = "Jaka konstrukcja służy do obsługi wyjątków w C#?",
              Points = 2,
              Answers = new List<Answer>
              {
                new Answer { Text = "if-else", IsCorrect = false },
                new Answer { Text = "for-each", IsCorrect = false },
                new Answer { Text = "switch-case", IsCorrect = false },
                new Answer { Text = "try-catch", IsCorrect = true }
              }
            }
          }
        }
      };

      await context.Quizzes.AddRangeAsync(quizzes);
      await context.SaveChangesAsync();

      var mathQuiz = await context.Quizzes
          .Include(q => q.Questions)
              .ThenInclude(q => q.Answers)
          .FirstOrDefaultAsync(q => q.Title == "Podstawy matematyki");

      var generalKnowledgeQuiz = await context.Quizzes
          .Include(q => q.Questions)
              .ThenInclude(q => q.Answers)
          .FirstOrDefaultAsync(q => q.Title == "Wiedza ogólna");

      var programmingQuiz = await context.Quizzes
          .Include(q => q.Questions)
              .ThenInclude(q => q.Answers)
          .FirstOrDefaultAsync(q => q.Title == "Podstawy programowania");

      if (mathQuiz == null || generalKnowledgeQuiz == null || programmingQuiz == null)
      {
        Console.WriteLine("Error: One or more quizzes were not found in the database.");
        return;
      }

      var attempts = new List<QuizAttempt>
      {
        new QuizAttempt
        {
          QuizId = mathQuiz.Id,
          UserId = users[1].Id,
          StartedAt = DateTime.Now.AddDays(-2),
          CompletedAt = DateTime.Now.AddDays(-2).AddMinutes(5),
          Score = 3,
          TotalPossibleScore = 6
        },
        new QuizAttempt
        {
          QuizId = generalKnowledgeQuiz.Id,
          UserId = users[0].Id,
          StartedAt = DateTime.Now.AddDays(-1),
          CompletedAt = DateTime.Now.AddDays(-1).AddMinutes(3),
          Score = 2,
          TotalPossibleScore = 5
        },
        new QuizAttempt
        {
          QuizId = programmingQuiz.Id,
          UserId = users[1].Id,
          StartedAt = DateTime.Now.AddHours(-5),
          CompletedAt = DateTime.Now.AddHours(-5).AddMinutes(10),
          Score = 4,
          TotalPossibleScore = 7
        }
      };

      await context.QuizAttempts.AddRangeAsync(attempts);
      await context.SaveChangesAsync();

      var savedAttempts = await context.QuizAttempts.ToListAsync();
      if (savedAttempts.Count != 3)
      {
        Console.WriteLine("Error: Failed to create quiz attempts.");
        return;
      }

      if (mathQuiz.Questions.Count >= 3)
      {
        var userAnswers1 = new List<UserAnswer>
        {
          new UserAnswer
          {
            QuizAttemptId = savedAttempts[0].Id,
            QuestionId = mathQuiz.Questions[0].Id,
            AnswerId = mathQuiz.Questions[0].Answers.First(a => a.IsCorrect).Id
          },
          new UserAnswer
          {
            QuizAttemptId = savedAttempts[0].Id,
            QuestionId = mathQuiz.Questions[1].Id,
            AnswerId = mathQuiz.Questions[1].Answers.First(a => a.IsCorrect).Id
          },
          new UserAnswer
          {
            QuizAttemptId = savedAttempts[0].Id,
            QuestionId = mathQuiz.Questions[2].Id,
            AnswerId = mathQuiz.Questions[2].Answers.First(a => !a.IsCorrect).Id
          }
        };
        await context.UserAnswers.AddRangeAsync(userAnswers1);
      }

      if (generalKnowledgeQuiz.Questions.Count >= 2)
      {
        var userAnswers2 = new List<UserAnswer>
        {
          new UserAnswer
          {
            QuizAttemptId = savedAttempts[1].Id,
            QuestionId = generalKnowledgeQuiz.Questions[0].Id,
            AnswerId = generalKnowledgeQuiz.Questions[0].Answers.First(a => !a.IsCorrect).Id
          },
          new UserAnswer
          {
            QuizAttemptId = savedAttempts[1].Id,
            QuestionId = generalKnowledgeQuiz.Questions[1].Id,
            AnswerId = generalKnowledgeQuiz.Questions[1].Answers.First(a => a.IsCorrect).Id
          }
        };
        await context.UserAnswers.AddRangeAsync(userAnswers2);
      }

      if (programmingQuiz.Questions.Count >= 4)
      {
        var userAnswers3 = new List<UserAnswer>
        {
          new UserAnswer
          {
            QuizAttemptId = savedAttempts[2].Id,
            QuestionId = programmingQuiz.Questions[0].Id,
            AnswerId = programmingQuiz.Questions[0].Answers.First(a => a.IsCorrect).Id
          },
          new UserAnswer
          {
            QuizAttemptId = savedAttempts[2].Id,
            QuestionId = programmingQuiz.Questions[1].Id,
            AnswerId = programmingQuiz.Questions[1].Answers.First(a => a.IsCorrect).Id
          },
          new UserAnswer
          {
            QuizAttemptId = savedAttempts[2].Id,
            QuestionId = programmingQuiz.Questions[2].Id,
            AnswerId = programmingQuiz.Questions[2].Answers.First(a => a.IsCorrect).Id
          },
          new UserAnswer
          {
            QuizAttemptId = savedAttempts[2].Id,
            QuestionId = programmingQuiz.Questions[3].Id,
            AnswerId = programmingQuiz.Questions[3].Answers.First(a => !a.IsCorrect).Id
          }
        };
        await context.UserAnswers.AddRangeAsync(userAnswers3);
      }

      await context.SaveChangesAsync();
      Console.WriteLine("Test data initialized successfully.");
    }
  }
}