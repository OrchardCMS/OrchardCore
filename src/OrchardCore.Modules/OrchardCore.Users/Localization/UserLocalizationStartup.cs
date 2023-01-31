using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Users.Localization.Drivers;
using OrchardCore.Users.Localization.Providers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Localization;

[Feature("OrchardCore.Users.Localization")]
public class UserLocalizationStartup : StartupBase
{
    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var localizationService = serviceProvider.GetService<ILocalizationService>();

        var defaultCulture = localizationService.GetDefaultCultureAsync().GetAwaiter().GetResult();
        var supportedCultures = localizationService.GetSupportedCulturesAsync().GetAwaiter().GetResult();

        var cultureOptions = serviceProvider.GetService<IOptions<CultureOptions>>().Value;

        var requestLocalizationOptions = new OrchardCoreRequestLocalizationOptions(ignoreSystemSettings: cultureOptions.IgnoreSystemSettings)
            .SetDefaultCulture(defaultCulture)
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);

        requestLocalizationOptions.RequestCultureProviders.Insert(0, new UserLocalizationRequestCultureProvider());

        app.UseRequestLocalization(requestLocalizationOptions);
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IDisplayDriver<User>, UserLocalizationDisplayDriver>();
        services.AddScoped<IUserClaimsProvider, UserLocalizationClaimsProvider>();
    }
}
