using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;
using OrchardCore.Documents;
using OrchardCore.Entities;
using OrchardCore.Layers.Handlers;
using OrchardCore.Layers.Models;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Layers.Services;

internal sealed class LayerWidgetService : ILayerWidgetService
{
    private readonly IContentManager _contentManager;
    private readonly IContentDefinitionManager _definitions;
    private readonly ILayerService _layers;
    private readonly ISiteService _site;
    private readonly IAuthorizationService _authorization;
    private readonly ISession _session;
    private readonly IVolatileDocumentManager<LayerState> _state;
    internal readonly IStringLocalizer S;

    public LayerWidgetService(IContentManager contentManager, IContentDefinitionManager definitions, ILayerService layers,
        ISiteService site, IAuthorizationService authorization, ISession session, IVolatileDocumentManager<LayerState> state,
        IStringLocalizer<LayerWidgetService> localizer)
    {
        _contentManager = contentManager;
        _definitions = definitions;
        _layers = layers;
        _site = site;
        _authorization = authorization;
        _session = session;
        _state = state;
        S = localizer;
    }

    public async Task<string[]> GetZonesAsync() => (await _site.GetSettingsAsync<LayerSettings>()).Zones ?? [];

    public async Task<Dictionary<string, string[]>> ValidateAsync(LayerMetadata placement)
    {
        var errors = new Dictionary<string, string[]>();
        if (placement is null)
        {
            errors["placement"] = [S["A widget placement is required."]];
            return errors;
        }
        if (string.IsNullOrEmpty(placement.Layer) || await _layers.GetLayerAsync(placement.Layer) is null)
        {
            errors["layer"] = [S["Select an existing layer."]];
        }
        if (string.IsNullOrEmpty(placement.Zone) || !(await GetZonesAsync()).Contains(placement.Zone, StringComparer.Ordinal))
        {
            errors["zone"] = [S["Select a configured zone using its exact name."]];
        }
        if (!double.IsFinite(placement.Position))
        {
            errors["position"] = [S["The position must be a finite number."]];
        }
        return errors;
    }

    public async Task<LayerWidgetMutationResult> UpdateAsync(ClaimsPrincipal user, string contentItemId, LayerMetadata placement, bool positionOnly = false)
    {
        if (!await _authorization.AuthorizeAsync(user, Permissions.ManageLayers))
        {
            return new() { Status = LayerWidgetMutationStatus.Forbidden };
        }
        var latest = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
        if (latest is null)
        {
            return new() { Status = LayerWidgetMutationStatus.NotFound };
        }
        var published = latest.Published ? null : await _contentManager.GetAsync(contentItemId, VersionOptions.Published);
        ContentItem[] versions = published is null ? [latest] : [latest, published];
        // Check both versions before applying anything: a denied published version must not leave an edited draft.
        foreach (var item in versions)
        {
            if (!await _authorization.AuthorizeAsync(user, CommonPermissions.EditContent, item)
                || (item.Published && !await _authorization.AuthorizeAsync(user, CommonPermissions.PublishContent, item)))
            {
                return new() { Status = LayerWidgetMutationStatus.Forbidden };
            }
        }
        var placements = new List<LayerMetadata>();
        foreach (var item in versions)
        {
            var definition = await _definitions.GetTypeDefinitionAsync(item.ContentType);
            if (definition?.GetStereotype() != "Widget" || (positionOnly && !item.Has<LayerMetadata>()))
            {
                return new() { Status = LayerWidgetMutationStatus.Invalid, Errors = new() { ["contentItemId"] = [S["Select an existing widget with layer metadata when moving its position."]] } };
            }
            var current = item.TryGet<LayerMetadata>(out var existing) ? existing : new LayerMetadata();
            var desired = placement is null ? null : new LayerMetadata
            {
                Layer = positionOnly ? current.Layer : placement.Layer,
                RenderTitle = positionOnly ? current.RenderTitle : placement.RenderTitle,
                Zone = placement.Zone,
                Position = placement.Position,
            };
            var errors = await ValidateAsync(desired);
            if (errors.Count > 0)
            {
                return new() { Status = LayerWidgetMutationStatus.Invalid, Errors = errors };
            }
            desired.Layer = (await _layers.GetLayerAsync(desired.Layer)).Name;
            placements.Add(desired);
        }
        var changed = false;
        for (var index = 0; index < versions.Length; index++)
        {
            var item = versions[index];
            var desired = placements[index];
            var current = item.TryGet<LayerMetadata>(out var existing) ? existing : new LayerMetadata();
            if (item.Has<LayerMetadata>() && current.Layer == desired.Layer && current.Zone == desired.Zone
                && current.Position == desired.Position && current.RenderTitle == desired.RenderTitle)
            {
                continue;
            }
            // Retain extension properties and every other content part, version flag and lifecycle field.
            current.Layer = desired.Layer;
            current.Zone = desired.Zone;
            current.Position = desired.Position;
            current.RenderTitle = desired.RenderTitle;
            item.Apply(current);
            await _session.SaveAsync(item);
            changed = true;
        }
        if (changed)
        {
            // As in the existing drag-and-drop action, invalidate after the ambient session commits.
            await _state.UpdateAsync(new LayerState());
        }
        return new() { Status = LayerWidgetMutationStatus.Success, Placement = latest.Get<LayerMetadata>(nameof(LayerMetadata)) };
    }
}
