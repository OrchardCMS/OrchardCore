using OrchardCore.Exam.Models;

namespace OrchardCore.Exam.ViewModels;

public class QuestionPartViewModel
{
    public QuestionType QuestionType { get; set; }
    public string Stem { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string Analysis { get; set; } = string.Empty;
    public int Score { get; set; }
}
