namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="ReadMediaFileStep"/>.
/// </summary>
public sealed class ReadMediaFileStepSettings
{
    /// <summary>
    /// Gets or sets the path of the file in the media library.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the technical name of the format of the file, or <see langword="null"/> to infer it from the
    /// extension.
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    /// Gets or sets the character that separates the values of a CSV file. Defaults to a comma.
    /// </summary>
    public string Delimiter { get; set; } = ",";

    /// <summary>
    /// Gets or sets a value indicating whether the first row of a CSV file or a workbook holds the names of the
    /// columns.
    /// </summary>
    public bool HasHeaderRow { get; set; } = true;

    /// <summary>
    /// Gets or sets the worksheet of a workbook to read, or <see langword="null"/> for the first one.
    /// </summary>
    public string SheetName { get; set; }
}
