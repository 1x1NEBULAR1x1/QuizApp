using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    [Authorize]
    public class QuizzesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QuizzesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Quizzes
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var quizzes = await _context.Quizzes
                .Include(q => q.User)
                .Include(q => q.Questions)
                .Where(q => q.UserId == currentUser.Id)
                .ToListAsync();

            return View(quizzes);
        }

        // GET: Quizzes/Browse
        [AllowAnonymous]
        public async Task<IActionResult> Browse()
        {
            var quizzes = await _context.Quizzes
                .Include(q => q.User)
                .ToListAsync();

            return View(quizzes);
        }

        // GET: Quizzes/Details/5
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
                .Include(q => q.User)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUser.Id);

            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // GET: Quizzes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Quizzes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description")] Quiz quiz)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }
            quiz.UserId = currentUser.Id;
            var newQuiz = new Quiz
            {
                Title = quiz.Title,
                Description = quiz.Description ?? string.Empty,
                UserId = quiz.UserId,
                CreatedAt = DateTime.Now
            };
            _context.Quizzes.Add(newQuiz);
            var result = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Quizzes/Edit/5
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

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id && q.UserId == currentUser.Id);

            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // POST: Quizzes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description")] Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var existingQuiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && q.UserId == currentUser.Id);

            if (existingQuiz == null)
            {
                return NotFound();
            }
            existingQuiz.Title = quiz.Title;
            existingQuiz.Description = quiz.Description;

            _context.Update(existingQuiz);
            await _context.SaveChangesAsync();
            if (!QuizExists(quiz.Id))
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Quizzes/Delete/5
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

            var quiz = await _context.Quizzes
                .Include(q => q.User)
                .Include(q => q.Questions)
                .Include(q => q.Attempts)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUser.Id);

            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // POST: Quizzes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.UserAnswers)
                .Include(q => q.Attempts)
                    .ThenInclude(a => a.UserAnswers)
                .FirstOrDefaultAsync(q => q.Id == id && q.UserId == currentUser.Id);

            if (quiz == null)
            {
                return NotFound();
            }

            foreach (var question in quiz.Questions)
            {
                if (question.UserAnswers != null && question.UserAnswers.Any())
                {
                    _context.UserAnswers.RemoveRange(question.UserAnswers);
                }
            }
            foreach (var attempt in quiz.Attempts)
            {
                if (attempt.UserAnswers != null && attempt.UserAnswers.Any())
                {
                    _context.UserAnswers.RemoveRange(attempt.UserAnswers);
                }
            }
            if (quiz.Attempts != null && quiz.Attempts.Any())
            {
                _context.QuizAttempts.RemoveRange(quiz.Attempts);
            }
            foreach (var question in quiz.Questions)
            {
                if (question.Answers != null && question.Answers.Any())
                {
                    _context.Answers.RemoveRange(question.Answers);
                }
            }
            if (quiz.Questions != null && quiz.Questions.Any())
            {
                _context.Questions.RemoveRange(quiz.Questions);
            }
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }
    }
}
