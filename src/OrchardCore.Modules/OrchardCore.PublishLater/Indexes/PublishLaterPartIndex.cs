using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using OrchardCore.PublishLater.Models;
using YesSql.Indexes;

namespace OrchardCore.PublishLater.Indexes
{
    public class PublishLaterPartIndex : MapIndex
    {
        public DateTime? ScheduledPublishUtc { get; set; }
    }

    public class PublishLaterPartIndexProvider : IndexProvider<ContentItem>, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private IContentDefinitionManager _contentDefinitionManager;
        private HashSet<string> _ignoredTypes;

        public PublishLaterPartIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<PublishLaterPartIndex>()
                .Map(contentItem =>
                {
                    var publishLaterPart = contentItem.As<PublishLaterPart>();
                    if (publishLaterPart == null || !publishLaterPart.ScheduledPublishUtc.HasValue)
                    {
                        return null;
                    }

                    // Remove index for items that are already published or not the latest version
                    if (contentItem.Published || !contentItem.Latest)
                    {
                        return null;
                    }

                    // Can we safely ignore this content item?
                    if (_ignoredTypes != null && _ignoredTypes.Contains(contentItem.ContentType))
                    {
                        contentItem.Remove<PublishLaterPart>();
                        return null;
                    }

                    // Lazy initialization because of ISession cyclic dependency
                    _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                    // Search for AliasPart
                    var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                    // Validate that the content definition contains an PublishLaterPart.
                    // This prevents indexing parts that have been removed from the type definition, but are still present in the elements.
                    if (contentTypeDefinition == null || !contentTypeDefinition.Parts.Any(ctpd => ctpd.Name == nameof(PublishLaterPart)))
                    {
                        _ignoredTypes ??= new HashSet<string>();
                        _ignoredTypes.Add(contentItem.ContentType);
                        contentItem.Remove<PublishLaterPart>();

                        return null;
                    }

                    // wouldn't it be better to do valid types?

                    return new PublishLaterPartIndex
                    {
                        ScheduledPublishUtc = publishLaterPart.ScheduledPublishUtc,
                    };
                });
        }
    }
}
