using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataLocalization.Services;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.DataLocalization;

/// <summary>Registers the optional database UI-localization overlay.</summary>
[Feature("OrchardCore.DataLocalization.Ui")]
public sealed class UiLocalizationStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IUiLocalizationCatalog, EmbeddedUiLocalizationCatalog>();
        services.AddScoped<IUiTranslationsManager, UiTranslationsManager>();
        services.AddSingleton<ITranslationOverrideProvider, UiTranslationProvider>();
        services.AddScoped<INavigationProvider, UiLocalizationAdminMenu>();
    }
}
