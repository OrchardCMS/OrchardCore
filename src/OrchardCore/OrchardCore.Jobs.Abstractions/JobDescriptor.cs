using OrchardCore.Localization;

namespace OrchardCore.Jobs;

/// <summary>
/// Describes a registered job: its name, its type, and how its runs are retried, scheduled and kept.
/// </summary>
public sealed class JobDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JobDescriptor"/> class.
    /// </summary>
    /// <param name="name">The unique name of the job, which its runs refer to.</param>
    /// <param name="jobType">The type that executes the runs, which implements <see cref="IJob"/>.</param>
    public JobDescriptor(string name, Type jobType)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(jobType);

        if (!typeof(IJob).IsAssignableFrom(jobType))
        {
            throw new ArgumentException($"The type '{jobType}' doesn't implement '{nameof(IJob)}'.", nameof(jobType));
        }

        Name = name;
        JobType = jobType;
    }

    /// <summary>
    /// Gets the unique name of the job. Don't change it once runs or schedules of the job exist.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type that executes the runs. It is created for each run, in the service scope of the run.
    /// </summary>
    public Type JobType { get; }

    /// <summary>
    /// Gets or sets the title of the job shown in the admin. The name of the job is shown when it is not set.
    /// </summary>
    public LocalizationSource Title { get; set; }

    /// <summary>
    /// Gets or sets what the job does, shown in the admin.
    /// </summary>
    public LocalizationSource Description { get; set; }

    /// <summary>
    /// Gets or sets the number of attempts a run may make. With the default of 1, a run that fails, or whose
    /// server stops while executing it, fails and is not executed again. Set it higher only for a job that can run
    /// again from the start.
    /// </summary>
    public int MaxAttempts { get; set; } = 1;

    /// <summary>
    /// Gets or sets how long a failed run waits before its second attempt. The delay doubles for each following
    /// attempt.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the cron expression, with five fields, of the schedule the job declares, or
    /// <see langword="null"/> for a job that is not scheduled. The schedule is created with this value, and can then
    /// be changed in the admin.
    /// </summary>
    public string Schedule { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the schedule the job declares is created enabled.
    /// </summary>
    public bool ScheduleEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a user can run the job from the admin, and schedule it. Leave it
    /// off for a job whose runs only make sense with the parameters its module queues them with. A job that
    /// declares a <see cref="Schedule"/> can always be run from the admin.
    /// </summary>
    public bool AllowManualRun { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the runs that succeeded are kept. Turn it off for a job that runs
    /// very often, whose succeeded runs are of no interest: they are deleted as they complete, and the failed ones
    /// are kept.
    /// </summary>
    public bool KeepSucceededRuns { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether a user can run the job from the admin.
    /// </summary>
    public bool CanRunManually => AllowManualRun || !string.IsNullOrEmpty(Schedule);
}
