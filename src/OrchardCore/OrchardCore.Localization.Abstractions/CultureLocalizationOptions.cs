namespace OrchardCore.Localization;

public class CultureLocalizationOptions
{
    public bool IgnoreSystemCulture { get; set; }

    public CultureSettings CultureSettings { get; set; } = CultureSettings.Default;
}
