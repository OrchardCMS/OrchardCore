using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Users.Localization.Drivers;
using OrchardCore.Users.Localization.Providers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Localization;

[Feature("OrchardCore.Users.Localization")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<User, UserLocalizationDisplayDriver>();
        services.AddScoped<IUserClaimsProvider, UserLocalizationClaimsProvider>();

        services.Configure<RequestLocalizationOptions>(options =>
            options.AddInitialRequestCultureProvider(new UserLocalizationRequestCultureProvider()));
    }
}
