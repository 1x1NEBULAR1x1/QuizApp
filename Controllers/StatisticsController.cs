using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizApp.Controllers
{
  [Authorize]
  public class StatisticsController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public StatisticsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
      _context = context;
      _userManager = userManager;
    }

    // GET: Statistics
    public async Task<IActionResult> Index()
    {
      var currentUser = await _userManager.GetUserAsync(User);
      if (currentUser == null)
      {
        return Challenge();
      }

      var quizzes = await _context.Quizzes
          .Where(q => q.UserId == currentUser.Id)
          .Include(q => q.Attempts)
          .ToListAsync();

      var statistics = new List<QuizStatisticsViewModel>();

      foreach (var quiz in quizzes)
      {
        double averageScore = 0;
        if (quiz.Attempts.Count > 0)
        {
          double totalScorePercentage = 0;
          foreach (var attempt in quiz.Attempts)
          {
            if (attempt.TotalPossibleScore > 0)
            {
              double attemptPercentage = (double)attempt.Score / attempt.TotalPossibleScore * 100;
              totalScorePercentage += attemptPercentage;
            }
          }
          averageScore = totalScorePercentage / quiz.Attempts.Count;
        }

        var quizStats = new QuizStatisticsViewModel
        {
          QuizId = quiz.Id,
          QuizTitle = quiz.Title,
          AttemptCount = quiz.Attempts.Count,
          AverageScore = averageScore
        };

        statistics.Add(quizStats);
      }

      return View(statistics);
    }

    // GET: Statistics/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var currentUser = await _userManager.GetUserAsync(User);
      if (currentUser == null)
      {
        return Challenge();
      }

      var quiz = await _context.Quizzes
          .Include(q => q.Questions)
              .ThenInclude(q => q.Answers)
                  .ThenInclude(a => a.UserAnswers)
          .Include(q => q.Attempts)
          .FirstOrDefaultAsync(q => q.Id == id && q.UserId == currentUser.Id);

      if (quiz == null)
      {
        return NotFound();
      }

      var questionStats = new List<QuestionStatisticsViewModel>();

      foreach (var question in quiz.Questions)
      {
        var answerStats = new List<AnswerStatisticsViewModel>();
        int totalAnswers = question.UserAnswers.Count;

        foreach (var answer in question.Answers)
        {
          int answerCount = answer.UserAnswers.Count;
          double percentage = totalAnswers > 0 ? (double)answerCount / totalAnswers * 100 : 0;

          answerStats.Add(new AnswerStatisticsViewModel
          {
            AnswerId = answer.Id,
            AnswerText = answer.Text,
            IsCorrect = answer.IsCorrect,
            SelectionCount = answerCount,
            SelectionPercentage = percentage
          });
        }

        questionStats.Add(new QuestionStatisticsViewModel
        {
          QuestionId = question.Id,
          QuestionText = question.Text,
          AnswerStatistics = answerStats
        });
      }

      double averageScore = 0;
      if (quiz.Attempts.Count > 0)
      {
        double totalScorePercentage = 0;
        foreach (var attempt in quiz.Attempts)
        {
          if (attempt.TotalPossibleScore > 0)
          {
            double attemptPercentage = (double)attempt.Score / attempt.TotalPossibleScore * 100;
            totalScorePercentage += attemptPercentage;
          }
        }
        averageScore = totalScorePercentage / quiz.Attempts.Count;
      }

      var viewModel = new QuizDetailedStatisticsViewModel
      {
        QuizId = quiz.Id,
        QuizTitle = quiz.Title,
        AttemptCount = quiz.Attempts.Count,
        AverageScore = averageScore,
        QuestionStatistics = questionStats
      };

      return View(viewModel);
    }
  }
}