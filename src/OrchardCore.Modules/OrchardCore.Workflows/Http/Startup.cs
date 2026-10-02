using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Drivers;
using OrchardCore.Workflows.Http.Filters;
using OrchardCore.Workflows.Http.Handlers;
using OrchardCore.Workflows.Http.Liquid;
using OrchardCore.Workflows.Http.Scripting;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Http.WorkflowContextProviders;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http;

[Feature("OrchardCore.Workflows.Http")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<MvcOptions>(o =>
        {
            o.Filters.Add<WorkflowActionFilter>();
        });

        services.AddHttpClient();

        services.AddLiquidFilter<SignalUrlFilter>("signal_url");

        services.AddScoped<IWorkflowTypeEventHandler, WorkflowTypeRoutesHandler>();
        services.AddScoped<IWorkflowHandler, WorkflowRoutesHandler>();

        services.AddSingleton<IWorkflowTypeRouteEntries, WorkflowTypeRouteEntries>();
        services.AddSingleton<IWorkflowInstanceRouteEntries, WorkflowInstanceRouteEntries>();
        services.AddSingleton<IGlobalMethodProvider, HttpMethodsProvider>();
        services.AddScoped<IWorkflowExecutionContextHandler, SignalWorkflowExecutionContextHandler>();

        services.AddActivity<HttpRequestEvent, HttpRequestEventDisplayDriver>(activity => activity.Icon = "fa-solid fa-globe");
        services.AddActivity<HttpRequestFilterEvent, HttpRequestFilterEventDisplayDriver>(activity => activity.Icon = "fa-solid fa-filter");
        services.AddActivity<HttpRedirectTask, HttpRedirectTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-share");
        services.AddActivity<HttpRequestTask, HttpRequestTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-paper-plane");
        services.AddActivity<HttpResponseTask, HttpResponseTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-reply");
        services.AddActivity<SignalEvent, SignalEventDisplayDriver>(activity => activity.Icon = "fa-solid fa-signal");

        services.AddSingleton<IGlobalMethodProvider, TokenMethodProvider>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "HttpWorkflow",
            areaName: "OrchardCore.Workflows",
            pattern: "workflows/{action}",
            defaults: new { controller = "HttpWorkflow" }
        );

        routes.MapAreaControllerRoute(
            name: "InvokeWorkflow",
            areaName: "OrchardCore.Workflows",
            pattern: "workflows/invoke/{token}",
            defaults: new { controller = "HttpWorkflow", action = "Invoke" }
        );
    }
}
