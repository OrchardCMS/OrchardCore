using OrchardCore.ContentManagement;

namespace OrchardCore.Exam.Models;

public class ExamRecordPart : ContentPart
{
    public string? ExamAssignmentContentItemId { get; set; }
    public string? ExamPaperContentItemId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ExamRecordStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? SubmitTime { get; set; }
    public List<AnswerEntry> Answers { get; set; } = [];
    public int ObjectiveScore { get; set; }
    public int? SubjectiveScore { get; set; }
    public int TotalScore => SubjectiveScore.HasValue ? ObjectiveScore + SubjectiveScore.Value : ObjectiveScore;
    public List<ExamSection>? DrillSections { get; set; }
}

public class AnswerEntry
{
    public string QuestionId { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string? Comment { get; set; }
}
