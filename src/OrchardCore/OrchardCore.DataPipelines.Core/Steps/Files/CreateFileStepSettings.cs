using OrchardCore.DataSources.Files;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="CreateFileStep"/>.
/// </summary>
public sealed class CreateFileStepSettings
{
    /// <summary>
    /// Gets or sets the technical name of the file format, such as <c>csv</c>, <c>json</c>, <c>jsonl</c> or
    /// <c>xlsx</c>.
    /// </summary>
    public string Format { get; set; } = CsvDataFileFormat.FormatName;

    /// <summary>
    /// Gets or sets the name of the file, without its extension. It can use the placeholders of
    /// <see cref="DataPipelineTemplate"/>, such as <c>orders-{Date:yyyyMMdd}</c>.
    /// </summary>
    public string FileName { get; set; } = "{PipelineName}-{Date:yyyyMMdd-HHmmss}";

    /// <summary>
    /// Gets or sets a value indicating whether a CSV file or a workbook starts with a row of column names.
    /// </summary>
    public bool IncludeHeader { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the columns are named with the labels of the fields instead of their
    /// technical names.
    /// </summary>
    public bool UseDisplayNames { get; set; }

    /// <summary>
    /// Gets or sets the character that separates the values of a CSV file. Defaults to a comma.
    /// </summary>
    public string Delimiter { get; set; } = ",";

    /// <summary>
    /// Gets or sets the name of the worksheet of a workbook.
    /// </summary>
    public string SheetName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a JSON file is indented.
    /// </summary>
    public bool Indented { get; set; }
}
