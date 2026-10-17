namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="ZipFilesStep"/>.
/// </summary>
public sealed class ZipFilesStepSettings
{
    /// <summary>
    /// Gets or sets the name of the archive, without its extension. It can use the placeholders of
    /// <see cref="DataPipelineTemplate"/>.
    /// </summary>
    public string FileName { get; set; } = "{PipelineName}-{Date:yyyyMMdd-HHmmss}";
}
