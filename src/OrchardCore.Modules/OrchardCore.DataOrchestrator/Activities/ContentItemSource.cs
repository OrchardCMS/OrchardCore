using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DataOrchestrator.Models;
using YesSql;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Extracts published content items of a specified content type.
/// </summary>
public sealed class ContentItemSource : EtlSourceActivity
{
    public const string PublishedVersionScope = "Published";
    public const string LatestVersionScope = "Latest";
    public const string AllVersionsScope = "AllVersions";

    public override string Name => nameof(ContentItemSource);

    public override string DisplayText => "Content Items";

    public string ContentType
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string VersionScope
    {
        get => GetProperty(() => PublishedVersionScope);
        set => SetProperty(value);
    }

    public string Owner
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public DateTime? CreatedUtcFrom
    {
        get => GetProperty<DateTime?>();
        set => SetProperty(value);
    }

    public DateTime? CreatedUtcTo
    {
        get => GetProperty<DateTime?>();
        set => SetProperty(value);
    }

    public override IEnumerable<EtlOutcome> GetPossibleOutcomes()
    {
        return [new EtlOutcome("Done")];
    }

    public override Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context)
    {
        context.DataStream = ExtractAsync(
            context.ServiceProvider,
            ContentType,
            VersionScope,
            Owner,
            CreatedUtcFrom,
            CreatedUtcTo,
            context.CancellationToken);

        return Task.FromResult(Outcomes("Done"));
    }

    private static async IAsyncEnumerable<JsonObject> ExtractAsync(
        IServiceProvider serviceProvider,
        string contentType,
        string versionScope,
        string owner,
        DateTime? createdUtcFrom,
        DateTime? createdUtcTo,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            yield break;
        }

        var session = serviceProvider.GetRequiredService<ISession>();
        var query = versionScope switch
        {
            LatestVersionScope => session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentType && x.Latest),
            AllVersionsScope => session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentType),
            _ => session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentType && x.Published),
        };

        var items = await query.ListAsync(cancellationToken);

        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.IsNullOrWhiteSpace(owner) && !string.Equals(item.Owner, owner, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (createdUtcFrom.HasValue && item.CreatedUtc < createdUtcFrom.Value)
            {
                continue;
            }

            if (createdUtcTo.HasValue && item.CreatedUtc > createdUtcTo.Value)
            {
                continue;
            }

            var json = JConvert.SerializeObject(item);
            var node = JsonNode.Parse(json);

            if (node is JsonObject obj)
            {
                yield return obj;
            }
        }
    }
}
