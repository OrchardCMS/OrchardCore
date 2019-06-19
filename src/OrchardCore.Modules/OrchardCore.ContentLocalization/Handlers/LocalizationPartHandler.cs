using System;
using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentLocalization.Handlers
{
    public class LocalizationPartHandler : ContentPartHandler<LocalizationPart>
    {
        private readonly ILocalizationEntries _entries;

        public LocalizationPartHandler(ILocalizationEntries entries)
        {
            _entries = entries;
        }

        public override Task PublishedAsync(PublishContentContext context, LocalizationPart part)
        {
            if (!String.IsNullOrWhiteSpace(part.LocalizationSet))
            {
                _entries.AddEntry(new LocalizationEntry()
                {
                    ContentItemId = part.ContentItem.ContentItemId,
                    LocalizationSet = part.LocalizationSet,
                    Culture = part.Culture.ToLowerInvariant()
                });
            }

            return Task.CompletedTask;
        }

        public override Task UnpublishedAsync(PublishContentContext context, LocalizationPart part)
        {
            _entries.RemoveEntry(new LocalizationEntry()
            {
                ContentItemId = part.ContentItem.ContentItemId,
                LocalizationSet = part.LocalizationSet,
                Culture = part.Culture.ToLowerInvariant()
            });

            return Task.CompletedTask;
        }

        public override Task RemovedAsync(RemoveContentContext context, LocalizationPart part)
        {
            _entries.RemoveEntry(new LocalizationEntry()
            {
                ContentItemId = part.ContentItem.ContentItemId,
                LocalizationSet = part.LocalizationSet,
                Culture = part.Culture.ToLowerInvariant()
            });

            return Task.CompletedTask;
        }
    }
}
