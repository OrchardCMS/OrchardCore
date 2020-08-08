using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Data;
using YesSql.Indexes;

namespace OrchardCore.ContentManagement.Records
{
    public class AutoroutePartIndex : MapIndex
    {
        /// <summary>
        /// The container content item id.
        /// </summary>
        public string ContentItemId { get; set; }

        /// <summary>
        /// Route path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Whether this content item is published.
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Whether this content item is latest.
        /// </summary>
        public bool Latest { get; set; }

        /// <summary>
        /// Only used if content item is contained in a container.
        /// </summary>
        public string ContainedContentItemId { get; set; }

        /// <summary>
        /// Only used if the content item is contained in a container.
        /// </summary>
        public string JsonPath { get; set; }
    }

    public class AutoroutePartIndexProvider : IndexProvider<ContentItem>, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;

        private IContentManager _contentManager;

        public AutoroutePartIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<AutoroutePartIndex>()
                .Map(async contentItem =>
                {
                    var part = contentItem.As<AutoroutePart>();

                    if (part == null)
                    {
                        return null;
                    }

                    var path = part?.Path;
                    if (String.IsNullOrEmpty(path) || part.Disabled || !(contentItem.Published || contentItem.Latest))
                    {
                        return null;
                    }

                    var results = new List<AutoroutePartIndex>
                    {
                        new AutoroutePartIndex
                        {
                            ContentItemId = contentItem.ContentItemId,
                            Path = path,
                            Published = contentItem.Published,
                            Latest = contentItem.Latest
                        }
                    };

                    if (!part.RouteContainedItems)
                    {
                        return results;
                    }

                    _contentManager ??= _serviceProvider.GetRequiredService<IContentManager>();

                    var containedContentItemsAspect = await _contentManager.PopulateAspectAsync<ContainedContentItemsAspect>(contentItem);

                    await PopulateContainedContentItemIndexes(results, contentItem, containedContentItemsAspect, contentItem.Content as JObject, part.Path);

                    return results;
                });
        }

        private async Task PopulateContainedContentItemIndexes(List<AutoroutePartIndex> results, ContentItem containerContentItem, ContainedContentItemsAspect containedContentItemsAspect, JObject content, string basePath)
        {
            foreach (var accessor in containedContentItemsAspect.Accessors)
            {
                var items = accessor.Invoke(content);

                foreach (JObject jItem in items)
                {
                    var contentItem = jItem.ToObject<ContentItem>();
                    var handlerAspect = await _contentManager.PopulateAspectAsync<RouteHandlerAspect>(contentItem);

                    if (!handlerAspect.Disabled)
                    {
                        var path = handlerAspect.Path;
                        if (!handlerAspect.Absolute)
                        {
                            path = (basePath.EndsWith('/') ? basePath : basePath + '/') + handlerAspect.Path;
                        }

                        results.Add(new AutoroutePartIndex
                        {
                            ContentItemId = containerContentItem.ContentItemId,
                            Path = path,
                            Published = containerContentItem.Published,
                            Latest = containerContentItem.Latest,
                            ContainedContentItemId = contentItem.ContentItemId,
                            JsonPath = jItem.Path
                        });
                    }

                    var itemBasePath = (basePath.EndsWith('/') ? basePath : basePath + '/') + handlerAspect.Path;
                    var childrenAspect = await _contentManager.PopulateAspectAsync<ContainedContentItemsAspect>(contentItem);

                    await PopulateContainedContentItemIndexes(results, containerContentItem, childrenAspect, jItem, itemBasePath);
                }
            }
        }
    }
}
