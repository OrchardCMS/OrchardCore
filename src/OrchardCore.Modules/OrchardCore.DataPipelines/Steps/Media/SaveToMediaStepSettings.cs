namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="SaveToMediaStep"/>.
/// </summary>
public sealed class SaveToMediaStepSettings
{
    /// <summary>
    /// Gets or sets the folder of the media library the files are saved to. Empty for the root.
    /// </summary>
    public string Folder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a file with the same name is replaced. Otherwise the file is saved
    /// under a name with a number appended.
    /// </summary>
    public bool Overwrite { get; set; } = true;
}
