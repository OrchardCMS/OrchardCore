using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Options;
using OrchardCore.Workflows.Services;

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

    /// <summary>
    /// Registers an expression syntax, as a scoped service. It replaces a syntax registered earlier with the same
    /// name.
    /// </summary>
    /// <typeparam name="TProvider">The provider of the syntax.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddWorkflowExpressionProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IWorkflowExpressionProvider
    {
        services.TryAddScoped<TProvider>();
        services.Configure<WorkflowExpressionOptions>(options => options.Add(WorkflowExpressionProviderDescriptor.Create<TProvider>()));

        return services;
    }

    /// <summary>
    /// Registers a variable type in the <see cref="WorkflowVariableTypeOptions"/>, which hold the one instance of the
    /// type, so it is not a service and has no dependencies. It replaces a type registered earlier with the same name.
    /// </summary>
    /// <typeparam name="TType">The variable type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddWorkflowVariableType<TType>(this IServiceCollection services)
        where TType : class, IWorkflowVariableType, new()
    {
        services.Configure<WorkflowVariableTypeOptions>(options => options.Add(WorkflowVariableTypeDescriptor.Create<TType>()));

        return services;
    }
}
