using OrchardCore.Exam.Models;

namespace OrchardCore.Exam.ViewModels;

public class ExamPaperPartViewModel
{
    public GenerationMode GenerationMode { get; set; }
    public List<ExamSection> Sections { get; set; } = new();
    public int TotalScore { get; set; }
    public int Duration { get; set; }
    public DisplayMode DisplayMode { get; set; }
}
