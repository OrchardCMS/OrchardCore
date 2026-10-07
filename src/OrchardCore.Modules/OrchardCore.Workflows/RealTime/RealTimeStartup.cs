using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Security;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.RealTime;

[Feature("OrchardCore.Workflows.SignalR")]
public sealed class RealTimeStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IWorkflowDesignerNotifier, SignalRWorkflowDesignerNotifier>();

        services.Configure<AuthorizationOptions>(options => options.AddPolicy(WorkflowsHub.PolicyName, policy =>
        {
            policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api, IdentityConstants.ApplicationScheme);
            policy.Requirements.Add(new PermissionRequirement(WorkflowsPermissions.ManageWorkflows));
            policy.RequireAuthenticatedUser();
        }));
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapHub<WorkflowsHub>(WorkflowsRealTime.HubPath);
    }
}
