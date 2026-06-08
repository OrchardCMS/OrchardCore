using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Exam.Models;
using OrchardCore.Exam.ViewModels;

namespace OrchardCore.Exam.Drivers;

public class ExamPaperPartDisplayDriver : ContentPartDisplayDriver<ExamPaperPart>
{
    public override IDisplayResult Display(ExamPaperPart part, BuildPartDisplayContext context)
    {
        return Initialize<ExamPaperPartViewModel>(GetDisplayShapeType(context), m =>
        {
            m.GenerationMode = part.GenerationMode;
            m.Sections = part.Sections;
            m.TotalScore = part.TotalScore;
            m.Duration = part.Duration;
            m.DisplayMode = part.DisplayMode;
        })
        .Location("Detail", "Content:5")
        .Location("Summary", "Content:5");
    }

    public override IDisplayResult Edit(ExamPaperPart part, BuildPartEditorContext context)
    {
        return Initialize<ExamPaperPartEditViewModel>(GetEditorShapeType(context), m =>
        {
            m.GenerationMode = part.GenerationMode;
            m.SectionsJson = part.Sections != null && part.Sections.Count > 0
                ? System.Text.Json.JsonSerializer.Serialize(part.Sections) : "[]";
            m.SectionRulesJson = part.SectionRules != null && part.SectionRules.Count > 0
                ? System.Text.Json.JsonSerializer.Serialize(part.SectionRules) : "[]";
            m.TotalScore = part.TotalScore;
            m.Duration = part.Duration;
            m.DisplayMode = part.DisplayMode;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(ExamPaperPart part, UpdatePartEditorContext context)
    {
        var viewModel = new ExamPaperPartEditViewModel();
        await context.Updater.UpdateModelAsync(viewModel, Prefix);

        part.GenerationMode = viewModel.GenerationMode;
        part.Duration = viewModel.Duration;
        part.DisplayMode = viewModel.DisplayMode;
        part.TotalScore = viewModel.TotalScore;

        if (!string.IsNullOrEmpty(viewModel.SectionsJson))
        {
            try { part.Sections = System.Text.Json.JsonSerializer.Deserialize<List<ExamSection>>(viewModel.SectionsJson) ?? new List<ExamSection>(); }
            catch { part.Sections = new List<ExamSection>(); }
        }

        if (!string.IsNullOrEmpty(viewModel.SectionRulesJson))
        {
            try { part.SectionRules = System.Text.Json.JsonSerializer.Deserialize<List<ExamSectionRule>>(viewModel.SectionRulesJson) ?? new List<ExamSectionRule>(); }
            catch { part.SectionRules = new List<ExamSectionRule>(); }
        }

        return Edit(part, context);
    }
}
