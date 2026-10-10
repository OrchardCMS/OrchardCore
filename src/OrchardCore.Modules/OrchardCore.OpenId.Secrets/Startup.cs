using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Server;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.OpenId.Secrets.Drivers;
using OrchardCore.OpenId.Secrets.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Secrets;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<OpenIdServerSettings, OpenIdSecretSettingsDisplayDriver>();
        services.AddSingleton<IPostConfigureOptions<OpenIddictServerOptions>, OpenIdSecretsOptionsConfiguration>();
        services.AddScoped<IDataMigration, Migrations>();
    }
}
