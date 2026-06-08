using OrchardCore.ContentManagement;
using OrchardCore.Exam.Models;

namespace OrchardCore.Exam.Services;

public interface IExamService
{
    Task<ContentItem> StartExamAsync(string assignmentId, string userId);
    Task<ContentItem> StartDrillAsync(string? categoryTermId, QuestionType? questionType, int count, int scorePerQuestion, string userId);
    Task SaveAnswerAsync(string recordId, string questionId, string answer);
    Task<ContentItem> SubmitExamAsync(string recordId);
    Task AutoSubmitExpiredAsync();
    Task GradeObjectiveAsync(string recordId);
    Task GradeSubjectiveAsync(string recordId, List<GradeEntry> grades);
    Task<ContentItem?> GetRecordAsync(string recordId);
    Task<IEnumerable<ContentItem>> GetRecordsAsync(string assignmentId);
    Task<IEnumerable<ContentItem>> GetRecordsForUserAsync(string userId, string? assignmentId = null);
}

public class GradeEntry
{
    public string QuestionId { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Comment { get; set; } = string.Empty;
}
