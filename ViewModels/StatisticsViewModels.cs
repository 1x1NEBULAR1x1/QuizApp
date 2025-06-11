using System.Collections.Generic;

namespace QuizApp.ViewModels
{
  public class QuizStatisticsViewModel
  {
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public double AverageScore { get; set; }
  }

  public class QuizDetailedStatisticsViewModel : QuizStatisticsViewModel
  {
    public List<QuestionStatisticsViewModel> QuestionStatistics { get; set; } = new List<QuestionStatisticsViewModel>();
  }

  public class QuestionStatisticsViewModel
  {
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<AnswerStatisticsViewModel> AnswerStatistics { get; set; } = new List<AnswerStatisticsViewModel>();
  }

  public class AnswerStatisticsViewModel
  {
    public int AnswerId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int SelectionCount { get; set; }
    public double SelectionPercentage { get; set; }
  }
}