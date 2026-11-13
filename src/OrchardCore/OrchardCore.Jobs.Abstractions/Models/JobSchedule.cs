using System.Text.Json.Nodes;

namespace OrchardCore.Jobs.Models;

/// <summary>
/// Queues a run of a job at each occurrence of a cron expression. Whatever the number of servers, an occurrence
/// creates a single run.
/// </summary>
public sealed class JobSchedule
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the schedule. The schedule a job declares has the name of the job.
    /// </summary>
    public string ScheduleId { get; set; }

    /// <summary>
    /// Gets or sets the name of the schedule shown in the admin. It is optional.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the job to run.
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    /// Gets or sets the parameters of the runs, if any.
    /// </summary>
    public JsonObject Parameters { get; set; }

    /// <summary>
    /// Gets or sets the cron expression, with five fields, in the time zone of the schedule.
    /// </summary>
    public string Cron { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the time zone the cron expression applies to, such as <c>Europe/Paris</c>.
    /// When it is <see langword="null"/>, which is the default, the expression applies to the time zone of the
    /// site, and follows it when it changes.
    /// </summary>
    public string TimeZoneId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the schedule creates runs.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the job declares this schedule. Such a schedule can be changed and
    /// disabled, but not deleted: it is created again as long as the job declares it.
    /// </summary>
    public bool IsDeclared { get; set; }

    /// <summary>
    /// Gets or sets the occurrence that creates the next run, in UTC.
    /// </summary>
    public DateTime? NextOccurrenceUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the time zone <see cref="NextOccurrenceUtc"/> was computed with. When the
    /// time zone of the schedule is not this one anymore, such as when the time zone of the site changed, the next
    /// occurrence is computed again.
    /// </summary>
    public string NextOccurrenceTimeZoneId { get; set; }

    /// <summary>
    /// Gets or sets the occurrence that created the last run, in UTC.
    /// </summary>
    public DateTime? LastOccurrenceUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the last run the schedule created.
    /// </summary>
    public string LastRunId { get; set; }

    /// <summary>
    /// Gets or sets when the schedule was created, in UTC.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the schedule was last changed by a user, in UTC.
    /// </summary>
    public DateTime? ModifiedUtc { get; set; }
}
