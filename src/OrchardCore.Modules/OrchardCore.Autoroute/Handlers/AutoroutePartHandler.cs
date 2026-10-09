using System.Globalization;
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
    // The length of int.MaxValue, the longest version.
    private const int MaxVersionLength = 10;

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
                var containedAspect = await _contentManager.PopulateAspectAsync<ContainedContentItemsAspect>(context.ContentItem);
                await CheckContainedHomeRouteAsync(part.ContentItem.ContentItemId, containedAspect, (JsonObject)context.ContentItem.Content);
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

    public override async Task CreatedAsync(CreateContentContext context, AutoroutePart part)
    {
        await GenerateContainerPathFromPatternAsync(part);
        await GenerateContainedPathsFromPatternAsync(context.ContentItem, part);
    }

    public override async Task UpdatedAsync(UpdateContentContext context, AutoroutePart part)
    {
        await GenerateContainerPathFromPatternAsync(part);
        await GenerateContainedPathsFromPatternAsync(context.ContentItem, part);
    }

    public override async Task CloningAsync(CloneContentContext context, AutoroutePart part)
    {
        if (!context.CloneContentItem.TryGet<AutoroutePart>(out var clonedPart))
        {
            throw new InvalidOperationException("The cloned content item doesn't contain an AutoroutePart.");
        }

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
                    if (contentItem.TryGet<AutoroutePart>(out var autoroutePart) && autoroutePart.SetHomepage)
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
                        DocumentId = contentItem.Id,
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
                // This is only relevant if the content items have an autoroute part as we adjust the part value as required to guarantee a unique route.
                // Content items routed only through the handler aspect already guarantee uniqueness.
                if (contentItem.TryGet<AutoroutePart>(out var containedAutoroutePart) && containedAutoroutePart is { Disabled: false, Path.Length: > 0 })
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
                            MergeNullValueHandling = MergeNullValueHandling.Merge,
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
                                MergeNullValueHandling = MergeNullValueHandling.Merge,
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

    internal static string GenerateRelativeUniquePath(List<AutorouteEntry> entries, string path, AutoroutePart context)
    {
        var version = 1;
        var unversionedPath = path;

        var versionSeparatorPosition = path.LastIndexOf('-');
        if (versionSeparatorPosition > -1 && int.TryParse(path[versionSeparatorPosition..].TrimStart('-'), out var parsedVersion))
        {
            version = parsedVersion;
            unversionedPath = path[..versionSeparatorPosition];
        }

        while (true)
        {
            // Find the max version already used for this unversionedPath in entries
            var maxVersion = entries
                .Where(e => e.Path.StartsWith(unversionedPath + "-", StringComparison.OrdinalIgnoreCase))
                .Select(e => int.TryParse(e.Path[(unversionedPath.Length + 1)..], out var v) ? v : 0)
                .DefaultIfEmpty(0)
                .Max();

            var nextVersion = Math.Max(version, maxVersion) + 1;

            while (true)
            {
                var versionedPath = $"{unversionedPath}-{nextVersion}";
                if (IsRelativePathUnique(entries, versionedPath, context))
                {
                    var entry = entries.SingleOrDefault(e => e.ContainedContentItemId == context.ContentItem.ContentItemId);
                    if (entry == null)
                    {
                        // Add new entry for this contained item if it doesn't exist yet, preventing a null reference exception
                        entry = new AutorouteEntry(context.ContentItem.ContentItemId, versionedPath, context.ContentItem.ContentItemId);
                        entries.Add(entry);
                    }
                    entry.Path = versionedPath;

                    return versionedPath;
                }
                nextVersion++;
            }
        }
    }


    private static bool IsRelativePathUnique(List<AutorouteEntry> entries, string path, AutoroutePart context)
    {
        var result = !entries.Any(e => context.ContentItem.ContentItemId != e.ContainedContentItemId && string.Equals(e.Path.Trim('/'), path.Trim('/'), StringComparison.OrdinalIgnoreCase));
        return result;
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

            part.Path = part.Path.ReplaceLineEndings(string.Empty);

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

    internal async Task<string> GenerateUniqueAbsolutePathAsync(string path, string contentItemId)
    {
        var version = 1;
        var unversionedPath = path;

        var versionSeparatorPosition = path.LastIndexOf('-');
        if (versionSeparatorPosition > -1 && int.TryParse(path[versionSeparatorPosition..].TrimStart('-'), out version))
        {
            unversionedPath = path[..versionSeparatorPosition];
        }

        // The versions taken by other content items are read with a single query, rather than with one query per
        // version, which made the n-th copy of a permalink run n queries.
        var takenVersions = await GetTakenVersionsAsync(unversionedPath, contentItemId);

        while (true)
        {
            if (!takenVersions.Contains(version))
            {
                var versionedPath = GetVersionedPath(unversionedPath, version);

                // The database has the final say, as its collation can consider two paths equal although they differ by
                // more than their case, for instance by trailing spaces or accents. Only such a path takes another query.
                if (await IsAbsolutePathUniqueAsync(versionedPath, contentItemId))
                {
                    return versionedPath;
                }
            }

            version++;
        }
    }

    /// <summary>
    /// Gets the versions for which another content item already uses the path that <see cref="GetVersionedPath"/>
    /// builds, with or without a leading or trailing slash, as <see cref="IsAbsolutePathUniqueAsync"/> checks them.
    /// </summary>
    private async Task<HashSet<int>> GetTakenVersionsAsync(string unversionedPath, string contentItemId)
    {
        // A versioned path starts with the unversioned path, shortened if needed to make room for the version, so the
        // stem of the longest version is a prefix of all of them. When it isn't shortened, neither is any other one.
        var shortestStem = GetVersionStem(unversionedPath, MaxVersionLength);
        var prefix = shortestStem.Length == unversionedPath.Length
            ? $"{shortestStem.TrimStart('/')}-"
            : shortestStem.TrimStart('/').ToString();

        // The prefix isn't escaped in the LIKE pattern. The '%' and '_' wildcards can only match more paths, which are
        // then left out below, but '[' starts a set of characters on SQL Server, and '\' escapes the next character on
        // PostgreSQL and MySQL, which could match fewer paths. So the prefix stops before them.
        var specialCharacterIndex = prefix.AsSpan().IndexOfAny('[', '\\');
        if (specialCharacterIndex > -1)
        {
            prefix = prefix[..specialCharacterIndex];
        }

        var slashedPrefix = "/" + prefix;

        var takenVersions = new HashSet<int>();

        var possibleConflicts = _session.QueryIndex<AutoroutePartIndex>(o => (o.Published || o.Latest) &&
            (o.Path.StartsWith(prefix) || o.Path.StartsWith(slashedPrefix))).ToAsyncEnumerable();

        await foreach (var possibleConflict in possibleConflicts)
        {
            if (possibleConflict.ContentItemId != contentItemId &&
                possibleConflict.ContainedContentItemId != contentItemId &&
                TryGetVersion(possibleConflict.Path, unversionedPath, out var takenVersion))
            {
                takenVersions.Add(takenVersion);
            }
        }

        return takenVersions;
    }

    /// <summary>
    /// Whether the path, without one leading and one trailing slash, is the one that <see cref="GetVersionedPath"/>
    /// builds for a version, ignoring the case.
    /// </summary>
    private static bool TryGetVersion(string path, string unversionedPath, out int version)
    {
        version = 0;

        if (path is null)
        {
            return false;
        }

        var trimmedPath = path.AsSpan();
        if (trimmedPath.StartsWith('/'))
        {
            trimmedPath = trimmedPath[1..];
        }

        if (trimmedPath.EndsWith('/'))
        {
            trimmedPath = trimmedPath[..^1];
        }

        var versionPosition = trimmedPath.Length;
        while (versionPosition > 0 && char.IsAsciiDigit(trimmedPath[versionPosition - 1]))
        {
            versionPosition--;
        }

        var versionText = trimmedPath[versionPosition..];

        // A version follows the separator, and is written without leading zeros.
        if (versionText.IsEmpty ||
            versionPosition == 0 ||
            trimmedPath[versionPosition - 1] != '-' ||
            (versionText.Length > 1 && versionText[0] == '0') ||
            !int.TryParse(versionText, NumberStyles.None, CultureInfo.InvariantCulture, out version))
        {
            return false;
        }

        var stem = GetVersionStem(unversionedPath, versionText.Length).TrimStart('/');

        return trimmedPath[..(versionPosition - 1)].Equals(stem, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetVersionedPath(string unversionedPath, int version)
    {
        var versionText = version.ToString(CultureInfo.InvariantCulture);

        return $"{GetVersionStem(unversionedPath, versionText.Length)}-{versionText}";
    }

    /// <summary>
    /// Gets the start of the unversioned path that fits before a version of the given length.
    /// </summary>
    private static ReadOnlySpan<char> GetVersionStem(string unversionedPath, int versionLength)
    {
        // Unversioned length + separator char + version length.
        var maxLength = AutoroutePart.MaxPathLength - 1 - versionLength;

        return unversionedPath.AsSpan(0, Math.Min(unversionedPath.Length, maxLength));
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
