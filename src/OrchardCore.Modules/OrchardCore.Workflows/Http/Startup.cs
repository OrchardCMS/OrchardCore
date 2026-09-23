using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Drivers;
using OrchardCore.Workflows.Http.Filters;
using OrchardCore.Workflows.Http.Handlers;
using OrchardCore.Workflows.Http.Liquid;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Http.Scripting;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Http.WorkflowContextProviders;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http;

[Feature("OrchardCore.Workflows.Http")]
public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="shellConfiguration">The shell configuration.</param>
    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<MvcOptions>(o =>
        {
            o.Filters.Add<WorkflowActionFilter>();
        });

        services.Configure<HttpRequestTaskOptions>(_shellConfiguration.GetSection(HttpRequestTaskOptions.ConfigurationSection));
        services.AddSingleton<IHttpRequestDestinationValidator, HttpRequestDestinationValidator>();
        services.AddHttpClient(HttpRequestTaskHttpClient.Name)
            .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                HttpRequestTaskHttpClient.CreateHandler(serviceProvider.GetRequiredService<IHttpRequestDestinationValidator>()));

        services.AddLiquidFilter<SignalUrlFilter>("signal_url");

        services.AddScoped<IWorkflowTypeEventHandler, WorkflowTypeRoutesHandler>();
        services.AddScoped<IWorkflowHandler, WorkflowRoutesHandler>();

        services.AddSingleton<IWorkflowTypeRouteEntries, WorkflowTypeRouteEntries>();
        services.AddSingleton<IWorkflowInstanceRouteEntries, WorkflowInstanceRouteEntries>();
        services.AddSingleton<IGlobalMethodProvider, HttpMethodsProvider>();
        services.AddScoped<IWorkflowExecutionContextHandler, SignalWorkflowExecutionContextHandler>();

        services.AddActivity<HttpRequestEvent, HttpRequestEventDisplayDriver>();
        services.AddActivity<HttpRequestFilterEvent, HttpRequestFilterEventDisplayDriver>();
        services.AddActivity<HttpRedirectTask, HttpRedirectTaskDisplayDriver>();
        services.AddActivity<HttpRequestTask, HttpRequestTaskDisplayDriver>();
        services.AddActivity<HttpResponseTask, HttpResponseTaskDisplayDriver>();
        services.AddActivity<SignalEvent, SignalEventDisplayDriver>();

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
