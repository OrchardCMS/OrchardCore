using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Exam.Models;
using YesSql;

namespace OrchardCore.Exam.Services;

public class ExamService : IExamService
{
    private readonly IContentManager _contentManager;
    private readonly ISession _session;
    private readonly IQuestionService _questionService;
    private readonly ILogger<ExamService> _logger;
    private static readonly Random _random = new();

    public ExamService(
        IContentManager contentManager,
        ISession session,
        IQuestionService questionService,
        ILogger<ExamService> logger)
    {
        _contentManager = contentManager;
        _session = session;
        _questionService = questionService;
        _logger = logger;
    }

    public async Task<ContentItem> StartExamAsync(string assignmentId, string userId)
    {
        var assignment = await _contentManager.GetAsync(assignmentId);
        if (assignment == null) throw new InvalidOperationException("Exam assignment not found");

        var assignmentPart = assignment.Get<ExamAssignmentPart>(nameof(ExamAssignmentPart));
        if (assignmentPart == null) throw new InvalidOperationException("Invalid exam assignment");

        // Check max attempts
        var existingRecords = await GetRecordsForUserAsync(userId, assignmentId);
        var completedCount = existingRecords.Count(r =>
        {
            var p = r.Get<ExamRecordPart>(nameof(ExamRecordPart));
            return p?.Status == ExamRecordStatus.Submitted || p?.Status == ExamRecordStatus.Graded;
        });
        if (completedCount >= assignmentPart.MaxAttempts)
            throw new InvalidOperationException("Maximum attempts reached");

        var paper = await _contentManager.GetAsync(assignmentPart.ExamPaperContentItemId);
        if (paper == null) throw new InvalidOperationException("Exam paper not found");

        var paperPart = paper.Get<ExamPaperPart>(nameof(ExamPaperPart));

        var record = await _contentManager.NewAsync("ExamRecord");
        var recordPart = record.Get<ExamRecordPart>(nameof(ExamRecordPart));
        recordPart!.ExamAssignmentContentItemId = assignmentId;
        recordPart.ExamPaperContentItemId = assignmentPart.ExamPaperContentItemId;
        recordPart.UserId = userId;
        recordPart.Status = ExamRecordStatus.InProgress;
        recordPart.StartTime = DateTime.UtcNow;
        recordPart.Answers = new List<AnswerEntry>();

        // Copy sections from paper for this exam instance
        if (paperPart?.Sections != null)
        {
            // Deep copy sections to avoid modifying the original
        }

        record.DisplayText = $"Exam Record - {assignment.DisplayText} - {DateTime.UtcNow:yyyy-MM-dd HH:mm}";
        record.Apply(recordPart);

        await _contentManager.CreateAsync(record);
        return record;
    }

    public async Task<ContentItem> StartDrillAsync(string? categoryTermId, QuestionType? questionType, int count, int scorePerQuestion, string userId)
    {
        var questions = await _questionService.GetRandomQuestionsAsync(categoryTermId, questionType, count);
        var questionList = questions.ToList();

        var drillSections = new List<ExamSection>
        {
            new()
            {
                Title = "Drill Questions",
                QuestionIds = questionList.Select(q => q.ContentItemId).ToList(),
                ScorePerQuestion = scorePerQuestion
            }
        };

        var record = await _contentManager.NewAsync("ExamRecord");
        var recordPart = record.Get<ExamRecordPart>(nameof(ExamRecordPart));
        recordPart!.ExamAssignmentContentItemId = null;
        recordPart.ExamPaperContentItemId = null;
        recordPart.UserId = userId;
        recordPart.Status = ExamRecordStatus.InProgress;
        recordPart.StartTime = DateTime.UtcNow;
        recordPart.Answers = new List<AnswerEntry>();
        recordPart.DrillSections = drillSections;

        record.DisplayText = $"Drill - {DateTime.UtcNow:yyyy-MM-dd HH:mm}";
        record.Apply(recordPart);

        await _contentManager.CreateAsync(record);
        return record;
    }

    public async Task SaveAnswerAsync(string recordId, string questionId, string answer)
    {
        var record = await _contentManager.GetAsync(recordId);
        if (record == null) throw new InvalidOperationException("Record not found");

        var part = record.Get<ExamRecordPart>(nameof(ExamRecordPart));
        if (part?.Status != ExamRecordStatus.InProgress)
            throw new InvalidOperationException("Exam is not in progress");

        var existing = part.Answers.FirstOrDefault(a => a.QuestionId == questionId);
        if (existing != null)
        {
            existing.Answer = answer;
        }
        else
        {
            part.Answers.Add(new AnswerEntry { QuestionId = questionId, Answer = answer });
        }

        record.Apply(part);
        await _contentManager.UpdateAsync(record);
    }

    public async Task<ContentItem> SubmitExamAsync(string recordId)
    {
        var record = await _contentManager.GetAsync(recordId);
        if (record == null) throw new InvalidOperationException("Record not found");

        var part = record.Get<ExamRecordPart>(nameof(ExamRecordPart));
        if (part?.Status != ExamRecordStatus.InProgress)
            throw new InvalidOperationException("Exam is not in progress");

        part.Status = ExamRecordStatus.Submitted;
        part.SubmitTime = DateTime.UtcNow;

        record.Apply(part);
        await _contentManager.UpdateAsync(record);

        // Auto-grade objective questions
        await GradeObjectiveAsync(recordId);

        return (await _contentManager.GetAsync(recordId))!;
    }

    public async Task AutoSubmitExpiredAsync()
    {
        // Find all in-progress records with expired assignments
        var records = await _session.Query<ContentItem, ContentItemIndex>()
            .Where(x => x.ContentType == "ExamRecord" && x.Published)
            .ListAsync();

        foreach (var record in records)
        {
            var part = record.Get<ExamRecordPart>(nameof(ExamRecordPart));
            if (part?.Status != ExamRecordStatus.InProgress) continue;
            if (part.ExamAssignmentContentItemId == null) continue;

            var assignment = await _contentManager.GetAsync(part.ExamAssignmentContentItemId);
            if (assignment == null) continue;

            var assignmentPart = assignment.Get<ExamAssignmentPart>(nameof(ExamAssignmentPart));
            if (assignmentPart?.EndTime.HasValue == true && DateTime.UtcNow > assignmentPart.EndTime.Value)
            {
                await SubmitExamAsync(record.ContentItemId);
            }
        }
    }

    public async Task GradeObjectiveAsync(string recordId)
    {
        var record = await _contentManager.GetAsync(recordId);
        if (record == null) return;

        var part = record.Get<ExamRecordPart>(nameof(ExamRecordPart));
        if (part == null) return;

        int objectiveScore = 0;
        bool hasSubjective = false;

        // Get all question IDs from paper or drill sections
        var allQuestionIds = new List<string>();
        var paperPart = part.ExamPaperContentItemId != null
            ? (await _contentManager.GetAsync(part.ExamPaperContentItemId))?.Get<ExamPaperPart>(nameof(ExamPaperPart))
            : null;

        if (paperPart?.Sections != null)
        {
            foreach (var section in paperPart.Sections)
            {
                allQuestionIds.AddRange(section.QuestionIds);
            }
        }
        else if (part.DrillSections != null)
        {
            foreach (var section in part.DrillSections)
            {
                allQuestionIds.AddRange(section.QuestionIds);
            }
        }

        foreach (var qId in allQuestionIds)
        {
            var question = await _contentManager.GetAsync(qId);
            if (question == null) continue;
            var qPart = question.Get<QuestionPart>(nameof(QuestionPart));
            if (qPart == null) continue;

            if (qPart.QuestionType == QuestionType.FillBlank || qPart.QuestionType == QuestionType.ShortAnswer)
            {
                hasSubjective = true;
                continue;
            }

            var answer = part.Answers.FirstOrDefault(a => a.QuestionId == qId);
            if (answer == null) continue;

            // For SingleChoice and TrueFalse: exact match
            // For MultipleChoice: all correct and no wrong
            bool correct = false;
            if (qPart.QuestionType == QuestionType.MultipleChoice)
            {
                var correctAnswers = qPart.Answer.Split(',').Select(a => a.Trim()).OrderBy(a => a).ToList();
                var userAnswers = answer.Answer.Split(',').Select(a => a.Trim()).OrderBy(a => a).ToList();
                correct = correctAnswers.SequenceEqual(userAnswers);
            }
            else
            {
                correct = string.Equals(answer.Answer?.Trim(), qPart.Answer?.Trim(), StringComparison.OrdinalIgnoreCase);
            }

            if (correct)
            {
                // Find the score for this question from sections
                int score = GetQuestionScore(paperPart, part.DrillSections, qId);
                objectiveScore += score;
                answer.Score = score;
            }
            else
            {
                answer.Score = 0;
            }
        }

        part.ObjectiveScore = objectiveScore;
        if (!hasSubjective)
        {
            part.SubjectiveScore = 0;
            part.Status = ExamRecordStatus.Graded;
        }

        record.Apply(part);
        await _contentManager.UpdateAsync(record);
    }

    private static int GetQuestionScore(ExamPaperPart? paperPart, List<ExamSection>? drillSections, string questionId)
    {
        var sections = paperPart?.Sections ?? drillSections ?? [];
        foreach (var section in sections)
        {
            if (section.QuestionIds.Contains(questionId))
                return section.ScorePerQuestion;
        }
        return 0;
    }

    public async Task GradeSubjectiveAsync(string recordId, List<GradeEntry> grades)
    {
        var record = await _contentManager.GetAsync(recordId);
        if (record == null) return;

        var part = record.Get<ExamRecordPart>(nameof(ExamRecordPart));
        if (part == null) return;

        int subjectiveScore = 0;
        foreach (var grade in grades)
        {
            var answer = part.Answers.FirstOrDefault(a => a.QuestionId == grade.QuestionId);
            if (answer != null)
            {
                answer.Score = grade.Score;
                answer.Comment = grade.Comment;
                subjectiveScore += grade.Score;
            }
        }

        part.SubjectiveScore = subjectiveScore;
        part.Status = ExamRecordStatus.Graded;

        record.Apply(part);
        await _contentManager.UpdateAsync(record);
    }

    public async Task<ContentItem?> GetRecordAsync(string recordId)
    {
        return await _contentManager.GetAsync(recordId);
    }

    public async Task<IEnumerable<ContentItem>> GetRecordsAsync(string assignmentId)
    {
        var records = await _session.Query<ContentItem, ContentItemIndex>()
            .Where(x => x.ContentType == "ExamRecord" && x.Published)
            .ListAsync();

        return records.Where(r =>
        {
            var part = r.Get<ExamRecordPart>(nameof(ExamRecordPart));
            return part?.ExamAssignmentContentItemId == assignmentId;
        });
    }

    public async Task<IEnumerable<ContentItem>> GetRecordsForUserAsync(string userId, string? assignmentId = null)
    {
        var records = await _session.Query<ContentItem, ContentItemIndex>()
            .Where(x => x.ContentType == "ExamRecord" && x.Published)
            .ListAsync();

        return records.Where(r =>
        {
            var part = r.Get<ExamRecordPart>(nameof(ExamRecordPart));
            if (part?.UserId != userId) return false;
            if (assignmentId != null && part.ExamAssignmentContentItemId != assignmentId) return false;
            return true;
        });
    }
}
