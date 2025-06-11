using Microsoft.AspNetCore.Identity;

namespace QuizApp.Models
{
  public class UserAnswer
  {
    public int Id { get; set; }

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public int AnswerId { get; set; }
    public Answer Answer { get; set; } = null!;

    public int QuizAttemptId { get; set; }
    public QuizAttempt QuizAttempt { get; set; } = null!;
  }
}