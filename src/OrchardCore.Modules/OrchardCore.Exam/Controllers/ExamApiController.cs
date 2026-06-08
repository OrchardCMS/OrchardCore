using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Exam.Models;
using OrchardCore.Exam.Services;

namespace OrchardCore.Exam.Controllers;

[Route("api/exam")]
[ApiController]
[Authorize]
public class ExamApiController : ControllerBase
{
    private readonly IExamService _examService;
    private readonly IContentManager _contentManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHtmlLocalizer<ExamApiController> H;

    public ExamApiController(
        IExamService examService,
        IContentManager contentManager,
        IAuthorizationService authorizationService,
        IHtmlLocalizer<ExamApiController> htmlLocalizer)
    {
        _examService = examService;
        _contentManager = contentManager;
        _authorizationService = authorizationService;
        H = htmlLocalizer;
    }

    private async Task<bool> AuthorizeExamTakeAsync()
    {
        return (await _authorizationService.AuthorizeAsync(User, Permissions.TakeExam)).Succeeded;
    }

    [HttpPost("start/{assignmentId}")]
    public async Task<IActionResult> StartExam(string assignmentId)
    {
        if (!await AuthorizeExamTakeAsync()) return Forbid();

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var record = await _examService.StartExamAsync(assignmentId, userId);
            var part = record.Get<ExamRecordPart>(nameof(ExamRecordPart));
            return Ok(new { recordId = record.ContentItemId, status = part?.Status.ToString() });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("drill")]
    public async Task<IActionResult> StartDrill([FromBody] DrillStartViewModel model)
    {
        if (!await AuthorizeExamTakeAsync()) return Forbid();

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var record = await _examService.StartDrillAsync(
                model.CategoryTermId, model.QuestionType, model.Count, model.ScorePerQuestion, userId);
            var part = record.Get<ExamRecordPart>(nameof(ExamRecordPart));
            return Ok(new { recordId = record.ContentItemId, status = part?.Status.ToString() });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveAnswer([FromBody] SaveAnswerRequest model)
    {
        if (!await AuthorizeExamTakeAsync()) return Forbid();

        try
        {
            await _examService.SaveAnswerAsync(model.RecordId, model.QuestionId, model.Answer);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("submit/{recordId}")]
    public async Task<IActionResult> SubmitExam(string recordId)
    {
        if (!await AuthorizeExamTakeAsync()) return Forbid();

        try
        {
            var record = await _examService.SubmitExamAsync(recordId);
            var part = record.Get<ExamRecordPart>(nameof(ExamRecordPart));
            return Ok(new { recordId, status = part?.Status.ToString(), totalScore = part?.TotalScore });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("paper/{recordId}")]
    public async Task<IActionResult> GetPaper(string recordId)
    {
        if (!await AuthorizeExamTakeAsync()) return Forbid();

        var record = await _examService.GetRecordAsync(recordId);
        if (record == null) return NotFound();

        var part = record.Get<ExamRecordPart>(nameof(ExamRecordPart));
        if (part == null) return NotFound();

        var sections = new List<object>();

        // Get sections from paper or drill
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
                var questions = new List<object>();
                foreach (var qId in section.QuestionIds)
                {
                    var question = await _contentManager.GetAsync(qId);
                    if (question == null) continue;
                    var qPart = question.Get<QuestionPart>(nameof(QuestionPart));
                    if (qPart == null) continue;

                    var existingAnswer = part.Answers.FirstOrDefault(a => a.QuestionId == qId);

                    questions.Add(new
                    {
                        id = qId,
                        type = qPart.QuestionType.ToString(),
                        stem = qPart.Stem,
                        options = qPart.Options,
                        score = section.ScorePerQuestion,
                        answer = existingAnswer?.Answer ?? string.Empty
                    });
                }

                sections.Add(new { title = section.Title, questions });
            }
        }

        return Ok(new
        {
            recordId,
            status = part.Status.ToString(),
            mode = part.ExamAssignmentContentItemId == null ? "drill" : "exam",
            displayMode = paperPart?.DisplayMode.ToString() ?? DisplayMode.OneByOne.ToString(),
            sections
        });
    }
}

public class SaveAnswerRequest
{
    public string RecordId { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public class DrillStartViewModel
{
    public string? CategoryTermId { get; set; }
    public QuestionType? QuestionType { get; set; }
    public int Count { get; set; } = 10;
    public int ScorePerQuestion { get; set; } = 1;
}
