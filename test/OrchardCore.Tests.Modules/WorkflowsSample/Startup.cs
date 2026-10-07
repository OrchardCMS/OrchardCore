using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Workflows.Services;

namespace WorkflowsSample;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IWorkflowExpressionProvider, UpperCaseExpressionProvider>();
    }
}
