using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Smtp.Secrets.Drivers;
using OrchardCore.Email.Smtp.Secrets.Services;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Secrets;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IDisplayDriver<ISite>, SmtpSecretSettingsDisplayDriver>();
        services.AddSingleton<IPostConfigureOptions<SmtpOptions>, SmtpSecretsOptionsConfiguration>();
        services.AddScoped<IDataMigration, Migrations>();
    }
}
