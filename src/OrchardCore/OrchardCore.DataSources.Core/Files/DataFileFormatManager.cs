namespace OrchardCore.DataSources.Files;

/// <summary>
/// The default <see cref="IDataFileFormatManager"/>, built from the registered <see cref="IDataFileFormat"/> services.
/// </summary>
public sealed class DataFileFormatManager : IDataFileFormatManager
{
    private readonly IReadOnlyList<IDataFileFormat> _formats;
    private readonly Dictionary<string, IDataFileFormat> _byName;

    public DataFileFormatManager(IEnumerable<IDataFileFormat> formats)
    {
        ArgumentNullException.ThrowIfNull(formats);

        _formats = formats
            .OrderBy(format => format.DisplayName?.Value ?? format.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        _byName = new Dictionary<string, IDataFileFormat>(StringComparer.OrdinalIgnoreCase);

        foreach (var format in _formats)
        {
            _byName.TryAdd(format.Name, format);
        }
    }

    public IReadOnlyList<IDataFileFormat> GetFormats() => _formats;

    public IDataFileFormat GetFormat(string name)
        => string.IsNullOrEmpty(name) ? null : _byName.GetValueOrDefault(name);

    public IDataFileFormat GetFormatByFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrEmpty(extension))
        {
            return null;
        }

        return _formats.FirstOrDefault(format => string.Equals(format.Extension, extension, StringComparison.OrdinalIgnoreCase));
    }
}
