using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Exam.Models;

namespace OrchardCore.Exam.Drivers;

public class ExamRecordPartDisplayDriver : ContentPartDisplayDriver<ExamRecordPart>
{
    public override IDisplayResult Display(ExamRecordPart part, BuildPartDisplayContext context)
    {
        return Initialize<ExamRecordPartViewModel>(GetDisplayShapeType(context), m =>
        {
            m.Status = part.Status;
            m.StartTime = part.StartTime;
            m.SubmitTime = part.SubmitTime;
            m.ObjectiveScore = part.ObjectiveScore;
            m.SubjectiveScore = part.SubjectiveScore;
            m.TotalScore = part.TotalScore;
            m.Answers = part.Answers;
        })
        .Location("Detail", "Content:5");
    }
}

public class ExamRecordPartViewModel
{
    public ExamRecordStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? SubmitTime { get; set; }
    public int ObjectiveScore { get; set; }
    public int? SubjectiveScore { get; set; }
    public int TotalScore { get; set; }
    public List<AnswerEntry> Answers { get; set; } = new();
}
