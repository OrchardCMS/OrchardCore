using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.ViewModels.Designer;

// The JSON contract of DataPipelineDesignerController. Keep in sync with Assets/designer/src/api/types.ts.

public sealed class DesignerIssue
{
    public string Severity { get; set; }

    public string Message { get; set; }

    public string StepId { get; set; }

    public static DesignerIssue From(DataPipelineIssue issue)
        => new()
        {
            Severity = issue.Severity.ToString(),
            Message = issue.Message,
            StepId = issue.StepId,
        };
}

public sealed class DesignerPort
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string Kind { get; set; }

    public bool IsRequired { get; set; }

    public bool AllowsMany { get; set; }

    public static DesignerPort From(DataPipelinePort port)
        => new()
        {
            Name = port.Name,
            DisplayName = port.DisplayName,
            Kind = port.Kind.ToString(),
            IsRequired = port.IsRequired,
            AllowsMany = port.AllowsMany,
        };
}

public sealed class DesignerField
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string Type { get; set; }

    public string Group { get; set; }

    public static DesignerField From(DataField field)
        => new()
        {
            Name = field.Name,
            DisplayName = field.DisplayName,
            Type = field.Type.ToString(),
            Group = field.Group,
        };
}

public sealed class DesignerNode
{
    public string Id { get; set; }

    public string Type { get; set; }

    public string Title { get; set; }

    public string DisplayText { get; set; }

    public string Category { get; set; }

    public string Icon { get; set; }

    public bool IsMissing { get; set; }

    public bool HasEditor { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public List<DesignerPort> Inputs { get; set; } = [];

    public List<DesignerPort> Outputs { get; set; } = [];

    public string DesignHtml { get; set; }
}

public sealed class DesignerConnection
{
    public string SourceStepId { get; set; }

    public string SourcePort { get; set; }

    public string TargetStepId { get; set; }

    public string TargetPort { get; set; }

    public static DesignerConnection From(DataPipelineConnection connection)
        => new()
        {
            SourceStepId = connection.SourceStepId,
            SourcePort = connection.SourcePort,
            TargetStepId = connection.TargetStepId,
            TargetPort = connection.TargetPort,
        };

    public DataPipelineConnection ToConnection()
        => new()
        {
            SourceStepId = SourceStepId,
            SourcePort = SourcePort,
            TargetStepId = TargetStepId,
            TargetPort = TargetPort,
        };
}

public sealed class DesignerSettings
{
    public string Name { get; set; }

    public string Description { get; set; }
}

public sealed class DesignerVersion
{
    public string VersionId { get; set; }

    public int Number { get; set; }

    public DateTime PublishedUtc { get; set; }

    public string PublishedBy { get; set; }

    public bool IsPublished { get; set; }

    public static DesignerVersion From(DataPipelineVersion version, DataPipeline pipeline)
        => version is null
            ? null
            : new()
            {
                VersionId = version.VersionId,
                Number = version.Number,
                PublishedUtc = version.PublishedUtc,
                PublishedBy = version.PublishedBy,
                IsPublished = version.VersionId == pipeline?.PublishedVersionId,
            };
}

public sealed class DesignerRunStep
{
    public string StepId { get; set; }

    public string Title { get; set; }

    public string Status { get; set; }

    public long RowsIn { get; set; }

    public long RowsOut { get; set; }

    public long FilesIn { get; set; }

    public long FilesOut { get; set; }

    public long Warnings { get; set; }

    public string Error { get; set; }
}

public sealed class DesignerRunDelivery
{
    public string StepId { get; set; }

    public string Description { get; set; }

    public string Url { get; set; }

    public DateTime DeliveredUtc { get; set; }
}

public sealed class DesignerRunLogEntry
{
    public DateTime Utc { get; set; }

    public string Level { get; set; }

    public string StepId { get; set; }

    public string Message { get; set; }
}

public sealed class DesignerRun
{
    public string RunId { get; set; }

    public string Status { get; set; }

    public int VersionNumber { get; set; }

    public string Trigger { get; set; }

    public string TriggeredBy { get; set; }

    public DateTime QueuedUtc { get; set; }

    public DateTime? StartedUtc { get; set; }

    public DateTime? CompletedUtc { get; set; }

    public string Error { get; set; }

    public string FailedStepId { get; set; }

    public List<DesignerRunStep> Steps { get; set; } = [];

    public List<DesignerRunDelivery> Deliveries { get; set; } = [];

    public List<DesignerRunLogEntry> Log { get; set; } = [];

    public string Url { get; set; }

    public bool CanCancel { get; set; }
}

public sealed class DesignerDefinition
{
    public string PipelineId { get; set; }

    public int Revision { get; set; }

    public bool HasDraft { get; set; }

    public string DraftModifiedBy { get; set; }

    public string DraftModifiedByUserId { get; set; }

    public DateTime? DraftModifiedUtc { get; set; }

    public DesignerSettings Settings { get; set; }

    public List<DesignerNode> Nodes { get; set; } = [];

    public List<DesignerConnection> Connections { get; set; } = [];

    public List<DesignerIssue> Issues { get; set; } = [];

    public DesignerVersion PublishedVersion { get; set; }

    public DesignerVersion Version { get; set; }

    public bool CanRun { get; set; }

    public DesignerRun LastRun { get; set; }
}

public sealed class DesignerLibraryStep
{
    public string Name { get; set; }

    public string DisplayText { get; set; }

    public string Description { get; set; }

    public string Category { get; set; }

    public string Icon { get; set; }

    public List<DesignerPort> Inputs { get; set; } = [];

    public List<DesignerPort> Outputs { get; set; } = [];
}

public sealed class DesignerLibraryCategory
{
    public string Category { get; set; }

    public string DisplayName { get; set; }

    public List<DesignerLibraryStep> Steps { get; set; } = [];
}

public sealed class DesignerLibrary
{
    public List<DesignerLibraryCategory> Categories { get; set; } = [];
}

public sealed class DesignerSaveNode
{
    public string Id { get; set; }

    public int X { get; set; }

    public int Y { get; set; }
}

public sealed class DesignerSaveRequest
{
    public int Revision { get; set; }

    public List<DesignerSaveNode> Nodes { get; set; } = [];

    public List<DesignerConnection> Connections { get; set; } = [];

    public List<string> RemovedStepIds { get; set; } = [];

    public List<string> RestoredStepIds { get; set; } = [];
}

public sealed class DesignerConnectFrom
{
    public string StepId { get; set; }

    public string Port { get; set; }
}

public sealed class DesignerAddStepRequest
{
    public int Revision { get; set; }

    public string Type { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public DesignerConnectFrom ConnectFrom { get; set; }
}

public sealed class DesignerRevisionRequest
{
    public int Revision { get; set; }
}

public sealed class DesignerRestoreRequest
{
    public string VersionId { get; set; }

    public int Revision { get; set; }
}

public sealed class DesignerPreviewRequest
{
    public string StepId { get; set; }
}

public sealed class DesignerRunRequest
{
    public string RunId { get; set; }
}

public sealed class DesignerPortFields
{
    public string Port { get; set; }

    public string DisplayName { get; set; }

    public string Kind { get; set; }

    public List<DesignerField> Fields { get; set; } = [];
}

public sealed class DesignerStepFields
{
    public string StepId { get; set; }

    public List<DesignerPortFields> Inputs { get; set; } = [];

    public List<DesignerPortFields> Outputs { get; set; } = [];
}

public sealed class DesignerPreviewFile
{
    public string FileName { get; set; }

    public string ContentType { get; set; }

    public long Length { get; set; }

    public long? RowCount { get; set; }
}

public sealed class DesignerPreviewPort
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string Kind { get; set; }

    public List<DesignerField> Fields { get; set; } = [];

    public List<object[]> Rows { get; set; } = [];

    public bool Truncated { get; set; }

    public List<DesignerPreviewFile> Files { get; set; } = [];
}

public sealed class DesignerPreview
{
    public string StepId { get; set; }

    public string Error { get; set; }

    public bool IsInputPreview { get; set; }

    public List<DesignerPreviewPort> Ports { get; set; } = [];

    public List<DesignerIssue> Issues { get; set; } = [];

    public long DurationMilliseconds { get; set; }
}

public sealed class DesignerFragmentViewModel
{
    public OrchardCore.DisplayManagement.IShape Shape { get; init; }

    public string PartialName { get; init; }

    public object PartialModel { get; init; }

    public string PipelineId { get; init; }

    public string StepId { get; init; }

    public bool Valid { get; init; } = true;
}
