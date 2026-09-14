using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Settings;

/// <summary>
/// The name of the breadcrumb rendered by the settings screen.
/// </summary>
public static class SettingsConstants
{
    /// <summary>
    /// The breadcrumb of the settings screen.
    /// </summary>
    public const string List = "GeneralSettings";
}

/// <summary>
/// Describes the breadcrumb trail of the settings screen.
/// </summary>
public sealed class SettingsBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public SettingsBreadcrumbProvider(IStringLocalizer<SettingsBreadcrumbProvider> stringLocalizer)
        : base(SettingsConstants.List)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["Settings"], item => item.Id("Settings"));

        return ValueTask.CompletedTask;
    }
}
