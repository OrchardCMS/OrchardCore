namespace OrchardCore.FileStorage;

/// <summary>
/// Provides tenant-scoped locations (files and directories) for storing files temporarily, such as
/// in-progress chunked uploads or extracted import archives.
/// </summary>
/// <remarks>
/// This abstraction hands out real local filesystem paths; the caller performs the actual I/O with
/// <see cref="System.IO"/> APIs. It does not itself read or write file contents. The base location is controlled by
/// <see cref="TempWorkspaceOptions.TempPath"/>, allowing operators to relocate temporary storage onto a larger or
/// shared volume (for example a mounted Azure Files or AWS EFS share) instead of the operating system temporary
/// directory, whose available space is often limited.
/// <para>
/// Because the paths are consumed with local filesystem APIs (such as <c>ZipFile.ExtractToDirectory</c>,
/// <c>PhysicalFileProvider</c>, or random-access <see cref="System.IO.FileStream"/> operations), the location must be
/// a real filesystem. To relocate temporary files onto Azure or AWS, mount a file share (Azure Files, AWS EFS/FSx) and
/// point <see cref="TempWorkspaceOptions.TempPath"/> at it; object storage (Azure Blob, AWS S3) cannot satisfy this
/// contract.
/// </para>
/// </remarks>
public interface ITempWorkspace
{
    /// <summary>
    /// Gets the absolute, tenant-scoped root directory for temporary files, creating it if it does not exist.
    /// </summary>
    string GetRootDirectory();

    /// <summary>
    /// Gets the absolute path of a named sub-directory under the root, creating it if it does not exist.
    /// </summary>
    /// <param name="name">
    /// A relative sub-directory name. Path segments that would resolve outside of the root are not allowed.
    /// </param>
    string GetOrCreateSubdirectory(string name);

    /// <summary>
    /// Returns an absolute path to a new, unique temporary file under the root. The file itself is not created,
    /// but the root directory is ensured to exist.
    /// </summary>
    /// <param name="extension">An optional file extension, with or without a leading dot.</param>
    string GetTempFileName(string extension = null);

    /// <summary>
    /// Creates a new, unique temporary sub-directory under the root and returns its absolute path.
    /// </summary>
    string CreateTempSubdirectory();
}
