using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Flows.Drivers;

public sealed class FlowPartDisplayDriver : ContentPartDisplayDriver<FlowPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentManager _contentManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEnumerable<IContentHandler> _contentHandlers;
    private readonly IAuthorizationService _authorizationService;
    private readonly IEnumerable<IContentHandler> _reversedContentHandlers;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;

    internal readonly IHtmlLocalizer H;

    public FlowPartDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IContentManager contentManager,
        IServiceProvider serviceProvider,
        IHttpContextAccessor httpContextAccessor,
        IEnumerable<IContentHandler> contentHandlers,
        IAuthorizationService authorizationService,
        IHtmlLocalizer<FlowPartDisplayDriver> htmlLocalizer,
        INotifier notifier,
        ILogger<FlowPartDisplayDriver> logger
        )
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentManager = contentManager;
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
        _contentHandlers = contentHandlers;
        _authorizationService = authorizationService;
        _reversedContentHandlers = contentHandlers.Reverse();
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
        .Location(OrchardCoreConstants.DisplayType.Detail, "Content");
    }

    public override IDisplayResult Edit(FlowPart flowPart, BuildPartEditorContext context)
        => EditInternal(flowPart, context, null);

    private ShapeResult EditInternal(FlowPart flowPart, BuildPartEditorContext context, string[] prefixes)
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
            model.Prefixes = prefixes;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(FlowPart part, UpdatePartEditorContext context)
    {
        var contentItemDisplayManager = _serviceProvider.GetRequiredService<IContentItemDisplayManager>();

        var model = new FlowPartEditViewModel { FlowPart = part };

        await context.Updater.TryUpdateModelAsync(model, Prefix);


        var contentItems = new Dictionary<string, ContentItem>(StringComparer.OrdinalIgnoreCase);
        var existsingContentItems = part.Widgets.ToDictionary(x => x.ContentItemId, StringComparer.OrdinalIgnoreCase);

        // Handle the content found in the request
        for (var i = 0; i < model.Prefixes.Length; i++)
        {
            var contentItem = await _contentManager.NewAsync(model.ContentTypes[i]);

            // Assign the owner of the item to ensure we can validate access to it later.
            contentItem.Owner = GetCurrentOwner();

            // Try to match the requested id with an existing id
            ContentItem existingContentItem = null;

            existsingContentItems.TryGetValue(model.ContentItems[i], out existingContentItem);

            var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

            if (existingContentItem == null && !await AuthorizeAsync(contentTypeDefinition, CommonPermissions.EditContent, contentItem))
            {
                // At this point the user is somehow trying to add content with no privileges. ignore the request
                continue;
            }

            // When the content item already exists merge its elements to preserve nested content item ids.
            // All of the data for these merged items is then replaced by the model values on update, while a nested content item id is maintained.
            // This prevents nested items which rely on the content item id, i.e. the media attached field, losing their reference point.
            if (existingContentItem != null)
            {
                if (!await AuthorizeAsync(contentTypeDefinition, CommonPermissions.EditContent, existingContentItem))
                {
                    // At this point, the user is somehow modifying existing content with no privileges.
                    // honor the existing data and ignore the data in the request
                    contentItems.Add(existingContentItem.ContentItemId, existingContentItem);

                    continue;
                }

                contentItem.Weld(new FlowMetadata());

                // At this point the user have privileges to edit, merge the data from the request
                var updateContentContext = new UpdateContentContext(contentItem);

                await _contentHandlers.InvokeAsync((handler, context) => handler.UpdatingAsync(context), updateContentContext, _logger);

                contentItem.ContentItemId = model.ContentItems[i];
                contentItem.Merge(existingContentItem);

                await contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, context.IsNew, htmlFieldPrefix: model.Prefixes[i]);
                await _reversedContentHandlers.InvokeAsync((handler, context) => handler.UpdatedAsync(context), updateContentContext, _logger);
            }
            else
            {
                contentItem.Weld(new FlowMetadata());

                var createContentContext = new CreateContentContext(contentItem);

                await _contentHandlers.InvokeAsync((handler, context) => handler.CreatingAsync(context), createContentContext, _logger);
                await contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, context.IsNew, htmlFieldPrefix: model.Prefixes[i]);
                await _reversedContentHandlers.InvokeAsync((handler, context) => handler.CreatedAsync(context), createContentContext, _logger);
            }

            contentItems.Add(contentItem.ContentItemId, contentItem);
        }

        // At the end, lets add existing readonly contents.
        foreach (var existingContentItem in part.Widgets)
        {
            if (contentItems.ContainsKey(existingContentItem.ContentItemId))
            {
                // Item was already added using the edit.

                continue;
            }

            var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(existingContentItem.ContentType);

            if (await AuthorizeAsync(contentTypeDefinition, CommonPermissions.DeleteContent, existingContentItem))
            {
                // At this point, the user has permission to delete a securable item or the type isn't securable
                // if the existing content id isn't in the requested ids, don't add the content item... meaning the user deleted it.
                if (!model.ContentItems.Contains(existingContentItem.ContentItemId))
                {
                    continue;
                }
            }

            // Since the content item isn't editable, lets add it so it's not removed from the collection
            contentItems.Add(existingContentItem.ContentItemId, existingContentItem);
        }

        part.Widgets = contentItems.Values.ToList();

        return EditInternal(part, context, model.Prefixes);
    }

    private async Task<IEnumerable<ContentTypeDefinition>> GetContainedContentTypesAsync(ContentTypePartDefinition typePartDefinition)
    {
        var settings = typePartDefinition.GetSettings<FlowPartSettings>();

        if (settings?.ContainedContentTypes == null || settings.ContainedContentTypes.Length == 0)
        {
            return await _contentDefinitionManager.ListWidgetTypeDefinitionsAsync();
        }

        return (await _contentDefinitionManager.ListWidgetTypeDefinitionsAsync())
            .Where(t => settings.ContainedContentTypes.Contains(t.Name));
    }

    private async Task<bool> AuthorizeAsync(ContentTypeDefinition contentTypeDefinition, Permission permission, ContentItem contentItem)
    {
        if (contentTypeDefinition is not null && contentTypeDefinition.IsSecurable())
        {
            return await AuthorizeAsync(permission, contentItem);
        }

        return true;
    }

    private Task<bool> AuthorizeAsync(Permission permission, ContentItem contentItem)
        => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, permission, contentItem);

    private string GetCurrentOwner()
    {
        return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
