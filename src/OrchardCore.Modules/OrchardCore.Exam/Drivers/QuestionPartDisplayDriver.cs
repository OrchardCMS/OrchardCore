using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Exam.Models;
using OrchardCore.Exam.ViewModels;

namespace OrchardCore.Exam.Drivers;

public class QuestionPartDisplayDriver : ContentPartDisplayDriver<QuestionPart>
{
    public override IDisplayResult Display(QuestionPart part, BuildPartDisplayContext context)
    {
        return Initialize<QuestionPartViewModel>(GetDisplayShapeType(context), m =>
        {
            m.QuestionType = part.QuestionType;
            m.Stem = part.Stem;
            m.Options = part.Options;
            m.Analysis = part.Analysis;
            m.Score = part.Score;
        })
        .Location("Detail", "Content:5")
        .Location("Summary", "Content:5");
    }

    public override IDisplayResult Edit(QuestionPart part, BuildPartEditorContext context)
    {
        return Initialize<QuestionPartEditViewModel>(GetEditorShapeType(context), m =>
        {
            m.QuestionType = part.QuestionType;
            m.Stem = part.Stem;
            m.Options = part.Options;
            m.Answer = part.Answer;
            m.Analysis = part.Analysis;
            m.Score = part.Score;
            m.CategoryTermId = part.CategoryTermId;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(QuestionPart part, UpdatePartEditorContext context)
    {
        var viewModel = new QuestionPartEditViewModel();
        await context.Updater.UpdateModelAsync(viewModel, Prefix);

        part.QuestionType = viewModel.QuestionType;
        part.Stem = viewModel.Stem;
        part.Options = viewModel.Options ?? new List<string>();
        part.Answer = viewModel.Answer ?? string.Empty;
        part.Analysis = viewModel.Analysis ?? string.Empty;
        part.Score = viewModel.Score;
        part.CategoryTermId = viewModel.CategoryTermId ?? string.Empty;

        return Edit(part, context);
    }
}
