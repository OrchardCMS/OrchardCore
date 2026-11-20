using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Jobs.Models;
using OrchardCore.Localization;

namespace OrchardCore.Jobs.Services;

/// <summary>
/// Gives the admin pages the localized title and description of the registered jobs.
/// </summary>
public sealed class JobDisplayService
{
    private readonly JobOptions _jobOptions;
    private readonly IStringLocalizerFactory _stringLocalizerFactory;

    public JobDisplayService(IOptions<JobOptions> jobOptions, IStringLocalizerFactory stringLocalizerFactory)
    {
        _jobOptions = jobOptions.Value;
        _stringLocalizerFactory = stringLocalizerFactory;
    }

    /// <summary>
    /// Gets the registered jobs, ordered by title.
    /// </summary>
    /// <returns>The jobs.</returns>
    public IReadOnlyList<JobDescriptor> GetJobs()
    {
        return _jobOptions.Jobs.Values
            .OrderBy(GetTitle, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Finds a registered job.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <returns>The job, or <see langword="null"/> when it is not registered, such as when its feature is disabled.</returns>
    public JobDescriptor Find(string jobName) => _jobOptions.Find(jobName);

    /// <summary>
    /// Gets the title of a job, or its name when it has no title or is not registered anymore.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <returns>The localized title.</returns>
    public string GetTitle(string jobName)
    {
        var descriptor = _jobOptions.Find(jobName);

        return descriptor is null ? jobName : GetTitle(descriptor);
    }

    /// <summary>
    /// Gets the title of a job, or its name when it has no title.
    /// </summary>
    /// <param name="descriptor">The job.</param>
    /// <returns>The localized title.</returns>
    public string GetTitle(JobDescriptor descriptor)
    {
        return descriptor.Title is null ? descriptor.Name : _stringLocalizerFactory.Localize(descriptor.Title).Value;
    }

    /// <summary>
    /// Gets what a job does.
    /// </summary>
    /// <param name="descriptor">The job.</param>
    /// <returns>The localized description, or <see langword="null"/>.</returns>
    public string GetDescription(JobDescriptor descriptor)
    {
        return descriptor.Description is null ? null : _stringLocalizerFactory.Localize(descriptor.Description).Value;
    }

    /// <summary>
    /// Gets a value that changes when what the admin pages show of a run changes.
    /// </summary>
    /// <param name="run">The run.</param>
    /// <returns>The signature of the run.</returns>
    public static string GetSignature(JobRun run)
    {
        return FormattableString.Invariant($"{run.Status}|{run.Attempt}|{run.Progress}|{run.ProgressMessage?.GetHashCode()}|{run.Log.Count}|{run.CancellationRequested}|{run.DueUtc.Ticks}");
    }
}
