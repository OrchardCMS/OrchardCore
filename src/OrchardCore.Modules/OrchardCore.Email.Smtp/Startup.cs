using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Smtp.Drivers;
using OrchardCore.Email.Smtp.Extensions;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Email.Smtp;

public sealed class Startup
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSmtpEmailProvider()
            .AddSiteDisplayDriver<SmtpSettingsDisplayDriver>()
            .AddSignalOptionsChangeTokenSource<SmtpOptions>()
            .AddTransient<IConfigureOptions<SmtpOptions>, SmtpOptionsConfiguration>()
            .AddTransient<IPostConfigureOptions<DefaultSmtpOptions>, DefaultSmtpOptionsConfiguration>();

        services.Configure<DefaultSmtpOptions>(options =>
        {
            // The 'OrchardCore_Email_Smtp' and 'OrchardCore_Email' sections are deprecated and will be removed in a future major version,
            // use 'Email:Smtp' instead.
            _shellConfiguration.GetSectionCompat("Email:Smtp", "OrchardCore_Email_Smtp", "OrchardCore_Email").Bind(options);

            options.IsEnabled = options.ConfigurationExists();
        });
    }
}
