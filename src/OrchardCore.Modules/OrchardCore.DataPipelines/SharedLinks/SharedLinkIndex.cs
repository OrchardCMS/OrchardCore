using YesSql.Indexes;

namespace OrchardCore.DataPipelines.SharedLinks;

/// <summary>
/// Indexes the shared links, to find them by identifier, list and search them, and delete the expired ones.
/// </summary>
public sealed class SharedLinkIndex : MapIndex
{
    public const int MaxIdLength = 26;
    public const int MaxKindLength = 100;
    public const int MaxTitleLength = 255;

    public long DocumentId { get; set; }

    public string LinkId { get; set; }

    public string Kind { get; set; }

    public string Title { get; set; }

    public string SourceName { get; set; }

    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the link stopped or stops working: when it was revoked, or when it expires.
    /// </summary>
    public DateTime ExpiresUtc { get; set; }
}

public sealed class SharedLinkIndexProvider : IndexProvider<SharedLink>
{
    public override void Describe(DescribeContext<SharedLink> context)
    {
        context.For<SharedLinkIndex>()
            .Map(link => new SharedLinkIndex
            {
                LinkId = link.LinkId,
                Kind = link.Kind,
                Title = Truncate(link.Title),
                SourceName = Truncate(link.SourceName),
                CreatedUtc = link.CreatedUtc,
                ExpiresUtc = link.RevokedUtc ?? link.ExpiresUtc,
            });
    }

    private static string Truncate(string value)
    {
        return value?.Length > SharedLinkIndex.MaxTitleLength ? value[..SharedLinkIndex.MaxTitleLength] : value;
    }
}
