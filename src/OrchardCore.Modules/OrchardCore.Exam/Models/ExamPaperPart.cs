using OrchardCore.ContentManagement;

namespace OrchardCore.Exam.Models;

public class ExamPaperPart : ContentPart
{
    public GenerationMode GenerationMode { get; set; }
    public List<ExamSection> Sections { get; set; } = [];
    public List<ExamSectionRule> SectionRules { get; set; } = [];
    public int TotalScore { get; set; }
    public int Duration { get; set; } // minutes
    public DisplayMode DisplayMode { get; set; }
    public bool HasSubjective => Sections.Any(s => s.QuestionIds.Any());
}
