using OrchardCore.Deployment;

namespace OrchardCore.Tests.Stubs;

/// <summary>
/// In memory file builder that uses a dictionary as virtual file system.
/// Intended for unit testing.
/// </summary>
public class MemoryFileBuilder : IFileBuilder
{
    public Dictionary<string, byte[]> VirtualFiles { get; private set; } = [];

    public async Task SetFileAsync(string subpath, Stream stream)
    {
        var buffer = new byte[stream.Length];
        using var ms = new MemoryStream(buffer);
        await stream.CopyToAsync(ms);
        VirtualFiles[subpath] = buffer;
    }

    /// <summary>
    /// Read the contents of a file built with this file builder, using the specified encoding.
    /// </summary>
    /// <param name="subpath">Path and/or file name</param>
    /// <param name="encoding">Encoding used to convert the byte array to string</param>
    /// <returns></returns>
    public string GetFileContents(string subpath, Encoding encoding)
    {
        return encoding.GetString(VirtualFiles[subpath]);
    }
}
