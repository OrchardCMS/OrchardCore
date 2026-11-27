using Microsoft.AspNetCore.Routing;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents;
using OrchardCore.ContentTypes;
using OrchardCore.Lists.Models;
using OrchardCore.Localization.Data;
using OrchardCore.Navigation;

namespace OrchardCore.Lists.Services;

/// <summary>
/// Adds the container of a contained content item, such as a blog for a blog post, to the breadcrumb trail
/// of the content item's Create and Edit pages, so the user can navigate back to the parent.
/// </summary>
public sealed class ContainerBreadcrumbProvider : IBreadcrumbProvider
{
    private const string ContainerIdKey = "ListPart.ContainerId";

    private readonly IContentManager _contentManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IDataLocalizer _dataLocalizer;

    public ContainerBreadcrumbProvider(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, IDataLocalizer dataLocalizer)
    {
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        _dataLocalizer = dataLocalizer;
    }

    public async ValueTask BuildBreadcrumbAsync(BreadcrumbContext context)
    {
        if (context.Name is not ("ContentsEdit" or "ContentsCreate")
            || !AdminAttribute.IsApplied(context.ViewContext.HttpContext)
            || context.Items.Any(item => item.Id == "Container"))
        {
            return;
        }

        var containerId = await GetContainerIdAsync(context);

        if (string.IsNullOrEmpty(containerId))
        {
            return;
        }

        var container = await _contentManager.GetAsync(containerId)
            ?? await _contentManager.GetAsync(containerId, VersionOptions.Latest);

        if (container is null)
        {
            return;
        }

        var text = container.DisplayText;
        if (string.IsNullOrWhiteSpace(text))
        {
            var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(container.ContentType);
            text = typeDefinition is not null ? _dataLocalizer[typeDefinition.DisplayName, DataLocalizationContext.ContentType] : container.ContentType;
        }

        var item = new BreadcrumbItem
        {
            Id = "Container",
            Text = text,

            RouteValues = new RouteValueDictionary
            {
                { "area", "OrchardCore.Contents" },
                { "controller", "Admin" },
                { "action", "Display" },
                { "contentItemId", container.ContentItemId },
            },
            Resource = container,
            Permissions = { CommonPermissions.ViewContent },
        };

        // The container replaces the generic content type node, as the contained items are listed by their container.
        var typeIndex = context.Items.FindIndex(i => i.Id == "ContentType");

        if (typeIndex >= 0)
        {
            context.Items[typeIndex] = item;
        }
        else
        {
            context.Items.Add(item);
        }
    }

    private async Task<string> GetContainerIdAsync(BreadcrumbContext context)
    {
        var request = context.ViewContext.HttpContext.Request;

        if (context.Name == "ContentsCreate")
        {
            return request.Query[ContainerIdKey].ToString();
        }

        var contentItemId = context.ViewContext.RouteData.Values["contentItemId"]?.ToString();

        if (string.IsNullOrEmpty(contentItemId))
        {
            return null;
        }

        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        return contentItem is not null && contentItem.TryGet<ContainedPart>(out var containedPart)
            ? containedPart.ListContentItemId
            : null;
    }
}
