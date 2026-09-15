using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Lucene;

/// <summary>
/// Describes the breadcrumb trail of the Lucene query screen.
/// </summary>
public sealed class LuceneBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public LuceneBreadcrumbProvider(IStringLocalizer<LuceneBreadcrumbProvider> stringLocalizer)
        : base(LuceneConstants.Query)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["Lucene Query"], item => item.Id("Query"));

        return ValueTask.CompletedTask;
    }
}
