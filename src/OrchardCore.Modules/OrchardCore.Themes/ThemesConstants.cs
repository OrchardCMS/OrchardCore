using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Themes;

/// <summary>
/// The name of the breadcrumb rendered by the themes screen.
/// </summary>
public static class ThemesConstants
{
    /// <summary>
    /// The breadcrumb of the themes screen.
    /// </summary>
    public const string List = "Themes";
}

/// <summary>
/// Describes the breadcrumb trail of the themes screen.
/// </summary>
public sealed class ThemesBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public ThemesBreadcrumbProvider(IStringLocalizer<ThemesBreadcrumbProvider> stringLocalizer)
        : base(ThemesConstants.List)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["Themes"], item => item.Id("Themes"));

        return ValueTask.CompletedTask;
    }
}
