using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using QuizApp.ViewModels;

namespace QuizApp.Controllers
{
    [Authorize]
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QuestionsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Questions
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Questions.Include(q => q.Quiz);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: Questions/Create
        public async Task<IActionResult> Create(int quizId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == quizId && q.UserId == currentUser.Id);

            if (quiz == null)
            {
                return NotFound();
            }

            var viewModel = new QuestionFormViewModel
            {
                QuizId = quizId,
                QuizTitle = quiz.Title,
                Points = 1,
                Answers = new List<AnswerFormViewModel>
                {
                    new AnswerFormViewModel { IsCorrect = true },
                    new AnswerFormViewModel()
                }
            };

            return View(viewModel);
        }

        // POST: Questions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == viewModel.QuizId && q.UserId == currentUser.Id);

            if (quiz == null)
            {
                return NotFound();
            }

            var question = new Question
            {
                Text = viewModel.Text ?? string.Empty,
                Points = viewModel.Points,
                QuizId = viewModel.QuizId,
                Answers = new List<Answer>()
            };

            foreach (var answerVM in viewModel.Answers.Where(a => !string.IsNullOrWhiteSpace(a.Text)))
            {
                question.Answers.Add(new Answer
                {
                    Text = answerVM.Text ?? string.Empty,
                    IsCorrect = answerVM.IsCorrect
                });
            }

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Quizzes", new { id = viewModel.QuizId });
        }

        // GET: Questions/Edit/5
        public async Task<IActionResult> Edit(int? id)
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

            var question = await _context.Questions
                .Include(q => q.Quiz)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id && q.Quiz.UserId == currentUser.Id);

            if (question == null)
            {
                return NotFound();
            }

            var viewModel = new QuestionFormViewModel
            {
                Id = question.Id,
                Text = question.Text ?? string.Empty,
                Points = question.Points,
                QuizId = question.QuizId,
                QuizTitle = question.Quiz.Title ?? string.Empty,
                Answers = question.Answers.Select(a => new AnswerFormViewModel
                {
                    Id = a.Id,
                    Text = a.Text ?? string.Empty,
                    IsCorrect = a.IsCorrect
                }).ToList()
            };

            if (viewModel.Answers.Count < 2)
            {
                while (viewModel.Answers.Count < 2)
                {
                    viewModel.Answers.Add(new AnswerFormViewModel());
                }
            }

            return View(viewModel);
        }

        // POST: Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuestionFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var question = await _context.Questions
                .Include(q => q.Quiz)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id && q.Quiz.UserId == currentUser.Id);

            if (question == null)
            {
                return NotFound();
            }

            question.Text = viewModel.Text;
            question.Points = viewModel.Points;

            var existingAnswerIds = question.Answers.Select(a => a.Id).ToList();
            var submittedAnswerIds = viewModel.Answers
                .Where(a => a.Id.HasValue)
                .Select(a => a.Id.GetValueOrDefault())
                .ToList();

            foreach (var answerId in existingAnswerIds.Except(submittedAnswerIds))
            {
                var answerToRemove = question.Answers.FirstOrDefault(a => a.Id == answerId);
                if (answerToRemove != null)
                {
                    _context.Answers.Remove(answerToRemove);
                }
            }

            foreach (var answerVM in viewModel.Answers.Where(a => !string.IsNullOrWhiteSpace(a.Text)))
            {
                if (answerVM.Id.HasValue)
                {
                    int answerId = answerVM.Id.GetValueOrDefault();
                    var existingAnswer = question.Answers.FirstOrDefault(a => a.Id == answerId);
                    if (existingAnswer != null)
                    {
                        existingAnswer.Text = answerVM.Text ?? string.Empty;
                        existingAnswer.IsCorrect = answerVM.IsCorrect;
                    }
                }
                else
                {
                    question.Answers.Add(new Answer
                    {
                        Text = answerVM.Text ?? string.Empty,
                        IsCorrect = answerVM.IsCorrect,
                        QuestionId = question.Id
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(question.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Details", "Quizzes", new { id = question.QuizId });
        }

        // GET: Questions/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            var question = await _context.Questions
                .Include(q => q.Quiz)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id && q.Quiz.UserId == currentUser.Id);

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var question = await _context.Questions
                .Include(q => q.Quiz)
                .Include(q => q.Answers)
                .Include(q => q.UserAnswers)
                .FirstOrDefaultAsync(q => q.Id == id && q.Quiz.UserId == currentUser.Id);

            if (question == null)
            {
                return NotFound();
            }

            int quizId = question.QuizId;

            if (question.UserAnswers != null && question.UserAnswers.Any())
            {
                _context.UserAnswers.RemoveRange(question.UserAnswers);
            }
            if (question.Answers != null && question.Answers.Any())
            {
                _context.Answers.RemoveRange(question.Answers);
            }
            _context.Questions.Remove(question);

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Quizzes", new { id = quizId });
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
}
