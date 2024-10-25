using System.Text.Json.Nodes;
using System.Text.Json.Settings;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Autoroute.Core.Indexes;
using OrchardCore.Autoroute.Models;
using OrchardCore.Autoroute.ViewModels;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid;
using OrchardCore.Localization;
using OrchardCore.Settings;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Autoroute.Handlers;

public class AutoroutePartHandler : ContentPartHandler<AutoroutePart>
{
    private readonly IAutorouteEntries _entries;
    private readonly AutorouteOptions _options;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ISiteService _siteService;
    private readonly ITagCache _tagCache;
    private readonly ISession _session;
    private readonly IServiceProvider _serviceProvider;

    protected readonly IStringLocalizer S;

    private IContentManager _contentManager;

    public AutoroutePartHandler(
        IAutorouteEntries entries,
        IOptions<AutorouteOptions> options,
        ILiquidTemplateManager liquidTemplateManager,
        IContentDefinitionManager contentDefinitionManager,
        ISiteService siteService,
        ITagCache tagCache,
        ISession session,
        IServiceProvider serviceProvider,
        IStringLocalizer<AutoroutePartHandler> stringLocalizer)
    {
        _entries = entries;
        _options = options.Value;
        _liquidTemplateManager = liquidTemplateManager;
        _contentDefinitionManager = contentDefinitionManager;
        _siteService = siteService;
        _tagCache = tagCache;
        _session = session;
        _serviceProvider = serviceProvider;
        S = stringLocalizer;
    }

    public override async Task PublishedAsync(PublishContentContext context, AutoroutePart part)
    {
        if (!string.IsNullOrWhiteSpace(part.Path))
        {
            if (part.RouteContainedItems)
            {
                _contentManager ??= _serviceProvider.GetRequiredService<IContentManager>();
                var containedAspect = await _contentManager.PopulateAspectAsync<ContainedContentItemsAspect>(context.PublishingItem);
                await CheckContainedHomeRouteAsync(part.ContentItem.ContentItemId, containedAspect, (JsonObject)context.PublishingItem.Content);
            }

            // Update entries from the index table after the session is committed.
            await _entries.UpdateEntriesAsync();
        }

        if (!string.IsNullOrWhiteSpace(part.Path) && !part.Disabled && part.SetHomepage)
        {
            await SetHomeRouteAsync(part, homeRoute =>
            {
                homeRoute[_options.ContentItemIdKey] = context.ContentItem.ContentItemId;
                homeRoute.Remove(_options.JsonPathKey);
            });
        }

        // Evict any dependent item from cache.
        await RemoveTagAsync(part);
    }

    public override async Task UnpublishedAsync(PublishContentContext context, AutoroutePart part)
    {
        if (!string.IsNullOrWhiteSpace(part.Path))
        {
            // Update entries from the index table after the session is committed.
            await _entries.UpdateEntriesAsync();

            // Evict any dependent item from cache.
            await RemoveTagAsync(part);
        }
    }

    public override async Task RemovedAsync(RemoveContentContext context, AutoroutePart part)
    {
        if (!string.IsNullOrWhiteSpace(part.Path) && context.NoActiveVersionLeft)
        {
            // Update entries from the index table after the session is committed.
            await _entries.UpdateEntriesAsync();

            // Evict any dependent item from cache.
            await RemoveTagAsync(part);
        }
    }

    public override async Task ValidatingAsync(ValidateContentContext context, AutoroutePart part)
    {
        // Only validate the path if it's not empty.
        if (string.IsNullOrWhiteSpace(part.Path))
        {
            return;
        }

        foreach (var item in part.ValidatePathFieldValue(S))
        {
            context.Fail(item);
        }

        if (!await IsAbsolutePathUniqueAsync(part.Path, part.ContentItem.ContentItemId))
        {
            context.Fail(S["Your permalink is already in use."], nameof(part.Path));
        }

    }

    public override async Task UpdatedAsync(UpdateContentContext context, AutoroutePart part)
    {
        await GenerateContainerPathFromPatternAsync(part);
        await GenerateContainedPathsFromPatternAsync(context.UpdatingItem, part);
    }

    public override async Task CloningAsync(CloneContentContext context, AutoroutePart part)
    {
        var clonedPart = context.CloneContentItem.As<AutoroutePart>();
        clonedPart.Path = await GenerateUniqueAbsolutePathAsync(part.Path, context.CloneContentItem.ContentItemId);
        clonedPart.SetHomepage = false;
        clonedPart.Apply();

        await GenerateContainedPathsFromPatternAsync(context.CloneContentItem, part);
    }

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context, AutoroutePart part)
    {
        return context.ForAsync<RouteHandlerAspect>(async aspect =>
        {
            var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "AutoroutePart", StringComparison.Ordinal));
            var settings = contentTypePartDefinition.GetSettings<AutoroutePartSettings>();
            if (settings.ManageContainedItemRoutes)
            {
                aspect.Path = part.Path;
                aspect.Absolute = part.Absolute;
                aspect.Disabled = part.Disabled;
            }
        });
    }

    private async Task SetHomeRouteAsync(AutoroutePart part, Action<RouteValueDictionary> action)
    {
        var site = await _siteService.LoadSiteSettingsAsync();

        site.HomeRoute ??= [];

        var homeRoute = site.HomeRoute;

        foreach (var entry in _options.GlobalRouteValues)
        {
            homeRoute[entry.Key] = entry.Value;
        }

        action.Invoke(homeRoute);

        // Once we took the flag into account we can dismiss it.
        part.SetHomepage = false;
        part.Apply();

        await _siteService.UpdateSiteSettingsAsync(site);
    }

    private Task RemoveTagAsync(AutoroutePart part)
    {
        return _tagCache.RemoveTagAsync($"slug:{part.Path}");
    }

    private async Task CheckContainedHomeRouteAsync(string containerContentItemId, ContainedContentItemsAspect containedAspect, JsonObject content)
    {
        foreach (var accessor in containedAspect.Accessors)
        {
            var jItems = accessor.Invoke(content);

            foreach (var jItem in jItems.Cast<JsonObject>())
            {
                var contentItem = jItem.ToObject<ContentItem>();
                var handlerAspect = await _contentManager.PopulateAspectAsync<RouteHandlerAspect>(contentItem);

                if (!handlerAspect.Disabled)
                {
                    // Only an autoroute part, not a default handler aspect can set itself as the homepage.
                    var autoroutePart = contentItem.As<AutoroutePart>();
                    if (autoroutePart != null && autoroutePart.SetHomepage)
                    {
                        await SetHomeRouteAsync(autoroutePart, homeRoute =>
                        {
                            homeRoute[_options.ContentItemIdKey] = containerContentItemId;
                            homeRoute[_options.JsonPathKey] = jItem.GetNormalizedPath();
                        });

                        break;
                    }
                }
            }
        }
    }

    private async Task GenerateContainedPathsFromPatternAsync(ContentItem contentItem, AutoroutePart part)
    {
        // Validate contained content item routes if container has valid path.
        if (string.IsNullOrWhiteSpace(part.Path) || !part.RouteContainedItems)
        {
            return;
        }

        _contentManager ??= _serviceProvider.GetRequiredService<IContentManager>();
        var containedAspect = await _contentManager.PopulateAspectAsync<ContainedContentItemsAspect>(contentItem);

        // Build the entries for this content item to evaluate for duplicates.
        var entries = new List<AutorouteEntry>();
        await PopulateContainedContentItemRoutesAsync(entries, part.ContentItem.ContentItemId, containedAspect, (JsonObject)contentItem.Content, part.Path);

        await ValidateContainedContentItemRoutesAsync(entries, part.ContentItem.ContentItemId, containedAspect, (JsonObject)contentItem.Content, part.Path);
    }

    private async Task PopulateContainedContentItemRoutesAsync(List<AutorouteEntry> entries, string containerContentItemId, ContainedContentItemsAspect containedContentItemsAspect, JsonObject content, string basePath)
    {
        foreach (var accessor in containedContentItemsAspect.Accessors)
        {
            var jItems = accessor.Invoke(content);

            foreach (var jItem in jItems.Cast<JsonObject>())
            {
                var contentItem = jItem.ToObject<ContentItem>();
                var handlerAspect = await _contentManager.PopulateAspectAsync<RouteHandlerAspect>(contentItem);

                if (!handlerAspect.Disabled)
                {
                    var path = handlerAspect.Path;
                    if (!handlerAspect.Absolute)
                    {
                        path = (basePath.EndsWith('/') ? basePath : basePath + '/') + handlerAspect.Path.TrimStart('/');
                    }

                    entries.Add(new AutorouteEntry(containerContentItemId, path, contentItem.ContentItemId, jItem.GetNormalizedPath())
                    {
                        DocumentId = contentItem.Id
                    });
                }

                var itemBasePath = (basePath.EndsWith('/') ? basePath : basePath + '/') + handlerAspect.Path.TrimStart('/');
                var childrenAspect = await _contentManager.PopulateAspectAsync<ContainedContentItemsAspect>(contentItem);
                await PopulateContainedContentItemRoutesAsync(entries, containerContentItemId, childrenAspect, jItem, itemBasePath);
            }
        }
    }

    private async Task ValidateContainedContentItemRoutesAsync(List<AutorouteEntry> entries, string containerContentItemId, ContainedContentItemsAspect containedContentItemsAspect, JsonObject content, string basePath)
    {
        foreach (var accessor in containedContentItemsAspect.Accessors)
        {
            var jItems = accessor.Invoke(content);

            foreach (var jItem in jItems.Cast<JsonObject>())
            {
                var contentItem = jItem.ToObject<ContentItem>();
                var containedAutoroutePart = contentItem.As<AutoroutePart>();

                // This is only relevant if the content items have an autoroute part as we adjust the part value as required to guarantee a unique route.
                // Content items routed only through the handler aspect already guarantee uniqueness.
                if (containedAutoroutePart != null && !containedAutoroutePart.Disabled)
                {
                    var path = containedAutoroutePart.Path;

                    if (containedAutoroutePart.Absolute && !await IsAbsolutePathUniqueAsync(path, contentItem.ContentItemId))
                    {
                        path = await GenerateUniqueAbsolutePathAsync(path, contentItem.ContentItemId);
                        containedAutoroutePart.Path = path;
                        containedAutoroutePart.Apply();

                        // Merge because we have disconnected the content item from it's json owner.
                        jItem.Merge((JsonObject)contentItem.Content, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Replace,
                            MergeNullValueHandling = MergeNullValueHandling.Merge
                        });
                    }
                    else
                    {
                        var currentItemBasePath = basePath.EndsWith('/') ? basePath : basePath + '/';
                        path = currentItemBasePath + containedAutoroutePart.Path.TrimStart('/');
                        if (!IsRelativePathUnique(entries, path, containedAutoroutePart))
                        {
                            path = GenerateRelativeUniquePath(entries, path, containedAutoroutePart);
                            // Remove base path and update part path.
                            containedAutoroutePart.Path = path[currentItemBasePath.Length..];
                            containedAutoroutePart.Apply();

                            // Merge because we have disconnected the content item from it's json owner.
                            jItem.Merge((JsonObject)contentItem.Content, new JsonMergeSettings
                            {
                                MergeArrayHandling = MergeArrayHandling.Replace,
                                MergeNullValueHandling = MergeNullValueHandling.Merge
                            });
                        }

                        path = path[currentItemBasePath.Length..];
                    }

                    var containedItemBasePath = (basePath.EndsWith('/') ? basePath : basePath + '/') + path;
                    var childItemAspect = await _contentManager.PopulateAspectAsync<ContainedContentItemsAspect>(contentItem);
                    await ValidateContainedContentItemRoutesAsync(entries, containerContentItemId, childItemAspect, jItem, containedItemBasePath);
                }
            }
        }
    }

    private static bool IsRelativePathUnique(List<AutorouteEntry> entries, string path, AutoroutePart context)
    {
        var result = !entries.Any(e => context.ContentItem.ContentItemId != e.ContainedContentItemId && string.Equals(e.Path.Trim('/'), path.Trim('/'), StringComparison.OrdinalIgnoreCase));
        return result;
    }

    private static string GenerateRelativeUniquePath(List<AutorouteEntry> entries, string path, AutoroutePart context)
    {
        var version = 1;
        var unversionedPath = path;

        var versionSeparatorPosition = path.LastIndexOf('-');
        if (versionSeparatorPosition > -1 && int.TryParse(path[versionSeparatorPosition..].TrimStart('-'), out version))
        {
            unversionedPath = path[..versionSeparatorPosition];
        }

        while (true)
        {
            // Unversioned length + separator char + version length.
            var quantityCharactersToTrim = unversionedPath.Length + 1 + version.ToString().Length - AutoroutePart.MaxPathLength;
            if (quantityCharactersToTrim > 0)
            {
                unversionedPath = unversionedPath[..^quantityCharactersToTrim];
            }

            var versionedPath = $"{unversionedPath}-{version++}";
            if (IsRelativePathUnique(entries, versionedPath, context))
            {
                var entry = entries.FirstOrDefault(e => e.ContainedContentItemId == context.ContentItem.ContentItemId);
                entry.Path = versionedPath;

                return versionedPath;
            }
        }
    }

    private async Task GenerateContainerPathFromPatternAsync(AutoroutePart part)
    {
        // Compute the Path only if it's empty.
        if (!string.IsNullOrWhiteSpace(part.Path))
        {
            return;
        }

        var pattern = await GetPatternAsync(part);

        if (!string.IsNullOrEmpty(pattern))
        {
            var model = new AutoroutePartViewModel()
            {
                Path = part.Path,
                AutoroutePart = part,
                ContentItem = part.ContentItem,
            };

            _contentManager ??= _serviceProvider.GetRequiredService<IContentManager>();

            var cultureAspect = await _contentManager.PopulateAspectAsync(part.ContentItem, new CultureAspect());

            var cultureOptions = _serviceProvider.GetService<IOptions<CultureOptions>>().Value;

            using (CultureScope.Create(cultureAspect.Culture, ignoreSystemSettings: cultureOptions.IgnoreSystemSettings))
            {
                part.Path = await _liquidTemplateManager.RenderStringAsync(pattern, NullEncoder.Default, model,
                    new Dictionary<string, FluidValue>() { [nameof(ContentItem)] = new ObjectValue(model.ContentItem) });
            }

            part.Path = part.Path.Replace("\r", string.Empty).Replace("\n", string.Empty);

            if (part.Path?.Length > AutoroutePart.MaxPathLength)
            {
                part.Path = part.Path[..AutoroutePart.MaxPathLength];
            }

            if (!await IsAbsolutePathUniqueAsync(part.Path, part.ContentItem.ContentItemId))
            {
                part.Path = await GenerateUniqueAbsolutePathAsync(part.Path, part.ContentItem.ContentItemId);
            }

            part.Apply();
        }
    }

    /// <summary>
    /// Get the pattern from the AutoroutePartSettings property for its type.
    /// </summary>
    private async Task<string> GetPatternAsync(AutoroutePart part)
    {
        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(part.ContentItem.ContentType);
        var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, nameof(AutoroutePart), StringComparison.Ordinal));
        var pattern = contentTypePartDefinition.GetSettings<AutoroutePartSettings>().Pattern;

        return pattern;
    }

    private async Task<string> GenerateUniqueAbsolutePathAsync(string path, string contentItemId)
    {
        var version = 1;
        var unversionedPath = path;

        var versionSeparatorPosition = path.LastIndexOf('-');
        if (versionSeparatorPosition > -1 && int.TryParse(path[versionSeparatorPosition..].TrimStart('-'), out version))
        {
            unversionedPath = path[..versionSeparatorPosition];
        }

        while (true)
        {
            // Unversioned length + separator char + version length.
            var quantityCharactersToTrim = unversionedPath.Length + 1 + version.ToString().Length - AutoroutePart.MaxPathLength;
            if (quantityCharactersToTrim > 0)
            {
                unversionedPath = unversionedPath[..^quantityCharactersToTrim];
            }

            var versionedPath = $"{unversionedPath}-{version++}";
            if (await IsAbsolutePathUniqueAsync(versionedPath, contentItemId))
            {
                return versionedPath;
            }
        }
    }

    private async Task<bool> IsAbsolutePathUniqueAsync(string path, string contentItemId)
    {
        path = path.Trim('/');
        var paths = new string[] { path, "/" + path, path + "/", "/" + path + "/" };

        var possibleConflicts = await _session.QueryIndex<AutoroutePartIndex>(o => (o.Published || o.Latest) && o.Path.IsIn(paths)).ListAsync();
        if (possibleConflicts.Any(x => x.ContentItemId != contentItemId && x.ContainedContentItemId != contentItemId))
        {
            return false;
        }

        return true;
    }
}
