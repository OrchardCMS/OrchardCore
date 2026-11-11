using OrchardCore.Jobs.Models;

namespace OrchardCore.Jobs;

/// <summary>
/// Manages the schedules that queue runs of jobs at the occurrences of a cron expression.
/// </summary>
public interface IJobScheduleManager
{
    /// <summary>
    /// Lists the schedules, including the ones the registered jobs declare.
    /// </summary>
    /// <returns>The schedules, ordered by job name.</returns>
    Task<IReadOnlyList<JobSchedule>> ListAsync();

    /// <summary>
    /// Finds a schedule.
    /// </summary>
    /// <param name="scheduleId">The identifier of the schedule.</param>
    /// <returns>The schedule, or <see langword="null"/>.</returns>
    Task<JobSchedule> GetAsync(string scheduleId);

    /// <summary>
    /// Creates or changes a schedule, and computes its next occurrence. A schedule without an identifier gets one.
    /// </summary>
    /// <param name="schedule">The schedule.</param>
    /// <returns>A task that completes when the schedule is saved with the current database session.</returns>
    /// <exception cref="ArgumentException">The cron expression is not valid, or the job is not registered.</exception>
    Task SaveAsync(JobSchedule schedule);

    /// <summary>
    /// Deletes a schedule, unless its job declares it.
    /// </summary>
    /// <param name="scheduleId">The identifier of the schedule.</param>
    /// <returns><see langword="true"/> when the schedule exists and was deleted.</returns>
    Task<bool> DeleteAsync(string scheduleId);

    /// <summary>
    /// Tells whether a cron expression with five fields is valid.
    /// </summary>
    /// <param name="cron">The cron expression.</param>
    /// <returns><see langword="true"/> when the expression is valid.</returns>
    bool IsValidCron(string cron);
}
