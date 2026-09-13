using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment;

/// <summary>Loads and serializes content using the existing download permission rules.</summary>
public sealed class ContentExportService
{
    private readonly IContentManager _contentManager;
    private readonly IAuthorizationService _authorization;

    /// <summary>Creates an export service for the current tenant.</summary>
    public ContentExportService(IContentManager contentManager, IAuthorizationService authorization)
    {
        _contentManager = contentManager;
        _authorization = authorization;
    }

    /// <summary>Loads the requested version after Export and per-item EditContent authorization.</summary>
    public async Task<ContentItem> GetAsync(string contentItemId, bool latest, ClaimsPrincipal user)
    {
        if (!await _authorization.AuthorizeAsync(user, DeploymentPermissions.Export))
        {
            throw new UnauthorizedAccessException("The initiating user cannot export content.");
        }
        var item = await _contentManager.GetAsync(contentItemId, latest ? VersionOptions.Latest : VersionOptions.Published);
        if (item is not null && !await _authorization.AuthorizeAsync(user, CommonPermissions.EditContent, item))
        {
            throw new UnauthorizedAccessException("The initiating user cannot export this content item.");
        }
        return item;
    }

    /// <summary>Serializes the same content as the admin download; recipe exports omit local document IDs.</summary>
    public static JsonObject Serialize(ContentItem item, bool recipe = false)
    {
        var json = JObject.FromObject(item);
        if (recipe) { json.Remove(nameof(ContentItem.Id)); }
        return json;
    }
}
