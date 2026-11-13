using OrchardCore.Jobs;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides the extension methods that register jobs.
/// </summary>
public static class JobServiceCollectionExtensions
{
    /// <summary>
    /// Registers a job, which runs once whatever the number of servers. The job is not added to the service
    /// container: it is created for each run, in the service scope of that run, so its constructor can take any
    /// service of the tenant. The feature that registers a job must depend on the <c>OrchardCore.Jobs</c> feature.
    /// </summary>
    /// <typeparam name="TJob">The type that executes the runs.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The unique name of the job, which its runs refer to.</param>
    /// <param name="configure">Describes the job, if needed: its title, its attempts, its schedule.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddJob<TJob>(this IServiceCollection services, string name, Action<JobDescriptor> configure = null)
        where TJob : class, IJob
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        services.Configure<JobOptions>(options => options.AddJob<TJob>(name, configure));

        return services;
    }
}
