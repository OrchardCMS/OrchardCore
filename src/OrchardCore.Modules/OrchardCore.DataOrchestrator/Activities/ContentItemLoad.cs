using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Creates or updates Orchard Core content items from pipeline data.
/// </summary>
public sealed class ContentItemLoad : EtlLoadActivity
{
    public override string Name => nameof(ContentItemLoad);

    public override string DisplayText => "Content Item";

    public string ContentType
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public override IEnumerable<EtlOutcome> GetPossibleOutcomes()
    {
        return [new EtlOutcome("Done"), new EtlOutcome("Failed")];
    }

    public override async Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context)
    {
        var contentType = ContentType;

        if (string.IsNullOrEmpty(contentType))
        {
            return EtlActivityResult.Failure("ContentItem load requires a 'contentType' configuration.");
        }

        var data = context.DataStream;
        if (data == null)
        {
            return EtlActivityResult.Failure("No data stream available.");
        }

        var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
        var logger = context.ServiceProvider.GetRequiredService<ILogger<ContentItemLoad>>();

        var loaded = 0;
        var failed = 0;

        await foreach (var record in data.WithCancellation(context.CancellationToken))
        {
            try
            {
                var contentItemId = record["ContentItemId"]?.GetValue<string>();
                ContentItem item;

                if (!string.IsNullOrEmpty(contentItemId))
                {
                    item = await contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);
                    item ??= await contentManager.NewAsync(contentType);
                }
                else
                {
                    item = await contentManager.NewAsync(contentType);
                }

                var itemJson = JConvert.SerializeObject(item);
                var itemNode = JsonNode.Parse(itemJson) as JsonObject;

                if (itemNode is not null)
                {
                    foreach (var property in record)
                    {
                        if (property.Key is "ContentItemId" or "ContentType")
                        {
                            continue;
                        }

                        itemNode[property.Key] = property.Value?.DeepClone();
                    }

                    var merged = JConvert.DeserializeObject<ContentItem>(itemNode.ToJsonString());
                    if (merged is not null)
                    {
                        await contentManager.CreateAsync(merged, VersionOptions.Draft);
                        await contentManager.PublishAsync(merged);
                        loaded++;
                    }
                }
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogError(ex, "Failed to create/update content item in ETL pipeline.");
            }
        }

        logger.LogInformation("ETL ContentItem load completed: {Loaded} loaded, {Failed} failed.", loaded, failed);

        return failed > 0
            ? EtlActivityResult.Failure($"{failed} record(s) failed to load.")
            : Outcomes("Done");
    }
}
