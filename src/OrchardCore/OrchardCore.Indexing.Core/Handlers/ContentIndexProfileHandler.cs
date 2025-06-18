using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Indexing.Core.Handlers;

public sealed class ContentIndexProfileHandler : IndexProfileHandlerBase
{
    private readonly IServiceProvider _serviceProvider;

    private readonly HashSet<string> _resetIndexIds = [];

    internal readonly IStringLocalizer S;

    public ContentIndexProfileHandler(
        IServiceProvider serviceProvider,
        IStringLocalizer<ContentIndexProfileHandler> stringLocalizer)
    {
        _serviceProvider = serviceProvider;
        S = stringLocalizer;
    }

    public override Task InitializingAsync(InitializingContext<IndexProfile> context)
        => PopulateAsync(context.Model, context.Data);

    public override async Task ResetAsync(IndexProfileResetContext context)
    {
        if (!string.Equals(IndexingConstants.ContentsIndexSource, context.IndexProfile.Type, StringComparison.Ordinal))
        {
            return;
        }

        var documentIndexManager = _serviceProvider.GetKeyedService<IDocumentIndexManager>(context.IndexProfile.ProviderName);

        if (documentIndexManager is null)
        {
            return;
        }

        // Cache the indexes per-request to avoid having to reset the same index multiple times.
        if (_resetIndexIds.Add(context.IndexProfile.Id))
        {
            await documentIndexManager.SetLastTaskIdAsync(context.IndexProfile, 0);
        }
    }

    public override Task SynchronizedAsync(IndexProfileSynchronizedContext context)
    {
        if (context.IndexProfile.Type != IndexingConstants.ContentsIndexSource)
        {
            return Task.CompletedTask;
        }

        ShellScope.AddDeferredTask((scope) =>
        {
            var indexingService = scope.ServiceProvider.GetRequiredService<ContentIndexingService>();

            return indexingService.ProcessRecordsAsync([context.IndexProfile.Id]);
        });

        return Task.CompletedTask;
    }

    public override Task ExportingAsync(IndexProfileExportingContext context)
    {
        if (context.IndexProfile.Type != IndexingConstants.ContentsIndexSource)
        {
            return Task.CompletedTask;
        }

        var metadata = context.IndexProfile.As<ContentIndexMetadata>();

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

    public override Task ValidatingAsync(ValidatingContext<IndexProfile> context)
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

    private static Task PopulateAsync(IndexProfile indexProfile, JsonNode data)
    {
        if (!string.Equals(IndexingConstants.ContentsIndexSource, indexProfile.Type, StringComparison.Ordinal))
        {
            return Task.CompletedTask;
        }

        var metadata = indexProfile.As<ContentIndexMetadata>();

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

        indexProfile.Put(metadata);

        return Task.CompletedTask;
    }
}
