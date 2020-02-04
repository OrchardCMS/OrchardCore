using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.ContainerRoute.Drivers;
using OrchardCore.ContainerRoute.Models;
using OrchardCore.ContainerRoute.Routing;
using OrchardCore.ContainerRoute.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid;
using OrchardCore.Settings;

namespace OrchardCore.ContainerRoute.Handlers
{
    public class ContainerRoutePartHandler : ContentPartHandler<ContainerRoutePart>
    {
        private readonly IContainerRouteEntries _entries;
        private readonly ContainerRouteOptions _options;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly ITagCache _tagCache;
        private readonly IContentRoutingValidationCoordinator _contentRoutingValidationCoordinator;
        private readonly IServiceProvider _serviceProvider;

        private IContentManager _contentManager;
        public ContainerRoutePartHandler(
            IContainerRouteEntries entries,
            IOptions<ContainerRouteOptions> options,
            ILiquidTemplateManager liquidTemplateManager,
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            ITagCache tagCache,
            IContentRoutingValidationCoordinator contentRoutingValidationCoordinator,
            IServiceProvider serviceProvider
            )
        {
            _entries = entries;
            _options = options.Value;
            _liquidTemplateManager = liquidTemplateManager;
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
            _tagCache = tagCache;
            _contentRoutingValidationCoordinator = contentRoutingValidationCoordinator;
            _serviceProvider = serviceProvider;
        }

        public override async Task UpdatedAsync(UpdateContentContext context, ContainerRoutePart part)
        {
            await SetContainerPathFromPattern(part);

            // Validate contained content item routes if container has valid path.
            if (String.IsNullOrWhiteSpace(part.Path))
            {
                return;
            }

            _contentManager ??= _serviceProvider.GetRequiredService<IContentManager>();

            var containedAspect = await _contentManager.PopulateAspectAsync<ContainedContentItemsAspect>(context.ContentItem);

            // Build the entries for this content item to evaluate for duplicates.
            var entries = new List<ContainerRouteEntry>();
            await PopulateContainedContentItemRoutes(entries, part.ContentItem.ContentItemId, containedAspect, context.ContentItem.Content as JObject, part.Path);

            await ValidateContainedContentItemRoutes(entries, part.ContentItem.ContentItemId, containedAspect, context.ContentItem.Content as JObject, part.Path);

        }

        //TODO validate to support absolute paths.
        public async override Task CloningAsync(CloneContentContext context, ContainerRoutePart part)
        {
            var clonedPart = context.CloneContentItem.As<ContainerRoutePart>();
            clonedPart.Path = await GenerateUniqueContainerPathAsync(part.Path, clonedPart);
            clonedPart.SetHomepage = false;
            clonedPart.Apply();
        }

        public override async Task PublishedAsync(PublishContentContext context, ContainerRoutePart part)
        {
            // Add parent content item path, and children, only if parent has a valid path.
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                var entries = new List<ContainerRouteEntry>
                {
                    new ContainerRouteEntry(part.ContentItem.ContentItemId, part.Path)
                };

                _contentManager ??= _serviceProvider.GetRequiredService<IContentManager>();

                var containedAspect = await _contentManager.PopulateAspectAsync<ContainedContentItemsAspect>(context.PublishingItem);

                await PopulateContainedContentItemRoutes(entries, part.ContentItem.ContentItemId, containedAspect, context.PublishingItem.Content as JObject, part.Path);

                _entries.AddEntries(entries);
            }

            if (part.SetHomepage)
            {
                var site = await _siteService.LoadSiteSettingsAsync();

                if (site.HomeRoute == null)
                {
                    site.HomeRoute = new RouteValueDictionary();
                }

                var homeRoute = site.HomeRoute;

                foreach (var entry in _options.GlobalRouteValues)
                {
                    homeRoute[entry.Key] = entry.Value;
                }

                homeRoute[_options.ContainerContentItemIdKey] = context.ContentItem.ContentItemId;

                // Once we too the flag into account we can dismiss it.
                part.SetHomepage = false;
                await _siteService.UpdateSiteSettingsAsync(site);
            }

            // Evict any dependent item from cache
            await RemoveTagAsync(part);
        }

        //TODO
        public override Task UnpublishedAsync(PublishContentContext context, ContainerRoutePart part)
        {
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                _entries.RemoveEntry(part.ContentItem.ContentItemId, part.Path);

                // Evict any dependent item from cache
                return RemoveTagAsync(part);
            }

            return Task.CompletedTask;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, ContainerRoutePart part)
        {
            return context.ForAsync<RouteHandlerAspect>(aspect =>
            {
                aspect.Path = part.Path;
                return Task.CompletedTask;
            });
        }

        //TODO
        public override Task RemovedAsync(RemoveContentContext context, ContainerRoutePart part)
        {
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                _entries.RemoveEntry(part.ContentItem.ContentItemId, part.Path);

                // Evict any dependent item from cache
                return RemoveTagAsync(part);
            }

            return Task.CompletedTask;
        }

        private Task RemoveTagAsync(ContainerRoutePart part)
        {
            return _tagCache.RemoveTagAsync($"slug:{part.Path}");
        }


        private async Task SetContainerPathFromPattern(ContainerRoutePart part)
        {
            // Compute the Path only if it's empty
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                return;
            }

            var pattern = GetPattern(part);

            if (!String.IsNullOrEmpty(pattern))
            {
                var model = new ContainerRoutePartViewModel()
                {
                    Path = part.Path,
                    ContainerRoutePart = part,
                    ContentItem = part.ContentItem
                };

                part.Path = await _liquidTemplateManager.RenderAsync(pattern, NullEncoder.Default, model,
                    scope => scope.SetValue("ContentItem", model.ContentItem));

                part.Path = part.Path.Replace("\r", String.Empty).Replace("\n", String.Empty);

                if (part.Path?.Length > ContainerRoutePartDisplay.MaxPathLength)
                {
                    part.Path = part.Path.Substring(0, ContainerRoutePartDisplay.MaxPathLength);
                }

                if (!await IsPathUniqueAsync(part.Path, part))
                {
                    part.Path = await GenerateUniqueContainerPathAsync(part.Path, part);
                }

                part.Apply();
            }
        }

        /// <summary>
        /// Get the pattern from the ContainerRoutePartSettings property for its type
        /// </summary>
        private string GetPattern(ContainerRoutePart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "ContainerRoutePart"));
            var pattern = contentTypePartDefinition.GetSettings<ContainerRoutePartSettings>().Pattern;

            return pattern;
        }

        private async Task<string> GenerateUniqueContainerPathAsync(string path, ContainerRoutePart context)
        {
            var version = 1;
            var unversionedPath = path;

            var versionSeparatorPosition = path.LastIndexOf('-');
            if (versionSeparatorPosition > -1 && int.TryParse(path.Substring(versionSeparatorPosition).TrimStart('-'), out version))
            {
                unversionedPath = path.Substring(0, versionSeparatorPosition);
            }

            while (true)
            {
                // Unversioned length + seperator char + version length.
                var quantityCharactersToTrim = unversionedPath.Length + 1 + version.ToString().Length - ContainerRoutePartDisplay.MaxPathLength;
                if (quantityCharactersToTrim > 0)
                {
                    unversionedPath = unversionedPath.Substring(0, unversionedPath.Length - quantityCharactersToTrim);
                }

                var versionedPath = $"{unversionedPath}-{version++}";
                if (await IsPathUniqueAsync(versionedPath, context))
                {
                    return versionedPath;
                }
            }
        }

        private async Task<bool> IsPathUniqueAsync(string path, ContainerRoutePart context)
        {
            return await _contentRoutingValidationCoordinator.IsPathUniqueAsync(path, context.ContentItem.ContentItemId);
        }

        private async Task PopulateContainedContentItemRoutes(List<ContainerRouteEntry> entries, string containerContentItemId, ContainedContentItemsAspect containedContentItemsAspect, JObject content, string basePath)
        {
            foreach (var accessor in containedContentItemsAspect.Accessors)
            {
                var jItems = accessor.Invoke(content);

                foreach (JObject jItem in jItems)
                {
                    var contentItem = jItem.ToObject<ContentItem>();
                    var handlerAspect = await _contentManager.PopulateAspectAsync<RouteHandlerAspect>(contentItem);

                    if (handlerAspect.IsRouteable)
                    {
                        var path = handlerAspect.Path;
                        if (handlerAspect.IsRelative)
                        {
                            path = (basePath.EndsWith('/') ? basePath : basePath + '/') + handlerAspect.Path;
                        }

                        entries.Add(new ContainerRouteEntry(containerContentItemId, path, contentItem.ContentItemId, jItem.Path));
                    }

                    var itemBasePath = (basePath.EndsWith('/') ? basePath : basePath + '/') + handlerAspect.Path;
                    var childrenAspect = await _contentManager.PopulateAspectAsync<ContainedContentItemsAspect>(contentItem);
                    await PopulateContainedContentItemRoutes(entries, containerContentItemId, childrenAspect, jItem, itemBasePath);
                }
            }
        }

        private async Task ValidateContainedContentItemRoutes(List<ContainerRouteEntry> entries, string containerContentItemId, ContainedContentItemsAspect containedContentItemsAspect, JObject content, string basePath)
        {
            foreach (var accessor in containedContentItemsAspect.Accessors)
            {
                var jItems = accessor.Invoke(content);

                foreach (JObject jItem in jItems)
                {
                    var contentItem = jItem.ToObject<ContentItem>();
                    var routeHandlerPart = contentItem.As<RouteHandlerPart>();

                    // This is only relevant if the content items have a routehandlerpart.
                    if (routeHandlerPart != null && routeHandlerPart.IsRoutable)
                    {
                        var path = string.Empty;
                        if (routeHandlerPart.IsRelative)
                        {
                            var currentItemBasePath = basePath.EndsWith('/') ? basePath : basePath + '/';
                            path = currentItemBasePath + routeHandlerPart.Path;
                            if (!IsContainedRelativePathUnique(entries, path, routeHandlerPart))
                            {
                                path = GenerateContainedUniquePath(entries, path, routeHandlerPart);
                                // Remove base path and update part path.
                                routeHandlerPart.Path = path.Substring(currentItemBasePath.Length);
                                routeHandlerPart.Apply();

                                // Merge because we have disconnected the content item from it's json owner.
                                jItem.Merge(contentItem.Content, new JsonMergeSettings
                                {
                                    MergeArrayHandling = MergeArrayHandling.Replace,
                                    MergeNullValueHandling = MergeNullValueHandling.Merge
                                });
                            }

                            path = path.Substring(currentItemBasePath.Length);
                        }
                        else
                        {
                            // TODO Support absolute path?
                            // Check path with other content for uniqueness.
                        }

                        var containedItemBasePath = (basePath.EndsWith('/') ? basePath : basePath + '/') + path;
                        var childrenAspect = await _contentManager.PopulateAspectAsync<ContainedContentItemsAspect>(contentItem);
                        await ValidateContainedContentItemRoutes(entries, containerContentItemId, childrenAspect, jItem, containedItemBasePath);
                    }
                }
            }
        }

        private bool IsContainedRelativePathUnique(List<ContainerRouteEntry> entries, string path, RouteHandlerPart context)
        {
            var result = !entries.Any(e => context.ContentItem.ContentItemId != e.ContainedContentItemId && String.Equals(e.Path, path, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        private string GenerateContainedUniquePath(List<ContainerRouteEntry> entries, string path, RouteHandlerPart context)
        {
            var version = 1;
            var unversionedPath = path;

            var versionSeparatorPosition = path.LastIndexOf('-');
            if (versionSeparatorPosition > -1 && int.TryParse(path.Substring(versionSeparatorPosition).TrimStart('-'), out version))
            {
                unversionedPath = path.Substring(0, versionSeparatorPosition);
            }

            while (true)
            {
                // Unversioned length + seperator char + version length.
                var quantityCharactersToTrim = unversionedPath.Length + 1 + version.ToString().Length - ContainerRoutePartDisplay.MaxPathLength;
                if (quantityCharactersToTrim > 0)
                {
                    unversionedPath = unversionedPath.Substring(0, unversionedPath.Length - quantityCharactersToTrim);
                }

                var versionedPath = $"{unversionedPath}-{version++}";
                if (IsContainedRelativePathUnique(entries, versionedPath, context))
                {
                    var entry = entries.FirstOrDefault(e => e.ContainedContentItemId == context.ContentItem.ContentItemId);
                    entry.Path = versionedPath;
                    return versionedPath;
                }
            }
        }

    }
}
