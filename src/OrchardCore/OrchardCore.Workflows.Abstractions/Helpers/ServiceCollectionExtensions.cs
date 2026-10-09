using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Options;

namespace OrchardCore.Workflows.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddActivity<TActivity, TDriver>(this IServiceCollection services)
        where TActivity : class, IActivity where TDriver : class, IDisplayDriver<IActivity>
    {
        services.Configure<WorkflowOptions>(options => options.RegisterActivity<TActivity, TDriver>());

        return services;
    }

    /// <summary>
    /// Registers an activity and its display driver, and configures its registration, for example
    /// <c>services.AddActivity&lt;MyTask, MyTaskDisplayDriver&gt;(activity => activity.Icon = "fa-solid fa-star")</c>.
    /// </summary>
    public static IServiceCollection AddActivity<TActivity, TDriver>(this IServiceCollection services, Action<ActivityRegistration> configure)
        where TActivity : class, IActivity where TDriver : class, IDisplayDriver<IActivity>
    {
        services.Configure<WorkflowOptions>(options => options.RegisterActivity(typeof(TActivity), typeof(TDriver), configure));

        return services;
    }
}
