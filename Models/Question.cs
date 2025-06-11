using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
  public class Question
  {
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Text { get; set; } = string.Empty;

    [Required]
    [Range(1, 100)]
    public int Points { get; set; }

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public List<Answer> Answers { get; set; } = new List<Answer>();
    public List<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
  }
}