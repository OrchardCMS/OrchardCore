using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes;

/// <summary>
/// Indexes <see cref="WorkflowTypeDraft"/> documents by the workflow type they belong to.
/// </summary>
public sealed class WorkflowTypeDraftIndex : MapIndex
{
    /// <summary>
    /// The <see cref="WorkflowTypeDraft.WorkflowTypeId"/>.
    /// </summary>
    public string WorkflowTypeId { get; set; }
}

/// <summary>
/// Maps <see cref="WorkflowTypeDraft"/> documents to <see cref="WorkflowTypeDraftIndex"/>.
/// </summary>
public sealed class WorkflowTypeDraftIndexProvider : IndexProvider<WorkflowTypeDraft>
{
    public override void Describe(DescribeContext<WorkflowTypeDraft> context)
    {
        context.For<WorkflowTypeDraftIndex>()
            .Map(draft => new WorkflowTypeDraftIndex
            {
                WorkflowTypeId = draft.WorkflowTypeId,
            });
    }
}
