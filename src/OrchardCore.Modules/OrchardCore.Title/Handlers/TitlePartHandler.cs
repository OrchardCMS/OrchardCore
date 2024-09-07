using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Liquid;
using OrchardCore.Title.Models;
using OrchardCore.Title.ViewModels;

namespace OrchardCore.Title.Handlers;

public class TitlePartHandler : ContentPartHandler<TitlePart>
{
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    protected readonly IStringLocalizer S;
    private readonly HashSet<ContentItem> _contentItems = [];

    public TitlePartHandler(
        ILiquidTemplateManager liquidTemplateManager,
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<TitlePartHandler> stringLocalizer)
    {
        _liquidTemplateManager = liquidTemplateManager;
        _contentDefinitionManager = contentDefinitionManager;
        S = stringLocalizer;
    }

    public override Task UpdatedAsync(UpdateContentContext context, TitlePart part)
    {
        return SetTitleAsync(part);
    }

    public override Task CreatedAsync(CreateContentContext context, TitlePart part)
    {
        return SetTitleAsync(part);
    }

    protected override Task ValidatingAsync(ValidateContentPartContext context, TitlePart part)
    {
        var settings = context.ContentTypePartDefinition.GetSettings<TitlePartSettings>();

        if (settings.Options == TitlePartOptions.EditableRequired && string.IsNullOrEmpty(part.Title))
        {
            context.Fail(S["A value is required for Title."], nameof(part.Title));
        }

        return Task.CompletedTask;
    }

    private async Task SetTitleAsync(TitlePart part)
    {
        if (!_contentItems.Add(part.ContentItem))
        {
            // At this point we know that the contentItem was already processed. No need to process it again.

            return;
        }

        var settings = await GetSettingsAsync(part);

        // Do not compute the title if the user can modify it.
        if (settings.Options == TitlePartOptions.Editable || settings.Options == TitlePartOptions.EditableRequired)
        {
            if (string.IsNullOrWhiteSpace(part.ContentItem.DisplayText))
            {
                // UpdatedAsync event is called from non-UI request like API, we update the DisplayText if it is not already set.
                // When the displayText is not set, we set it to the value of title.
                part.ContentItem.DisplayText = part.Title;
            }

            return;
        }

        if (!string.IsNullOrEmpty(settings.Pattern))
        {
            var model = new TitlePartViewModel()
            {
                Title = part.Title,
                TitlePart = part,
                ContentItem = part.ContentItem,
            };

            var title = await _liquidTemplateManager.RenderStringAsync(settings.Pattern, NullEncoder.Default, model,
                new Dictionary<string, FluidValue>()
                {
                    ["ContentItem"] = new ObjectValue(model.ContentItem)
                });

            title = title.Replace("\r", string.Empty).Replace("\n", string.Empty);

            part.Title = title;
            part.ContentItem.DisplayText = title;
            part.Apply();
        }
    }

    private async Task<TitlePartSettings> GetSettingsAsync(TitlePart part)
    {
        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(part.ContentItem.ContentType);
        var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, nameof(TitlePart), StringComparison.Ordinal));

        return contentTypePartDefinition.GetSettings<TitlePartSettings>();
    }
}
