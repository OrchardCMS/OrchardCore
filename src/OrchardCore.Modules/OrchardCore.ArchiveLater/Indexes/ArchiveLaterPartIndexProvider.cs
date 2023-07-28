using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ArchiveLater.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using YesSql.Indexes;

namespace OrchardCore.ArchiveLater.Indexes;

public class ArchiveLaterPartIndexProvider : ContentHandlerBase, IIndexProvider, IScopedIndexProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HashSet<string> _partRemoved = new();
    private IContentDefinitionManager _contentDefinitionManager;

    public ArchiveLaterPartIndexProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override Task UpdatedAsync(UpdateContentContext context)
    {
        var part = context.ContentItem.As<ArchiveLaterPart>();

        if (part != null)
        {
            _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (!contentTypeDefinition.Parts.Any(pd => pd.Name == nameof(ArchiveLaterPart)))
            {
                context.ContentItem.Remove<ArchiveLaterPart>();
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
        context
            .For<ArchiveLaterPartIndex>()
            .When(contentItem => contentItem.Has<ArchiveLaterPart>() || _partRemoved.Contains(contentItem.ContentItemId))
            .Map(contentItem =>
            {
                if (!contentItem.Published || !contentItem.Latest)
                {
                    return null;
                }

                var part = contentItem.As<ArchiveLaterPart>();
                if (part == null || !part.ScheduledArchiveUtc.HasValue)
                {
                    return null;
                }

                return new ArchiveLaterPartIndex
                {
                    ContentItemId = part.ContentItem.ContentItemId,
                    ScheduledArchiveDateTimeUtc = part.ScheduledArchiveUtc,
                    Published = part.ContentItem.Published,
                    Latest = part.ContentItem.Latest
                };
            });
    }
}
