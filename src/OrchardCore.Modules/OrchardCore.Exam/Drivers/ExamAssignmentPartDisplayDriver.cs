using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Exam.Models;
using OrchardCore.Exam.ViewModels;

namespace OrchardCore.Exam.Drivers;

public class ExamAssignmentPartDisplayDriver : ContentPartDisplayDriver<ExamAssignmentPart>
{
    public override IDisplayResult Display(ExamAssignmentPart part, BuildPartDisplayContext context)
    {
        return Initialize<ExamAssignmentPartViewModel>(GetDisplayShapeType(context), m =>
        {
            m.ExamPaperContentItemId = part.ExamPaperContentItemId;
            m.AssignmentMode = part.AssignmentMode;
            m.StartTime = part.StartTime;
            m.EndTime = part.EndTime;
            m.MaxAttempts = part.MaxAttempts;
            m.DisplayMode = part.DisplayMode;
        })
        .Location("Detail", "Content:5");
    }

    public override IDisplayResult Edit(ExamAssignmentPart part, BuildPartEditorContext context)
    {
        return Initialize<ExamAssignmentPartEditViewModel>(GetEditorShapeType(context), m =>
        {
            m.ExamPaperContentItemId = part.ExamPaperContentItemId;
            m.AssignmentMode = part.AssignmentMode;
            m.StartTime = part.StartTime;
            m.EndTime = part.EndTime;
            m.MaxAttempts = part.MaxAttempts;
            m.DisplayMode = part.DisplayMode;
            m.AllowedRoleIds = part.AllowedRoleIds;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(ExamAssignmentPart part, UpdatePartEditorContext context)
    {
        var viewModel = new ExamAssignmentPartEditViewModel();
        await context.Updater.UpdateModelAsync(viewModel, Prefix);

        part.ExamPaperContentItemId = viewModel.ExamPaperContentItemId ?? string.Empty;
        part.AssignmentMode = viewModel.AssignmentMode;
        part.StartTime = viewModel.StartTime;
        part.EndTime = viewModel.EndTime;
        part.MaxAttempts = viewModel.MaxAttempts;
        part.DisplayMode = viewModel.DisplayMode;
        part.AllowedRoleIds = viewModel.AllowedRoleIds ?? new List<string>();

        return Edit(part, context);
    }
}
