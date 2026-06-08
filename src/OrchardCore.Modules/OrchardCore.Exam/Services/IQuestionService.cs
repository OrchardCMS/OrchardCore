using OrchardCore.ContentManagement;
using OrchardCore.Exam.Models;

namespace OrchardCore.Exam.Services;

public interface IQuestionService
{
    Task<IEnumerable<ContentItem>> GetQuestionsAsync(string? categoryTermId = null, QuestionType? questionType = null);
    Task<IEnumerable<ContentItem>> GetRandomQuestionsAsync(string? categoryTermId, QuestionType? questionType, int count);
    Task<ContentItem?> GetQuestionAsync(string contentItemId);
}
