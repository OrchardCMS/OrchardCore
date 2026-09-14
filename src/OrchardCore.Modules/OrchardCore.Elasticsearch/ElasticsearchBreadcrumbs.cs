using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Elasticsearch;

/// <summary>
/// The name of the breadcrumb rendered by the elasticsearch screen.
/// </summary>
public static class ElasticsearchBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the elasticsearch screen.
    /// </summary>
    public const string Query = "ElasticsearchQuery";
}

/// <summary>
/// Describes the breadcrumb trail of the elasticsearch screen.
/// </summary>
public sealed class ElasticsearchBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public ElasticsearchBreadcrumbProvider(IStringLocalizer<ElasticsearchBreadcrumbProvider> stringLocalizer)
        : base(ElasticsearchBreadcrumbs.Query)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["Elasticsearch Query"], item => item.Id("Query"));

        return ValueTask.CompletedTask;
    }
}
