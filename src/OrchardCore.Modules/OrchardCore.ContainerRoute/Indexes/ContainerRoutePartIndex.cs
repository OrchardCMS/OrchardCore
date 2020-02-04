using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContainerRoute.Models;
using OrchardCore.ContainerRoute.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Data;
using YesSql.Indexes;

namespace OrchardCore.ContainerRoute.Indexes
{
    public class ContainerRoutePartIndex : MapIndex
    {
        public string ContainerContentItemId { get; set; }
        public string ContainedContentItemId { get; set; }
        public string JsonPath { get; set; }
        public string Path { get; set; }
        public bool Published { get; set; }
        public bool Latest { get; set; }
    }

    // A similar mechanism could be used to create a HandlerIndex.
    // But only if the items have a contained aspect?
    public class ContainerRoutePartIndexProvider : IndexProvider<ContentItem>, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private IContentManager _contentManager;

        public ContainerRoutePartIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<ContainerRoutePartIndex>()
                .Map(async contentItem =>
                {
                    var part = contentItem.As<ContainerRoutePart>();

                    if (part == null)
                    {
                        return null;
                    }

                    var containerPath = part?.Path;

                    if (String.IsNullOrEmpty(containerPath) || !(contentItem.Published || contentItem.Latest))
                    {
                        return null;
                    }

                    var results = new List<ContainerRoutePartIndex>
                    {
                        new ContainerRoutePartIndex
                        {
                            ContainerContentItemId = contentItem.ContentItemId,
                            Path = containerPath,
                            Published = contentItem.Published,
                            Latest = contentItem.Latest
                        }
                    };

                    _contentManager ??= _serviceProvider.GetRequiredService<IContentManager>();

                    var containedContentItemsAspect = await _contentManager.PopulateAspectAsync<ContainedContentItemsAspect>(contentItem);

                    await PopulateContainedContentItemIndexes(results, contentItem, containedContentItemsAspect, contentItem.Content as JObject, part.Path);

                    return results;
                });
        }

        private async Task PopulateContainedContentItemIndexes(List<ContainerRoutePartIndex> results,
            ContentItem containerContentItem,
            ContainedContentItemsAspect containedContentItemsAspect,
            JObject content,
            string basePath)
        {
            foreach (var accessor in containedContentItemsAspect.Accessors)
            {
                var items = accessor.Invoke(content);

                foreach (JObject jItem in items)
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

                        results.Add(new ContainerRoutePartIndex
                        {
                            ContainerContentItemId = containerContentItem.ContentItemId,
                            ContainedContentItemId = contentItem.ContentItemId,
                            Path = path,
                            JsonPath = jItem.Path,
                            Published = containerContentItem.Published,
                            Latest = containerContentItem.Latest
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