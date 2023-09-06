using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.ViewModels;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Flows.Drivers
{
    public class BagPartDisplayDriver : ContentPartDisplayDriver<BagPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly INotifier _notifier;
        protected readonly IHtmlLocalizer H;
        private readonly IAuthorizationService _authorizationService;

        public BagPartDisplayDriver(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor,
            ILogger<BagPartDisplayDriver> logger,
            INotifier notifier,
            IHtmlLocalizer<BagPartDisplayDriver> htmlLocalizer,
            IAuthorizationService authorizationService
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _notifier = notifier;
            H = htmlLocalizer;
            _authorizationService = authorizationService;
        }

        public override IDisplayResult Display(BagPart bagPart, BuildPartDisplayContext context)
        {
            var hasItems = bagPart.ContentItems.Any();

            return Initialize<BagPartViewModel>(hasItems ? "BagPart" : "BagPart_Empty", m =>
            {
                m.BagPart = bagPart;
                m.BuildPartDisplayContext = context;
                m.Settings = context.TypePartDefinition.GetSettings<BagPartSettings>();
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(BagPart bagPart, BuildPartEditorContext context)
        {
            return Initialize<BagPartEditViewModel>(GetEditorShapeType(context), async m =>
            {
                var contentDefinitionManager = _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                m.BagPart = bagPart;
                m.Updater = context.Updater;
                m.ContainedContentTypeDefinitions = await GetContainedContentTypesAsync(context.TypePartDefinition);
                m.AccessibleWidgets = await GetAccessibleWidgetsAsync(bagPart.ContentItems, contentDefinitionManager);
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(BagPart part, UpdatePartEditorContext context)
        {
            var contentItemDisplayManager = _serviceProvider.GetRequiredService<IContentItemDisplayManager>();
            var contentDefinitionManager = _serviceProvider.GetRequiredService<IContentDefinitionManager>();

            var model = new BagPartEditViewModel { BagPart = part };

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            var contentItems = new List<ContentItem>();

            // Handle the content found in the request
            for (var i = 0; i < model.Prefixes.Length; i++)
            {
                var contentItem = await _contentManager.NewAsync(model.ContentTypes[i]);

                // assign the owner of the item to ensure we can validate access to it later.
                contentItem.Owner = GetCurrentOwner();

                // Try to match the requested id with an existing id
                var existingContentItem = part.ContentItems.FirstOrDefault(x => String.Equals(x.ContentItemId, model.ContentItems[i], StringComparison.OrdinalIgnoreCase));

                if (existingContentItem == null && !await AuthorizeAsync(contentDefinitionManager, CommonPermissions.EditContent, contentItem))
                {
                    // at this point the user is somehow trying to add content with no privileges. ignore the request
                    continue;
                }

                // When the content item already exists merge its elements to preserve nested content item ids.
                // All of the data for these merged items is then replaced by the model values on update, while a nested content item id is maintained.
                // This prevents nested items which rely on the content item id, i.e. the media attached field, losing their reference point.
                if (existingContentItem != null)
                {
                    if (!await AuthorizeAsync(contentDefinitionManager, CommonPermissions.EditContent, existingContentItem))
                    {
                        // at this point the user is somehow modifying existing content with no privileges.
                        // honor the existing data and ignore the data in the request
                        contentItems.Add(existingContentItem);

                        continue;
                    }

                    // at this point the user have privileges to edit, merge the data from the request
                    contentItem.ContentItemId = model.ContentItems[i];
                    contentItem.Merge(existingContentItem);
                }

                var widgetModel = await contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, context.IsNew, htmlFieldPrefix: model.Prefixes[i]);

                contentItems.Add(contentItem);
            }

            // at the end, lets add existing readonly contents.
            foreach (var existingContentItem in part.ContentItems)
            {
                if (contentItems.Any(x => x.ContentItemId == existingContentItem.ContentItemId))
                {
                    // item was already added using the edit

                    continue;
                }

                if (await AuthorizeAsync(contentDefinitionManager, CommonPermissions.DeleteContent, existingContentItem))
                {
                    // at this point the user has permission to delete a securable item or the type isn't securable
                    // if the existsing content id isn't in the requested ids, don't add the content item... meaning the user deleted it
                    if (!model.ContentItems.Contains(existingContentItem.ContentItemId))
                    {
                        continue;
                    }
                }

                // since the content item isn't editable, lets add it so it's not removed from the collection
                contentItems.Add(existingContentItem);
            }

            // TODO, some how here contentItems should be sorted by a defined order
            part.ContentItems = contentItems;

            return Edit(part, context);
        }

        private async Task<IEnumerable<BagPartWidgetViewModel>> GetAccessibleWidgetsAsync(IEnumerable<ContentItem> contentItems, IContentDefinitionManager contentDefinitionManager)
        {
            var widgets = new List<BagPartWidgetViewModel>();

            foreach (var contentItem in contentItems)
            {
                var widget = new BagPartWidgetViewModel
                {
                    ContentItem = contentItem,
                    Viewable = true,
                    Editable = true,
                    Deletable = true,
                };

                if (IsSecurable(contentDefinitionManager, contentItem.ContentType, out var contentTypeDefinition))
                {
                    widget.Viewable = await AuthorizeAsync(CommonPermissions.ViewContent, contentItem);
                    widget.Editable = await AuthorizeAsync(CommonPermissions.EditContent, contentItem);
                    widget.Deletable = await AuthorizeAsync(CommonPermissions.DeleteContent, contentItem);
                }

                if (contentTypeDefinition == null)
                {
                    _logger.LogWarning("The Widget content item with id {ContentItemId} has no matching {ContentType} content type definition.", contentItem.ContentItemId, contentItem.ContentType);

                    await _notifier.WarningAsync(H["The Widget content item with id {0} has no matching {1} content type definition.", contentItem.ContentItemId, contentItem.ContentType]);

                    continue;
                }

                widget.ContentTypeDefinition = contentTypeDefinition;

                if (widget.Editable || widget.Viewable)
                {
                    widgets.Add(widget);
                }
            }

            return widgets;
        }

        private async Task<bool> AuthorizeAsync(IContentDefinitionManager contentDefinitionManager, Permission permission, ContentItem contentItem)
        {
            if (!IsSecurable(contentDefinitionManager, contentItem.ContentType, out _))
            {
                return true;
            }

            return await AuthorizeAsync(permission, contentItem);
        }

        private async Task<bool> AuthorizeAsync(Permission permission, ContentItem contentItem)
        {
            return await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, permission, contentItem);
        }

        private static bool IsSecurable(IContentDefinitionManager contentDefinitionManager, string contentType, out ContentTypeDefinition contentTypeDefinition)
        {
            contentTypeDefinition = contentDefinitionManager.GetTypeDefinition(contentType);

            var settings = contentTypeDefinition?.GetSettings<ContentTypeSettings>();

            return settings?.Securable ?? false;
        }

        private async Task<IEnumerable<ContentTypeDefinition>> GetContainedContentTypesAsync(ContentTypePartDefinition typePartDefinition)
        {
            var settings = typePartDefinition.GetSettings<BagPartSettings>();
            var contentTypes = Enumerable.Empty<ContentTypeDefinition>();

            if (settings.ContainedStereotypes != null && settings.ContainedStereotypes.Length > 0)
            {
                contentTypes = _contentDefinitionManager.ListTypeDefinitions()
                    .Where(contentType => contentType.HasStereotype() && settings.ContainedStereotypes.Contains(contentType.GetStereotype(), StringComparer.OrdinalIgnoreCase));
            }
            else if (settings.ContainedContentTypes != null && settings.ContainedContentTypes.Length > 0)
            {
                contentTypes = settings.ContainedContentTypes
                    .Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType))
                    .Where(contentType => contentType != null);
            }
            var user = _httpContextAccessor.HttpContext.User;

            var accessibleContentTypes = new List<ContentTypeDefinition>();

            foreach (var contentType in contentTypes)
            {
                if (contentType.IsSecurable() && !await _authorizationService.AuthorizeContentTypeAsync(user, CommonPermissions.EditContent, contentType, GetCurrentOwner()))
                {
                    continue;
                }

                accessibleContentTypes.Add(contentType);
            }

            return accessibleContentTypes;
        }

        private string GetCurrentOwner()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
