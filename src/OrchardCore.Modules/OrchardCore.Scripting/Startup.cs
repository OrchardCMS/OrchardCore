using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Scripting.Providers;

namespace OrchardCore.Scripting;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddJavaScriptEngine();
        services.Configure<JavaScriptEngineOptions>(_shellConfiguration.GetSection("OrchardCore_Scripting_JavaScript"));
        services.AddSingleton<IGlobalMethodProvider, LogProvider>();
        services.AddSingleton<IGlobalMethodProvider, ProtectDataProvider>();
        services.AddSingleton<IGlobalMethodProvider, DataProtectionMethods>();
    }
}
