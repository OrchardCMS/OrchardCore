using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Facebook.Activities;
using OrchardCore.Facebook.Drivers;
using OrchardCore.Facebook.Filters;
using OrchardCore.Facebook.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Workflows.Helpers;
using Polly;

namespace OrchardCore.Facebook;

[Feature(FacebookConstants.Features.Pixel)]
public sealed class StartupPixel : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteDisplayDriver<FacebookPixelSettingsDisplayDriver>();
        services.AddPermissionProvider<PixelPermissionProvider>();
        services.AddNavigationProvider<AdminMenuPixel>();

        services.Configure<MvcOptions>((options) =>
        {
            options.Filters.Add<FacebookPixelFilter>();
        });

        services.AddHttpClient<IMetaConversionsApiService, MetaConversionsApiService>(client =>
        {
            client.BaseAddress = new Uri("https://graph.facebook.com/");
        }).AddResilienceHandler("oc-handler", builder => builder
            .AddRetry(new HttpRetryStrategyOptions
            {
                Name = "oc-retry",
                MaxRetryAttempts = 3,
                OnRetry = attempt =>
                {
                    attempt.RetryDelay.Add(TimeSpan.FromSeconds(0.5 * attempt.AttemptNumber));

                    return ValueTask.CompletedTask;
                },
            })
        );
    }
}

[RequireFeatures(FacebookConstants.Features.Pixel, "OrchardCore.Workflows")]
public sealed class StartupPixelWorkflows : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
        => services.AddActivity<MetaConversionsApiEventTask, MetaConversionsApiEventTaskDisplayDriver>();
}

