using OrchardCore.ContentManagement;
using OrchardCore.Exam.Models;
using System.Linq;

namespace OrchardCore.Exam.Services;

public class ExamPaperService : IExamPaperService
{
    private readonly IContentManager _contentManager;
    private readonly IQuestionService _questionService;

    public ExamPaperService(IContentManager contentManager, IQuestionService questionService)
    {
        _contentManager = contentManager;
        _questionService = questionService;
    }

    public async Task<ContentItem?> GetPaperAsync(string contentItemId)
    {
        return await _contentManager.GetAsync(contentItemId);
    }

    public async Task<ContentItem> GeneratePaperAsync(string title, List<ExamSectionRule> rules, int duration, DisplayMode displayMode)
    {
        var sections = new List<ExamSection>();

        foreach (var rule in rules)
        {
            var questions = await _questionService.GetRandomQuestionsAsync(
                rule.CategoryTermId, rule.QuestionType, rule.Count);

            var section = new ExamSection
            {
                Title = rule.QuestionType.ToString(),
                QuestionIds = questions.Select(q => q.ContentItemId).ToList(),
                ScorePerQuestion = rule.ScorePerQuestion
            };
            sections.Add(section);
        }

        var contentItem = await _contentManager.NewAsync("ExamPaper");
        var part = contentItem.Get<ExamPaperPart>(nameof(ExamPaperPart));
        part!.GenerationMode = GenerationMode.ByRule;
        part.Sections = sections;
        part.SectionRules = rules;
        part.TotalScore = sections.Sum(s => s.TotalScore);
        part.Duration = duration;
        part.DisplayMode = displayMode;

        contentItem.DisplayText = title;
        contentItem.Apply(part);

        await _contentManager.CreateAsync(contentItem);
        return contentItem;
    }
}
