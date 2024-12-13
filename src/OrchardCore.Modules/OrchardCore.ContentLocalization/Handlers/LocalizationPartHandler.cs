using System.Globalization;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Entities;
using OrchardCore.Localization;

namespace OrchardCore.ContentLocalization.Handlers;

public class LocalizationPartHandler : ContentPartHandler<LocalizationPart>
{
    private readonly ILocalizationEntries _entries;
    private readonly IIdGenerator _idGenerator;
    private readonly ILocalizationService _localizationService;

    public LocalizationPartHandler(
        ILocalizationEntries entries,
        IIdGenerator idGenerator,
        ILocalizationService localizationService)
    {
        _entries = entries;
        _idGenerator = idGenerator;
        _localizationService = localizationService;
    }

    public override async Task CreatingAsync(CreateContentContext context, LocalizationPart part)
    {
        if (string.IsNullOrEmpty(part.LocalizationSet))
        {
            context.ContentItem.Alter<LocalizationPart>(p =>
                p.LocalizationSet = _idGenerator.GenerateUniqueId()
            );
        }

        if (string.IsNullOrEmpty(part.Culture))
        {
            await context.ContentItem.AlterAsync<LocalizationPart>(async p =>
                p.Culture = await _localizationService.GetDefaultCultureAsync()
            );
        }
    }

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context, LocalizationPart part)
    {
        return context.ForAsync<CultureAspect>(cultureAspect =>
        {
            if (part.Culture != null)
            {
                cultureAspect.Culture = CultureInfo.GetCultureInfo(part.Culture);
                cultureAspect.HasCulture = true;
            }

            return Task.CompletedTask;
        });
    }

    public override Task PublishedAsync(PublishContentContext context, LocalizationPart part)
    {
        if (!string.IsNullOrWhiteSpace(part.LocalizationSet) && part.Culture != null)
        {
            // Update entries from the index table after the session is committed.
            return _entries.UpdateEntriesAsync();
        }

        return Task.CompletedTask;
    }

    public override Task UnpublishedAsync(PublishContentContext context, LocalizationPart part)
    {
        if (!string.IsNullOrWhiteSpace(part.LocalizationSet) && part.Culture != null)
        {
            // Update entries from the index table after the session is committed.
            return _entries.UpdateEntriesAsync();
        }

        return Task.CompletedTask;
    }

    public override Task RemovedAsync(RemoveContentContext context, LocalizationPart part)
    {
        if (!string.IsNullOrWhiteSpace(part.LocalizationSet) && part.Culture != null && context.NoActiveVersionLeft)
        {
            // Update entries from the index table after the session is committed.
            return _entries.UpdateEntriesAsync();
        }

        return Task.CompletedTask;
    }

    public override Task CloningAsync(CloneContentContext context, LocalizationPart part)
    {
        var clonedPart = context.CloneContentItem.As<LocalizationPart>();
        clonedPart.LocalizationSet = string.Empty;
        clonedPart.Apply();
        return Task.CompletedTask;
    }
}
