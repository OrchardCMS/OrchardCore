using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Lucene;

/// <summary>
/// The name of the breadcrumb rendered by the lucene screen.
/// </summary>
public static class LuceneBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the lucene screen.
    /// </summary>
    public const string Query = "LuceneQuery";
}

/// <summary>
/// Describes the breadcrumb trail of the lucene screen.
/// </summary>
public sealed class LuceneBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public LuceneBreadcrumbProvider(IStringLocalizer<LuceneBreadcrumbProvider> stringLocalizer)
        : base(LuceneBreadcrumbs.Query)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["Lucene Query"], item => item.Id("Query"));

        return ValueTask.CompletedTask;
    }
}
