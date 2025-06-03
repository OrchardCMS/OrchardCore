using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.BackgroundJobs;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Indexing.Core.Handlers;

public sealed class ContentIndexEntryHandler : IndexEntityHandlerBase
{
    private readonly IServiceProvider _serviceProvider;

    internal readonly IStringLocalizer S;

    public ContentIndexEntryHandler(
        IServiceProvider serviceProvider,
        IStringLocalizer<ContentIndexEntryHandler> stringLocalizer)
    {
        _serviceProvider = serviceProvider;
        S = stringLocalizer;
    }

    public override Task InitializingAsync(InitializingContext<IndexEntity> context)
        => PopulateAsync(context.Model, context.Data);

    private static Task PopulateAsync(IndexEntity index, JsonNode data)
    {
        if (index.Type != IndexingConstants.ContentsIndexSource)
        {
            return Task.CompletedTask;
        }

        var metadata = index.As<ContentIndexMetadata>();

        var indexLatest = data[nameof(metadata.IndexLatest)]?.GetValue<bool>();

        if (indexLatest.HasValue)
        {
            metadata.IndexLatest = indexLatest.Value;
        }

        var culture = data[nameof(metadata.Culture)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(culture))
        {
            metadata.Culture = culture;
        }

        var indexContentTypes = data[nameof(metadata.IndexedContentTypes)]?.AsArray();

        if (indexContentTypes is not null)
        {
            var items = new HashSet<string>();

            foreach (var indexContentType in indexContentTypes)
            {
                var value = indexContentType.GetValue<string>();

                if (!string.IsNullOrEmpty(value))
                {
                    items.Add(value);
                }
            }

            metadata.IndexedContentTypes = items.ToArray();
        }

        index.Put(metadata);

        return Task.CompletedTask;
    }

    private readonly HashSet<string> _resetIndexIds = [];

    public override async Task ResetAsync(IndexEntityResetContext context)
    {
        if (context.Index.Type != IndexingConstants.ContentsIndexSource)
        {
            return;
        }

        var documentIndexManager = _serviceProvider.GetKeyedService<IDocumentIndexManager>(context.Index.ProviderName);

        if (documentIndexManager is null)
        {
            return;
        }

        // Cache the indexes per-request to avoid having to reset the same index multiple times.
        if (_resetIndexIds.Add(context.Index.Id))
        {
            await documentIndexManager.SetLastTaskIdAsync(context.Index, 0);
        }
    }

    public override Task SynchronizedAsync(IndexEntitySynchronizedContext context)
    {
        if (context.Index.Type != IndexingConstants.ContentsIndexSource)
        {
            return Task.CompletedTask;
        }

        return HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("sync-content-items-indexing", context.Index, async (scope, index) =>
        {
            var indexingService = scope.ServiceProvider.GetRequiredService<ContentIndexingService>();
            await indexingService.ProcessContentItemsAsync([index]);
        });
    }

    public override Task ExportingAsync(IndexEntityExportingContext context)
    {
        if (context.Index.Type != IndexingConstants.ContentsIndexSource)
        {
            return Task.CompletedTask;
        }

        var metadata = context.Index.As<ContentIndexMetadata>();

        context.Data["IndexLatest"] = metadata.IndexLatest;
        context.Data["Culture"] = metadata.Culture;

        var jsonArray = new JsonArray();

        foreach (var indexedContentType in metadata.IndexedContentTypes)
        {
            jsonArray.Add(indexedContentType);
        }

        context.Data["IndexedContentTypes"] = jsonArray;

        return Task.CompletedTask;
    }

    public override Task ValidatingAsync(ValidatingContext<IndexEntity> context)
    {
        if (context.Model.Type != IndexingConstants.ContentsIndexSource)
        {
            return Task.CompletedTask;
        }

        var metadata = context.Model.As<ContentIndexMetadata>();

        if (metadata.IndexedContentTypes is null || metadata.IndexedContentTypes.Length == 0)
        {
            context.Result.Fail(new ValidationResult(S["At least one content type must be selected."]));
        }

        return Task.CompletedTask;
    }
}
