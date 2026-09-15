using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Cors;

/// <summary>
/// The name of the breadcrumb rendered by the CORS screen. It is named to avoid the framework's
/// <c>Microsoft.AspNetCore.Cors.Infrastructure.CorsConstants</c>.
/// </summary>
public static class CorsBreadcrumbConstants
{
    /// <summary>
    /// The breadcrumb of the CORS screen.
    /// </summary>
    public const string Settings = "CorsSettings";
}

/// <summary>
/// Describes the breadcrumb trail of the CORS screen.
/// </summary>
public sealed class CorsBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public CorsBreadcrumbProvider(IStringLocalizer<CorsBreadcrumbProvider> stringLocalizer)
        : base(CorsBreadcrumbConstants.Settings)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["CORS Settings"], item => item.Id("Cors"));

        return ValueTask.CompletedTask;
    }
}
