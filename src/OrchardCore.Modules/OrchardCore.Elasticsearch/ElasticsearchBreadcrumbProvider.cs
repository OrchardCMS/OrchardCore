using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Elasticsearch;

/// <summary>
/// Describes the breadcrumb trail of the Elasticsearch query screen.
/// </summary>
public sealed class ElasticsearchBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public ElasticsearchBreadcrumbProvider(IStringLocalizer<ElasticsearchBreadcrumbProvider> stringLocalizer)
        : base(ElasticsearchConstants.Query)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["Elasticsearch Query"], item => item.Id("Query"));

        return ValueTask.CompletedTask;
    }
}
