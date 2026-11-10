using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;
using Shortcodes;

namespace OrchardCore.Markdown.Drivers;

public sealed class MarkdownBodyPartDisplayDriver : ContentPartDisplayDriver<MarkdownBodyPart>
{
    private readonly IMarkdownDisplayService _markdownDisplayService;

    public MarkdownBodyPartDisplayDriver(IMarkdownDisplayService markdownDisplayService)
    {
        _markdownDisplayService = markdownDisplayService;
    }

    public override IDisplayResult Display(MarkdownBodyPart markdownBodyPart, BuildPartDisplayContext context)
    {
        return Initialize<MarkdownBodyPartViewModel, MarkdownBodyPartDisplayDriver, MarkdownBodyPart, BuildPartDisplayContext>(GetDisplayShapeType(context), static (m, driver, part, context) => driver.BuildViewModel(m, part, context), this, markdownBodyPart, context)
            .Location(OrchardCoreConstants.DisplayType.Detail, "Content")
            .Location(OrchardCoreConstants.DisplayType.Summary, "Content");
    }

    public override IDisplayResult Edit(MarkdownBodyPart markdownBodyPart, BuildPartEditorContext context)
    {
        return Initialize<MarkdownBodyPartViewModel>(GetEditorShapeType(context), model =>
        {
            model.Markdown = markdownBodyPart.Markdown;
            model.ContentItem = markdownBodyPart.ContentItem;
            model.MarkdownBodyPart = markdownBodyPart;
            model.TypePartDefinition = context.TypePartDefinition;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(MarkdownBodyPart model, UpdatePartEditorContext context)
    {
        var viewModel = new MarkdownBodyPartViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, vm => vm.Markdown);

        model.Markdown = viewModel.Markdown;

        return Edit(model, context);
    }

    private async ValueTask BuildViewModel(MarkdownBodyPartViewModel model, MarkdownBodyPart markdownBodyPart, BuildPartDisplayContext context)
    {
        model.Markdown = markdownBodyPart.Markdown;
        model.MarkdownBodyPart = markdownBodyPart;
        model.ContentItem = markdownBodyPart.ContentItem;

        var settings = context.TypePartDefinition.GetSettings<MarkdownBodyPartSettings>();

        model.Html = await _markdownDisplayService.ToHtmlAsync(
            model.Markdown,
            new Context
            {
                ["ContentItem"] = markdownBodyPart.ContentItem,
                ["TypePartDefinition"] = context.TypePartDefinition,
            },
            settings.SanitizeHtml);
    }
}
