using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Exam.Models;
using OrchardCore.Exam.Services;
using YesSql;

namespace OrchardCore.Exam.Controllers;

public class AdminExamController : Controller
{
    private readonly IExamService _examService;
    private readonly IContentManager _contentManager;
    private readonly ISession _session;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHtmlLocalizer<AdminExamController> H;

    public AdminExamController(
        IExamService examService,
        IContentManager contentManager,
        ISession session,
        IAuthorizationService authorizationService,
        IHtmlLocalizer<AdminExamController> htmlLocalizer)
    {
        _examService = examService;
        _contentManager = contentManager;
        _session = session;
        _authorizationService = authorizationService;
        H = htmlLocalizer;
    }

    [HttpGet("/admin/exam/records/{assignmentId}")]
    [Authorize]
    public async Task<IActionResult> Records(string assignmentId)
    {
        if (!(await _authorizationService.AuthorizeAsync(User, Permissions.ManageExams)).Succeeded)
            return Forbid();

        var records = await _examService.GetRecordsAsync(assignmentId);
        return View(records);
    }

    [HttpGet("/admin/exam/grade/{recordId}")]
    [Authorize]
    public async Task<IActionResult> GradeSubjective(string recordId)
    {
        if (!(await _authorizationService.AuthorizeAsync(User, Permissions.ManageExams)).Succeeded)
            return Forbid();

        var record = await _examService.GetRecordAsync(recordId);
        if (record == null) return NotFound();

        var part = record.Get<ExamRecordPart>(nameof(ExamRecordPart));
        if (part == null) return NotFound();

        // Get all question IDs from paper or drill sections
        var allQuestionIds = new List<string>();
        ExamPaperPart? paperPart = null;
        if (part.ExamPaperContentItemId != null)
        {
            var paper = await _contentManager.GetAsync(part.ExamPaperContentItemId);
            paperPart = paper?.Get<ExamPaperPart>(nameof(ExamPaperPart));
        }

        var sectionSource = paperPart?.Sections ?? part.DrillSections;
        if (sectionSource != null)
        {
            foreach (var section in sectionSource)
            {
                allQuestionIds.AddRange(section.QuestionIds);
            }
        }

        // Get only subjective questions
        var subjectiveQuestions = new List<ContentItem>();
        foreach (var qId in allQuestionIds)
        {
            var question = await _contentManager.GetAsync(qId);
            if (question == null) continue;
            var qPart = question.Get<QuestionPart>(nameof(QuestionPart));
            if (qPart?.QuestionType == QuestionType.FillBlank || qPart?.QuestionType == QuestionType.ShortAnswer)
            {
                subjectiveQuestions.Add(question);
            }
        }

        ViewBag.RecordId = recordId;
        ViewBag.Record = record;
        return View(subjectiveQuestions);
    }

    [HttpPost("/admin/exam/grade/{recordId}")]
    [Authorize]
    public async Task<IActionResult> GradeSubjective(string recordId, List<GradeEntry> grades)
    {
        if (!(await _authorizationService.AuthorizeAsync(User, Permissions.ManageExams)).Succeeded)
            return Forbid();

        await _examService.GradeSubjectiveAsync(recordId, grades);
        return RedirectToAction(nameof(Records), new { assignmentId = "" });
    }
}
