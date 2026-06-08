using OrchardCore.ContentManagement;

namespace OrchardCore.Exam.Models;

public class QuestionPart : ContentPart
{
    public QuestionType QuestionType { get; set; }
    public string Stem { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public string Answer { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public int Score { get; set; }
    public string CategoryTermId { get; set; } = string.Empty;
}
