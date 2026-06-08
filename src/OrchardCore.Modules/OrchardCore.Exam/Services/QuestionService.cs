using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Queryable;
using OrchardCore.Exam.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Exam.Services;

public class QuestionService : IQuestionService
{
    private readonly ISession _session;
    private readonly IContentManager _contentManager;
    private static readonly Random _random = new();

    public QuestionService(ISession session, IContentManager contentManager)
    {
        _session = session;
        _contentManager = contentManager;
    }

    public async Task<IEnumerable<ContentItem>> GetQuestionsAsync(string? categoryTermId = null, QuestionType? questionType = null)
    {
        var query = _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == "Question" && x.Published);

        var items = await query.ListAsync();
        var result = new List<ContentItem>();

        foreach (var item in items)
        {
            var part = item.Get<QuestionPart>(nameof(QuestionPart));
            if (part == null) continue;

            if (categoryTermId != null && part.CategoryTermId != categoryTermId) continue;
            if (questionType.HasValue && part.QuestionType != questionType.Value) continue;

            result.Add(item);
        }

        return result;
    }

    public async Task<IEnumerable<ContentItem>> GetRandomQuestionsAsync(string? categoryTermId, QuestionType? questionType, int count)
    {
        var questions = (await GetQuestionsAsync(categoryTermId, questionType)).ToList();

        var shuffled = questions.OrderBy(_ => _random.Next()).Take(count).ToList();
        return shuffled;
    }

    public async Task<ContentItem?> GetQuestionAsync(string contentItemId)
    {
        return await _contentManager.GetAsync(contentItemId);
    }
}
