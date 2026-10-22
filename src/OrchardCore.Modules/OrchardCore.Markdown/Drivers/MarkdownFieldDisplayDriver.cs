using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;
using Shortcodes;

namespace OrchardCore.Markdown.Drivers;

public sealed class MarkdownFieldDisplayDriver : ContentFieldDisplayDriver<MarkdownField>
{
    private readonly IMarkdownDisplayService _markdownDisplayService;

    public MarkdownFieldDisplayDriver(IMarkdownDisplayService markdownDisplayService)
    {
        _markdownDisplayService = markdownDisplayService;
    }

    public override IDisplayResult Display(MarkdownField field, BuildFieldDisplayContext context)
    {
        return Initialize<MarkdownFieldViewModel>(GetDisplayShapeType(context), async model =>
        {
            model.Markdown = field.Markdown;
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;

            var settings = context.PartFieldDefinition.GetSettings<MarkdownFieldSettings>();

            model.Html = await _markdownDisplayService.ToHtmlAsync(
                model.Markdown,
                new Context
                {
                    ["ContentItem"] = field.ContentItem,
                    ["PartFieldDefinition"] = context.PartFieldDefinition,
                },
                settings.SanitizeHtml);
        })
        .Location(OrchardCoreConstants.DisplayType.Detail, "Content")
        .Location(OrchardCoreConstants.DisplayType.Summary, "Content");
    }

    public override IDisplayResult Edit(MarkdownField field, BuildFieldEditorContext context)
    {
        return Initialize<EditMarkdownFieldViewModel>(GetEditorShapeType(context), model =>
        {
            model.Markdown = field.Markdown;
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(MarkdownField field, UpdateFieldEditorContext context)
    {
        var viewModel = new EditMarkdownFieldViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, vm => vm.Markdown);

        field.Markdown = viewModel.Markdown;

        return Edit(field, context);
    }
}
