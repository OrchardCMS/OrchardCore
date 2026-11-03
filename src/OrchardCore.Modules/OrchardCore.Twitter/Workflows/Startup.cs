using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Twitter.Workflows.Activities;
using OrchardCore.Twitter.Workflows.Drivers;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Options;

namespace OrchardCore.Twitter.Workflows;

[RequireFeatures("OrchardCore.Workflows")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<WorkflowOptions>(options => options.CategoryIcons[TwitterLocalizationSources.Social.Value] = "fa-solid fa-share-nodes");

        services.AddActivity<UpdateTwitterStatusTask, UpdateTwitterStatusTaskDisplayDriver>();
    }
}
