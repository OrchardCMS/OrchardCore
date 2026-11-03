using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Email.Workflows.Activities;
using OrchardCore.Email.Workflows.Drivers;
using OrchardCore.Modules;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Options;

namespace OrchardCore.Email.Workflows;

[RequireFeatures("OrchardCore.Workflows")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<WorkflowOptions>(options => options.CategoryIcons[ActivityCategoryLocalizationSources.Messaging.Value] = "fa-solid fa-envelope");

        services.AddActivity<EmailTask, EmailTaskDisplayDriver>();
    }
}
