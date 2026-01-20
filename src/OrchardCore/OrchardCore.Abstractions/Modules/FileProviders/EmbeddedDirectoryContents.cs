using System.Collections;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Modules.FileProviders;

public class EmbeddedDirectoryContents : IDirectoryContents
{
    private readonly IList<IFileInfo> _entries;

    public EmbeddedDirectoryContents(IEnumerable<IFileInfo> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        _entries = entries.ToList();
    }

    public bool Exists
    {
        get { return _entries.Any(); }
    }

    public IEnumerator<IFileInfo> GetEnumerator()
    {
        return _entries.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _entries.GetEnumerator();
    }
}
