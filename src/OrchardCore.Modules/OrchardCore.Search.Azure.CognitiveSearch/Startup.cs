using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Azure.CognitiveSearch.Drivers;
using OrchardCore.Search.Azure.CognitiveSearch.Services;
using OrchardCore.Settings;

namespace OrchardCore.Search.Azure.CognitiveSearch;

public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAzureCognitiveSearchServices(_shellConfiguration);
        services.AddScoped<IDisplayDriver<ISite>, AzureCognitiveSearchSettingsDisplayDriver>();
        services.AddScoped<INavigationProvider, AdminMenu>();

    }
}

[RequireFeatures("OrchardCore.Search")]
public class SearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISearchService, AzureCognitiveSearchService>();
    }
}
