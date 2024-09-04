using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.ViewModels;

namespace OrchardCore.Flows.Drivers;

public sealed class FlowPartDisplayDriver : ContentPartDisplayDriver<FlowPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentManager _contentManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;

    internal readonly IHtmlLocalizer H;

    public FlowPartDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IContentManager contentManager,
        IServiceProvider serviceProvider,
        IHtmlLocalizer<FlowPartDisplayDriver> htmlLocalizer,
        INotifier notifier,
        ILogger<FlowPartDisplayDriver> logger
        )
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentManager = contentManager;
        _serviceProvider = serviceProvider;
        H = htmlLocalizer;
        _notifier = notifier;
        _logger = logger;
    }

    public override IDisplayResult Display(FlowPart flowPart, BuildPartDisplayContext context)
    {
        var hasItems = flowPart.Widgets.Count > 0;

        return Initialize<FlowPartViewModel>(hasItems ? "FlowPart" : "FlowPart_Empty", m =>
        {
            m.FlowPart = flowPart;
            m.BuildPartDisplayContext = context;
        })
        .Location("Detail", "Content");
    }

    public override IDisplayResult Edit(FlowPart flowPart, BuildPartEditorContext context)
    {
        return Initialize<FlowPartEditViewModel>(GetEditorShapeType(context), async model =>
        {
            var containedContentTypes = await GetContainedContentTypesAsync(context.TypePartDefinition);
            var notify = false;

            var existingWidgets = new List<ContentItem>();

            foreach (var widget in flowPart.Widgets)
            {
                if (!containedContentTypes.Any(c => c.Name == widget.ContentType))
                {
                    _logger.LogWarning("The Widget content item with id {ContentItemId} has no matching {ContentType} content type definition.", widget.ContentItem.ContentItemId, widget.ContentItem.ContentType);
                    await _notifier.WarningAsync(H["The Widget content item with id {0} has no matching {1} content type definition.", widget.ContentItem.ContentItemId, widget.ContentItem.ContentType]);
                    notify = true;
                }
                else
                {
                    existingWidgets.Add(widget);
                }
            }

            flowPart.Widgets = existingWidgets;

            if (notify)
            {
                await _notifier.WarningAsync(H["Publishing this content item may erase created content. Fix any content type issues beforehand."]);
            }

            model.FlowPart = flowPart;
            model.Updater = context.Updater;
            model.ContainedContentTypeDefinitions = containedContentTypes;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(FlowPart part, UpdatePartEditorContext context)
    {
        var contentItemDisplayManager = _serviceProvider.GetRequiredService<IContentItemDisplayManager>();

        var model = new FlowPartEditViewModel { FlowPart = part };

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var contentItems = new List<ContentItem>();

        for (var i = 0; i < model.Prefixes.Length; i++)
        {
            var contentItem = await _contentManager.NewAsync(model.ContentTypes[i]);
            var existingContentItem = part.Widgets.FirstOrDefault(x => string.Equals(x.ContentItemId, model.ContentItems[i], StringComparison.OrdinalIgnoreCase));

            // When the content item already exists merge its elements to reverse nested content item ids.
            // All of the data for these merged items is then replaced by the model values on update, while a nested content item id is maintained.
            // This prevents nested items which rely on the content item id, i.e. the media attached field, losing their reference point.
            if (existingContentItem != null)
            {
                contentItem.ContentItemId = model.ContentItems[i];
                contentItem.Merge(existingContentItem);
            }

            contentItem.Weld(new FlowMetadata());

            var widgetModel = await contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, context.IsNew, htmlFieldPrefix: model.Prefixes[i]);

            contentItems.Add(contentItem);
        }

        part.Widgets = contentItems;

        return Edit(part, context);
    }

    private async Task<IEnumerable<ContentTypeDefinition>> GetContainedContentTypesAsync(ContentTypePartDefinition typePartDefinition)
    {
        var settings = typePartDefinition.GetSettings<FlowPartSettings>();

        if (settings?.ContainedContentTypes == null || settings.ContainedContentTypes.Length == 0)
        {
            return (await _contentDefinitionManager.ListTypeDefinitionsAsync())
                .Where(t => t.StereotypeEquals("Widget"));
        }

        return (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Where(t => settings.ContainedContentTypes.Contains(t.Name) && t.StereotypeEquals("Widget"));
    }
}
