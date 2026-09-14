using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Features;

/// <summary>
/// Describes the breadcrumb trail of the features screen.
/// </summary>
public sealed class FeaturesBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public FeaturesBreadcrumbProvider(IStringLocalizer<FeaturesBreadcrumbProvider> stringLocalizer)
        : base(FeaturesConstants.Breadcrumb)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["Features"], item => item.Id("Features"));

        return ValueTask.CompletedTask;
    }
}
