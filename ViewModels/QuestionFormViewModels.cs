using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace QuizApp.ViewModels
{
  public class QuestionFormViewModel : IValidatableObject
  {
    public int? Id { get; set; }

    [Required(ErrorMessage = "Question text is required")]
    [StringLength(500, ErrorMessage = "Question text cannot be longer than 500 characters")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "Points value is required")]
    [Range(1, 100, ErrorMessage = "Points must be between 1 and 100")]
    public int Points { get; set; }

    [Required]
    public int QuizId { get; set; }

    public string QuizTitle { get; set; } = string.Empty;

    public List<AnswerFormViewModel> Answers { get; set; } = new List<AnswerFormViewModel>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      var validAnswers = Answers.Where(a => !string.IsNullOrWhiteSpace(a.Text)).ToList();

      if (validAnswers.Count < 2)
      {
        yield return new ValidationResult(
          "At least two answers are required.",
          new[] { nameof(Answers) });
      }

      int correctAnswersCount = validAnswers.Count(a => a.IsCorrect);
      if (correctAnswersCount != 1)
      {
        yield return new ValidationResult(
          "Exactly one correct answer must be selected.",
          new[] { nameof(Answers) });
      }
    }
  }

  public class AnswerFormViewModel
  {
    public int? Id { get; set; }

    [StringLength(500, ErrorMessage = "Answer text cannot be longer than 500 characters")]
    public string? Text { get; set; }

    public bool IsCorrect { get; set; }
  }
}