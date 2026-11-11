using OrchardCore.DataPipelines.Services;
using OrchardCore.Jobs;

namespace OrchardCore.DataPipelines.Jobs;

/// <summary>
/// The parameters of a run of <see cref="DataPipelineRunJob"/>.
/// </summary>
public sealed class DataPipelineRunJobParameters
{
    /// <summary>
    /// Gets or sets the identifier of the data pipeline run to execute.
    /// </summary>
    public string RunId { get; set; }
}

/// <summary>
/// Executes a run of a data pipeline. The job run is what makes a single server execute it, one run of a pipeline at
/// a time, and stop it when it is cancelled; the data pipeline run keeps what the pipeline did: its steps, its
/// deliveries and its log.
/// </summary>
public sealed class DataPipelineRunJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "DataPipelineRun";

    private readonly DataPipelineRunExecutor _executor;

    public DataPipelineRunJob(DataPipelineRunExecutor executor)
    {
        _executor = executor;
    }

    public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var parameters = context.GetParameters<DataPipelineRunJobParameters>();

        return _executor.ExecuteAsync(parameters.RunId, context, cancellationToken);
    }

    /// <summary>
    /// Gets the key that makes the runs of a pipeline execute one at a time, so two runs don't deliver the same data
    /// at once.
    /// </summary>
    /// <param name="pipelineId">The pipeline.</param>
    /// <returns>The concurrency key of the job runs of the pipeline.</returns>
    public static string GetConcurrencyKey(string pipelineId) => $"DataPipeline:{pipelineId}";
}
