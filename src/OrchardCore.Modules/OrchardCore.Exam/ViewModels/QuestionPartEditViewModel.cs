using OrchardCore.Exam.Models;

namespace OrchardCore.Exam.ViewModels;

public class QuestionPartEditViewModel
{
    public QuestionType QuestionType { get; set; }
    public string Stem { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string Answer { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public int Score { get; set; }
    public string CategoryTermId { get; set; } = string.Empty;
}
