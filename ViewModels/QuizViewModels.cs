using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizApp.ViewModels
{
  public class TakeQuizViewModel
  {
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
  }

  public class QuestionViewModel
  {
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Points { get; set; }
    public List<AnswerViewModel> Answers { get; set; } = new List<AnswerViewModel>();
    public int? SelectedAnswerId { get; set; }
  }

  public class AnswerViewModel
  {
    public int AnswerId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
  }

  public class SubmitQuizViewModel
  {
    public int AttemptId { get; set; }
    public List<UserAnswerViewModel> Answers { get; set; } = new List<UserAnswerViewModel>();
  }

  public class UserAnswerViewModel
  {
    [Required]
    public int QuestionId { get; set; }

    [Required]
    public int AnswerId { get; set; }
  }

  public class QuizResultViewModel
  {
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalPossibleScore { get; set; }
    public double ScorePercentage { get; set; }
    public DateTime CompletedAt { get; set; }
    public List<QuestionResultViewModel> Questions { get; set; } = new List<QuestionResultViewModel>();
  }

  public class QuestionResultViewModel
  {
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Points { get; set; }
    public AnswerResultViewModel UserAnswer { get; set; } = new AnswerResultViewModel();
    public AnswerResultViewModel CorrectAnswer { get; set; } = new AnswerResultViewModel();
    public bool IsCorrect { get; set; }
  }

  public class AnswerResultViewModel
  {
    public int AnswerId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
  }
}