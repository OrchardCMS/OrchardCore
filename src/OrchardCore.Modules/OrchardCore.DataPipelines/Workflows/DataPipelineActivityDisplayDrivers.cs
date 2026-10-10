using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DataPipelines.Services;
using OrchardCore.Workflows.Display;

namespace OrchardCore.DataPipelines.Workflows;

public sealed class RunDataPipelineTaskDisplayDriver : ActivityDisplayDriver<RunDataPipelineTask, RunDataPipelineTaskViewModel>
{
    private readonly DataPipelineManager _pipelineManager;

    public RunDataPipelineTaskDisplayDriver(DataPipelineManager pipelineManager)
    {
        _pipelineManager = pipelineManager;
    }

    protected override async ValueTask EditActivityAsync(RunDataPipelineTask activity, RunDataPipelineTaskViewModel model)
    {
        model.PipelineId = activity.PipelineId;
        model.WaitForCompletion = activity.WaitForCompletion;
        model.Pipelines = (await _pipelineManager.ListAsync()).Select(pipeline => (pipeline.PipelineId, pipeline.Name)).ToList();
    }

    protected override void UpdateActivity(RunDataPipelineTaskViewModel model, RunDataPipelineTask activity)
    {
        activity.PipelineId = model.PipelineId;
        activity.WaitForCompletion = model.WaitForCompletion;
    }
}

public class RunDataPipelineTaskViewModel
{
    public string PipelineId { get; set; }

    public bool WaitForCompletion { get; set; }

    [BindNever]
    public List<(string PipelineId, string Name)> Pipelines { get; set; } = [];
}

public sealed class DataPipelineRunCompletedEventDisplayDriver : ActivityDisplayDriver<DataPipelineRunCompletedEvent, DataPipelineRunCompletedEventViewModel>
{
    private readonly DataPipelineManager _pipelineManager;

    public DataPipelineRunCompletedEventDisplayDriver(DataPipelineManager pipelineManager)
    {
        _pipelineManager = pipelineManager;
    }

    protected override async ValueTask EditActivityAsync(DataPipelineRunCompletedEvent activity, DataPipelineRunCompletedEventViewModel model)
    {
        model.PipelineId = activity.PipelineId;
        model.Status = activity.Status;
        model.Pipelines = (await _pipelineManager.ListAsync()).Select(pipeline => (pipeline.PipelineId, pipeline.Name)).ToList();
    }

    protected override void UpdateActivity(DataPipelineRunCompletedEventViewModel model, DataPipelineRunCompletedEvent activity)
    {
        activity.PipelineId = string.IsNullOrEmpty(model.PipelineId) ? null : model.PipelineId;
        activity.Status = string.IsNullOrEmpty(model.Status) ? null : model.Status;
    }
}

public class DataPipelineRunCompletedEventViewModel
{
    public string PipelineId { get; set; }

    public string Status { get; set; }

    [BindNever]
    public List<(string PipelineId, string Name)> Pipelines { get; set; } = [];
}
