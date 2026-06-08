using OrchardCore.Exam.Models;

namespace OrchardCore.Exam.ViewModels;

public class ExamAssignmentPartViewModel
{
    public string ExamPaperContentItemId { get; set; } = string.Empty;
    public AssignmentMode AssignmentMode { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int MaxAttempts { get; set; }
    public DisplayMode DisplayMode { get; set; }
}
