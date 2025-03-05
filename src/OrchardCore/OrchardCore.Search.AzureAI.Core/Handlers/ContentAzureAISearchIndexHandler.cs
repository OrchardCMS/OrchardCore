using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.BackgroundJobs;
using OrchardCore.Entities;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Handlers;

public sealed class ContentAzureAISearchIndexHandler : AzureAISearchIndexSettingsHandlerBase
{
    private readonly IStringLocalizer S;

    public ContentAzureAISearchIndexHandler(IStringLocalizer<ContentAzureAISearchIndexHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task InitializingAsync(AzureAISearchIndexSettingsInitializingContext context)
        => PopulateAsync(context.Settings, context.Data);

    private static Task PopulateAsync(AzureAISearchIndexSettings settings, JsonNode data)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var metadata = settings.As<ContentIndexMetadata>();

        var indexLatest = data[nameof(ContentIndexMetadata.IndexLatest)]?.GetValue<bool>();

        if (indexLatest.HasValue)
        {
            metadata.IndexLatest = indexLatest.Value;
        }

        var culture = data[nameof(ContentIndexMetadata.Culture)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(culture))
        {
            metadata.Culture = culture;
        }

        var indexContentTypes = data[nameof(ContentIndexMetadata.IndexedContentTypes)]?.AsArray();

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

        settings.Put(metadata);

        return Task.CompletedTask;
    }

    public override Task ValidatingAsync(AzureAISearchIndexSettingsValidatingContext context)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, context.Settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var metadata = context.Settings.As<ContentIndexMetadata>();

        if (metadata.IndexedContentTypes is null || metadata.IndexedContentTypes.Length == 0)
        {
            context.Result.Fail(new ValidationResult(S["At least one content type must be selected."]));
        }

        return Task.CompletedTask;
    }

    public override Task ResetAsync(AzureAISearchIndexSettingsResetContext context)
    {
        context.Settings.SetLastTaskId(0);

        return Task.CompletedTask;
    }

    public override Task SynchronizedAsync(AzureAISearchIndexSettingsSynchronizedContext context)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, context.Settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        return HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("sync-content-items-azure-ai", context.Settings.IndexName, async (scope, indexName) =>
        {
            var indexingService = scope.ServiceProvider.GetRequiredService<AzureAISearchIndexingService>();
            await indexingService.ProcessContentItemsAsync(indexName);
        });
    }

    public override Task ExportingAsync(AzureAISearchIndexSettingsExportingContext context)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, context.Settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var metadata = context.Settings.As<ContentIndexMetadata>();

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
}
