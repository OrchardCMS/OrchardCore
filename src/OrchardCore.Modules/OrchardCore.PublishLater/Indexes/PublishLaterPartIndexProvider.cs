using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using OrchardCore.PublishLater.Models;
using YesSql.Indexes;

namespace OrchardCore.PublishLater.Indexes;

public class PublishLaterPartIndexProvider : ContentHandlerBase, IIndexProvider, IScopedIndexProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HashSet<string> _partRemoved = new();
    private IContentDefinitionManager _contentDefinitionManager;

    public PublishLaterPartIndexProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override Task UpdatedAsync(UpdateContentContext context)
    {
        var part = context.ContentItem.As<PublishLaterPart>();

        // Validate that the content definition contains this part, this prevents indexing parts
        // that have been removed from the type definition, but are still present in the elements.            
        if (part != null)
        {
            // Lazy initialization because of ISession cyclic dependency.
            _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

            // Search for this part.
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (!contentTypeDefinition.Parts.Any(ctpd => ctpd.Name == nameof(PublishLaterPart)))
            {
                context.ContentItem.Remove<PublishLaterPart>();
                _partRemoved.Add(context.ContentItem.ContentItemId);
            }
        }

        return Task.CompletedTask;
    }

    public string CollectionName { get; set; }
    public Type ForType() => typeof(ContentItem);
    public void Describe(IDescriptor context) => Describe((DescribeContext<ContentItem>)context);

    public void Describe(DescribeContext<ContentItem> context)
    {
        context.For<PublishLaterPartIndex>()
            .When(contentItem => contentItem.Has<PublishLaterPart>() || _partRemoved.Contains(contentItem.ContentItemId))
            .Map(contentItem =>
            {
                // Remove index records of items that are already published or not the latest version.
                if (contentItem.Published || !contentItem.Latest)
                {
                    return null;
                }

                var part = contentItem.As<PublishLaterPart>();
                if (part == null || !part.ScheduledPublishUtc.HasValue)
                {
                    return null;
                }

                return new PublishLaterPartIndex
                {
                    ContentItemId = part.ContentItem.ContentItemId,
                    ScheduledPublishDateTimeUtc = part.ScheduledPublishUtc,
                    Published = part.ContentItem.Published,
                    Latest = part.ContentItem.Latest
                };
            });
    }
}
