using OrchardCore.ContentManagement;
using OrchardCore.Exam.Models;

namespace OrchardCore.Exam.Services;

public interface IExamPaperService
{
    Task<ContentItem?> GetPaperAsync(string contentItemId);
    Task<ContentItem> GeneratePaperAsync(string title, List<ExamSectionRule> rules, int duration, DisplayMode displayMode);
}
