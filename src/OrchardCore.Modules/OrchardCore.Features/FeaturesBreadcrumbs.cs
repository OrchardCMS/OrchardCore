using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Features;

/// <summary>
/// The name of the breadcrumb rendered by the features screen.
/// </summary>
public static class FeaturesBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the features screen.
    /// </summary>
    public const string List = "Features";
}

/// <summary>
/// Describes the breadcrumb trail of the features screen.
/// </summary>
public sealed class FeaturesBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public FeaturesBreadcrumbProvider(IStringLocalizer<FeaturesBreadcrumbProvider> stringLocalizer)
        : base(FeaturesBreadcrumbs.List)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["Features"], item => item.Id("Features"));

        return ValueTask.CompletedTask;
    }
}
