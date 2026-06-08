namespace OrchardCore.Exam.Models;

public class ExamSection
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> QuestionIds { get; set; } = [];
    public int ScorePerQuestion { get; set; }
    public int TotalScore => QuestionIds.Count * ScorePerQuestion;
}

public class ExamSectionRule
{
    public string CategoryTermId { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public int Count { get; set; }
    public int ScorePerQuestion { get; set; }
}
