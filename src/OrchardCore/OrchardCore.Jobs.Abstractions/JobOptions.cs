namespace OrchardCore.Jobs;

/// <summary>
/// Holds the registered jobs. <c>services.AddJob&lt;TJob&gt;(name)</c> adds them.
/// </summary>
public sealed class JobOptions
{
    private readonly Dictionary<string, JobDescriptor> _jobs = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the registered jobs by name, ignoring case.
    /// </summary>
    public IReadOnlyDictionary<string, JobDescriptor> Jobs => _jobs;

    /// <summary>
    /// Registers a job. It replaces a job registered earlier with the same name, so a module can replace the job
    /// of another module.
    /// </summary>
    /// <typeparam name="TJob">The type that executes the runs.</typeparam>
    /// <param name="name">The unique name of the job.</param>
    /// <param name="configure">Describes the job, if needed.</param>
    /// <returns>The descriptor of the job.</returns>
    public JobDescriptor AddJob<TJob>(string name, Action<JobDescriptor> configure = null)
        where TJob : class, IJob
    {
        var descriptor = new JobDescriptor(name, typeof(TJob));
        configure?.Invoke(descriptor);
        _jobs[name] = descriptor;

        return descriptor;
    }

    /// <summary>
    /// Finds a job by its name.
    /// </summary>
    /// <param name="name">The name of the job.</param>
    /// <returns>The descriptor of the job, or <see langword="null"/> when no such job is registered.</returns>
    public JobDescriptor Find(string name)
    {
        return name is not null && _jobs.TryGetValue(name, out var descriptor) ? descriptor : null;
    }

    /// <summary>
    /// Finds a job by the type that executes its runs.
    /// </summary>
    /// <typeparam name="TJob">The type that executes the runs.</typeparam>
    /// <returns>The descriptor of the job, or <see langword="null"/> when the type is not registered.</returns>
    public JobDescriptor Find<TJob>()
        where TJob : class, IJob
    {
        foreach (var descriptor in _jobs.Values)
        {
            if (descriptor.JobType == typeof(TJob))
            {
                return descriptor;
            }
        }

        return null;
    }
}
