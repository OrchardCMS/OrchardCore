using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataLocalization.Services;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.DataLocalization;

/// <summary>Registers the optional database UI-localization overlay.</summary>
[Feature("OrchardCore.DataLocalization.UI")]
public sealed class UILocalizationStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<UILocalizationCatalog>();
        services.AddScoped<IUITranslationsManager, UITranslationsManager>();
        services.AddSingleton<ITranslationOverrideProvider, UITranslationProvider>();
        services.AddScoped<INavigationProvider, UILocalizationAdminMenu>();
    }
}
