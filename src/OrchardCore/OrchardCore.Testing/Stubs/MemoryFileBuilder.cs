using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Deployment;

namespace OrchardCore.Testing.Stubs;
/// <summary>
/// Represents In memory file builder that uses a dictionary as virtual file system.
/// </summary>
public class MemoryFileBuilder : IFileBuilder
{
    private readonly Dictionary<string, byte[]> _virtualFiles = new();

    /// <inheritdoc/>
    public async Task SetFileAsync(string subpath, Stream stream)
    {
        using var memoryStream = new MemoryStream();

        await stream.CopyToAsync(memoryStream);

        _virtualFiles[subpath] = memoryStream.ToArray();
    }

    /// <summary>
    /// Read the contents of a file using the specified encoding.
    /// </summary>
    /// <param name="subpath">The file path.</param>
    /// <param name="encoding">The encoding used to convert the byte array to string.</param>
    /// <returns></returns>
    public string GetFileContents(string subpath, Encoding encoding) => encoding.GetString(_virtualFiles[subpath]);
}
