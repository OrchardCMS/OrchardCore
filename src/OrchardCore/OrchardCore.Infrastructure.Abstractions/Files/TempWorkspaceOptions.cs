namespace OrchardCore.FileStorage;

/// <summary>
/// Options controlling where <see cref="ITempWorkspace"/> places temporary files.
/// Bound from the <c>OrchardCore:TempWorkspace</c> configuration section.
/// </summary>
public class TempWorkspaceOptions
{
    /// <summary>
    /// The base path under which tenant-scoped temporary files are stored. When not set, the operating system
    /// temporary directory (<see cref="System.IO.Path.GetTempPath"/>) is used.
    /// </summary>
    /// <remarks>
    /// Point this to a larger or shared volume (for example a mounted Azure Files or AWS EFS share) to avoid
    /// exhausting the limited space that is typically available in the system temporary directory.
    /// </remarks>
    public string TempPath { get; set; }
}
