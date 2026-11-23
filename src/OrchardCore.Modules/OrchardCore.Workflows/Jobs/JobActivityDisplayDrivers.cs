using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Jobs;
using OrchardCore.Localization;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Jobs;

public sealed class RunJobTaskDisplayDriver : ActivityDisplayDriver<RunJobTask, RunJobTaskViewModel>
{
    private readonly JobOptions _jobOptions;
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public RunJobTaskDisplayDriver(
        IOptions<JobOptions> jobOptions,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<RunJobTaskDisplayDriver> stringLocalizer)
    {
        _jobOptions = jobOptions.Value;
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override void EditActivity(RunJobTask activity, RunJobTaskViewModel model)
    {
        model.JobName = activity.JobName;
        model.RunTitle = activity.RunTitle.Expression;
        model.Parameters = activity.Parameters.Expression;
        model.WaitForCompletion = activity.WaitForCompletion;
        model.Jobs = JobActivityHelpers.ListJobs(_jobOptions, _localizerFactory, onlyManual: true);
    }

    public override async Task<IDisplayResult> UpdateAsync(RunJobTask activity, UpdateEditorContext context)
    {
        var model = new RunJobTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (string.IsNullOrEmpty(model.JobName))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(RunJobTaskViewModel.JobName), S["A job is required."]);
        }
        else if (_jobOptions.Find(model.JobName) is not { CanRunManually: true })
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(RunJobTaskViewModel.JobName), S["The job doesn't exist, such as when its feature is disabled, or can't be run from a workflow."]);
        }

        activity.JobName = model.JobName;
        activity.RunTitle = new WorkflowExpression<string>(model.RunTitle?.Trim());
        activity.Parameters = new WorkflowExpression<string>(model.Parameters?.Trim());
        activity.WaitForCompletion = model.WaitForCompletion;

        return await EditAsync(activity, context);
    }
}

public class RunJobTaskViewModel
{
    public string JobName { get; set; }

    public string RunTitle { get; set; }

    public string Parameters { get; set; }

    public bool WaitForCompletion { get; set; }

    [BindNever]
    public List<(string Name, string Title)> Jobs { get; set; } = [];
}

public sealed class JobRunCompletedEventDisplayDriver : ActivityDisplayDriver<JobRunCompletedEvent, JobRunCompletedEventViewModel>
{
    private readonly JobOptions _jobOptions;
    private readonly IStringLocalizerFactory _localizerFactory;

    public JobRunCompletedEventDisplayDriver(IOptions<JobOptions> jobOptions, IStringLocalizerFactory localizerFactory)
    {
        _jobOptions = jobOptions.Value;
        _localizerFactory = localizerFactory;
    }

    protected override void EditActivity(JobRunCompletedEvent activity, JobRunCompletedEventViewModel model)
    {
        model.JobName = activity.JobName;
        model.Status = activity.Status;
        model.Jobs = JobActivityHelpers.ListJobs(_jobOptions, _localizerFactory);
    }

    protected override void UpdateActivity(JobRunCompletedEventViewModel model, JobRunCompletedEvent activity)
    {
        activity.JobName = string.IsNullOrEmpty(model.JobName) ? null : model.JobName;
        activity.Status = string.IsNullOrEmpty(model.Status) ? null : model.Status;
    }
}

public class JobRunCompletedEventViewModel
{
    public string JobName { get; set; }

    public string Status { get; set; }

    [BindNever]
    public List<(string Name, string Title)> Jobs { get; set; } = [];
}

internal static class JobActivityHelpers
{
    // The registered jobs with their localized title, ordered by title.
    public static List<(string Name, string Title)> ListJobs(JobOptions jobOptions, IStringLocalizerFactory localizerFactory, bool onlyManual = false)
    {
        return jobOptions.Jobs.Values
            .Where(job => !onlyManual || job.CanRunManually)
            .Select(job => (job.Name, Title: job.Title is null ? job.Name : localizerFactory.Localize(job.Title).Value))
            .OrderBy(job => job.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
