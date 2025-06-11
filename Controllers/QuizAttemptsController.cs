using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizApp.Controllers
{
  [Authorize]
  public class QuizAttemptsController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public QuizAttemptsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
      _context = context;
      _userManager = userManager;
    }

    // GET: QuizAttempts
    public async Task<IActionResult> Index()
    {
      var currentUser = await _userManager.GetUserAsync(User);
      if (currentUser == null)
      {
        return Challenge();
      }

      var attempts = await _context.QuizAttempts
          .Include(a => a.Quiz)
          .Where(a => a.UserId == currentUser.Id)
          .OrderByDescending(a => a.StartedAt)
          .ToListAsync();

      return View(attempts);
    }

    // GET: QuizAttempts/Details/5
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

      var attempt = await _context.QuizAttempts
          .Include(a => a.Quiz)
          .Include(a => a.UserAnswers)
              .ThenInclude(ua => ua.Question)
          .Include(a => a.UserAnswers)
              .ThenInclude(ua => ua.Answer)
          .FirstOrDefaultAsync(a => a.Id == id && a.UserId == currentUser.Id);

      if (attempt == null)
      {
        return NotFound();
      }

      return View(attempt);
    }

    // GET: QuizAttempts/Start/5
    public async Task<IActionResult> Start(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var quiz = await _context.Quizzes
          .Include(q => q.Questions)
              .ThenInclude(q => q.Answers)
          .FirstOrDefaultAsync(q => q.Id == id);

      if (quiz == null)
      {
        return NotFound();
      }

      var currentUser = await _userManager.GetUserAsync(User);
      if (currentUser == null)
      {
        return Challenge();
      }

      var quizAttempt = new QuizAttempt
      {
        QuizId = quiz.Id,
        UserId = currentUser.Id,
        StartedAt = DateTime.Now,
        TotalPossibleScore = quiz.Questions.Sum(q => q.Points)
      };

      _context.Add(quizAttempt);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Take), new { id = quizAttempt.Id });
    }

    // GET: QuizAttempts/Take/5
    public async Task<IActionResult> Take(int? id)
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

      var attempt = await _context.QuizAttempts
          .Include(a => a.Quiz)
              .ThenInclude(q => q.Questions)
                  .ThenInclude(q => q.Answers)
          .Include(a => a.UserAnswers)
          .FirstOrDefaultAsync(a => a.Id == id && a.UserId == currentUser.Id);

      if (attempt == null)
      {
        return NotFound();
      }

      if (attempt.CompletedAt != null)
      {
        return RedirectToAction(nameof(Details), new { id = attempt.Id });
      }

      var viewModel = new TakeQuizViewModel
      {
        AttemptId = attempt.Id,
        QuizId = attempt.QuizId,
        QuizTitle = attempt.Quiz.Title,
        Questions = attempt.Quiz.Questions.Select(q => new QuestionViewModel
        {
          QuestionId = q.Id,
          QuestionText = q.Text,
          Points = q.Points,
          Answers = q.Answers.Select(a => new AnswerViewModel
          {
            AnswerId = a.Id,
            AnswerText = a.Text
          }).ToList()
        }).ToList()
      };

      return View(viewModel);
    }

    // POST: QuizAttempts/Submit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitQuizViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return RedirectToAction(nameof(Take), new { id = model.AttemptId });
      }

      var currentUser = await _userManager.GetUserAsync(User);
      if (currentUser == null)
      {
        return Challenge();
      }

      var attempt = await _context.QuizAttempts
          .Include(a => a.Quiz)
              .ThenInclude(q => q.Questions)
                  .ThenInclude(q => q.Answers)
          .FirstOrDefaultAsync(a => a.Id == model.AttemptId && a.UserId == currentUser.Id);

      if (attempt == null)
      {
        return NotFound();
      }

      if (attempt.CompletedAt != null)
      {
        return RedirectToAction(nameof(Details), new { id = attempt.Id });
      }

      int score = 0;

      foreach (var answer in model.Answers)
      {
        var question = attempt.Quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
        var selectedAnswer = question?.Answers.FirstOrDefault(a => a.Id == answer.AnswerId);

        if (question != null && selectedAnswer != null)
        {
          var userAnswer = new UserAnswer
          {
            QuestionId = question.Id,
            AnswerId = selectedAnswer.Id,
            QuizAttemptId = attempt.Id
          };

          _context.UserAnswers.Add(userAnswer);

          if (selectedAnswer.IsCorrect)
          {
            score += question.Points;
          }
        }
      }

      attempt.Score = score;
      attempt.CompletedAt = DateTime.Now;

      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Results), new { id = attempt.Id });
    }

    // GET: QuizAttempts/Results/5
    public async Task<IActionResult> Results(int? id)
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

      var attempt = await _context.QuizAttempts
          .Include(a => a.Quiz)
          .Include(a => a.UserAnswers)
              .ThenInclude(ua => ua.Question)
          .Include(a => a.UserAnswers)
              .ThenInclude(ua => ua.Answer)
                  .ThenInclude(a => a.Question)
                      .ThenInclude(q => q.Answers)
          .FirstOrDefaultAsync(a => a.Id == id && a.UserId == currentUser.Id);

      if (attempt == null)
      {
        return NotFound();
      }

      var resultViewModel = new QuizResultViewModel
      {
        AttemptId = attempt.Id,
        QuizId = attempt.QuizId,
        QuizTitle = attempt.Quiz.Title,
        Score = attempt.Score,
        TotalPossibleScore = attempt.TotalPossibleScore,
        ScorePercentage = (double)attempt.Score / attempt.TotalPossibleScore * 100,
        CompletedAt = attempt.CompletedAt ?? DateTime.Now,
        Questions = new List<QuestionResultViewModel>()
      };

      var questionAnswers = attempt.UserAnswers.GroupBy(ua => ua.QuestionId);

      foreach (var questionGroup in questionAnswers)
      {
        var userAnswer = questionGroup.First();
        var question = userAnswer.Question;
        var selectedAnswer = userAnswer.Answer;
        var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);

        if (correctAnswer == null)
        {
          correctAnswer = question.Answers.FirstOrDefault() ?? new Answer { Id = 0, Text = "No correct answer", IsCorrect = false };
        }

        resultViewModel.Questions.Add(new QuestionResultViewModel
        {
          QuestionId = question.Id,
          QuestionText = question.Text,
          Points = question.Points,
          UserAnswer = new AnswerResultViewModel
          {
            AnswerId = selectedAnswer.Id,
            AnswerText = selectedAnswer.Text,
            IsCorrect = selectedAnswer.IsCorrect
          },
          CorrectAnswer = new AnswerResultViewModel
          {
            AnswerId = correctAnswer.Id,
            AnswerText = correctAnswer.Text,
            IsCorrect = true
          },
          IsCorrect = selectedAnswer.IsCorrect
        });
      }

      return View(resultViewModel);
    }
  }
}