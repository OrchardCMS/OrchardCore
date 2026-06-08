using OrchardCore.Exam.Models;

namespace OrchardCore.Exam.ViewModels;

public class ExamPaperPartEditViewModel
{
    public GenerationMode GenerationMode { get; set; }
    public string SectionsJson { get; set; } = "[]";
    public string SectionRulesJson { get; set; } = "[]";
    public int TotalScore { get; set; }
    public int Duration { get; set; }
    public DisplayMode DisplayMode { get; set; }
}
