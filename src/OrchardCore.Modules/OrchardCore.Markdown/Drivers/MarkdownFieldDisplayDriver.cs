using System.Text.Encodings.Web;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Markdown.Drivers;

public sealed class MarkdownFieldDisplayDriver : ContentFieldDisplayDriver<MarkdownField>
{
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    private readonly IShortcodeService _shortcodeService;
    private readonly IMarkdownService _markdownService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public MarkdownFieldDisplayDriver(ILiquidTemplateManager liquidTemplateManager,
        HtmlEncoder htmlEncoder,
        IHtmlSanitizerService htmlSanitizerService,
        IShortcodeService shortcodeService,
        IMarkdownService markdownService,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<MarkdownFieldDisplayDriver> localizer)
    {
        _liquidTemplateManager = liquidTemplateManager;
        _htmlEncoder = htmlEncoder;
        _htmlSanitizerService = htmlSanitizerService;
        _shortcodeService = shortcodeService;
        _markdownService = markdownService;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        S = localizer;
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

            if (settings.RenderLiquid)
            {
                model.Markdown = await _liquidTemplateManager.RenderStringAsync(model.Markdown, _htmlEncoder, model,
                    new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(field.ContentItem) });
            }

            // The default Markdown option is to entity escape html
            // so filters must be run after the markdown has been processed.
            model.Html = _markdownService.ToHtml(model.Markdown ?? string.Empty);

            model.Html = await _shortcodeService.ProcessAsync(model.Html,
                new Context
                {
                    ["ContentItem"] = field.ContentItem,
                    ["PartFieldDefinition"] = context.PartFieldDefinition,
                });

            if (settings.SanitizeHtml)
            {
                model.Html = _htmlSanitizerService.Sanitize(model.Html ?? string.Empty);
            }
        })
        .Location(OrchardCoreConstants.DisplayType.Detail, "Content")
        .Location(OrchardCoreConstants.DisplayType.Summary, "Content");
    }

    public override async Task<IDisplayResult> EditAsync(MarkdownField field, BuildFieldEditorContext context)
    {
        var settings = context.PartFieldDefinition.GetSettings<MarkdownFieldSettings>();

        if (settings.RenderLiquid && !await CanManageLiquidTemplatesAsync())
        {
            return null;
        }

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
        var settings = context.PartFieldDefinition.GetSettings<MarkdownFieldSettings>();

        if (settings.RenderLiquid && !await CanManageLiquidTemplatesAsync())
        {
            context.Updater.ModelState.AddModelError(
                Prefix,
                S["You do not have permission to edit Liquid templates."]);

            return null;
        }

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, vm => vm.Markdown);

        if (settings.RenderLiquid
            && !string.IsNullOrEmpty(viewModel.Markdown)
            && !_liquidTemplateManager.Validate(viewModel.Markdown, out var errors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Markdown),
                S["{0} contains invalid Liquid expression: {1}",
                    context.PartFieldDefinition.DisplayName(),
                    string.Join(" ", errors)]);
        }
        else
        {
            field.Markdown = viewModel.Markdown;
        }

        return await EditAsync(field, context);
    }

    private Task<bool> CanManageLiquidTemplatesAsync() =>
        _authorizationService.AuthorizeAsync(
            _httpContextAccessor.HttpContext?.User,
            Permissions.ManageLiquidTemplates);
}
