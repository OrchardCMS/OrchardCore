using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Cors;

/// <summary>
/// The name of the breadcrumb rendered by the cors screen.
/// </summary>
public static class CorsBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the cors screen.
    /// </summary>
    public const string Settings = "CorsSettings";
}

/// <summary>
/// Describes the breadcrumb trail of the cors screen.
/// </summary>
public sealed class CorsBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public CorsBreadcrumbProvider(IStringLocalizer<CorsBreadcrumbProvider> stringLocalizer)
        : base(CorsBreadcrumbs.Settings)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["CORS Settings"], item => item.Id("Cors"));

        return ValueTask.CompletedTask;
    }
}
