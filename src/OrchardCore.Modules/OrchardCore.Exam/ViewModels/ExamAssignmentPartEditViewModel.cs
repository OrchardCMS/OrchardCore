using OrchardCore.Exam.Models;

namespace OrchardCore.Exam.ViewModels;

public class ExamAssignmentPartEditViewModel
{
    public string ExamPaperContentItemId { get; set; } = string.Empty;
    public AssignmentMode AssignmentMode { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int MaxAttempts { get; set; } = 1;
    public DisplayMode DisplayMode { get; set; }
    public List<string> AllowedRoleIds { get; set; } = new();
}
