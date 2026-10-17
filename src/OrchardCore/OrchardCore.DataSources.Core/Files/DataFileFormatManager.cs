using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.DataSources.Files;

/// <summary>
/// The default <see cref="IDataFileFormatManager"/>, built from the formats of <see cref="DataFileFormatOptions"/>. It
/// lists them from their descriptors, and resolves a format from the services only when it is used.
/// </summary>
public sealed class DataFileFormatManager : IDataFileFormatManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyList<DataFileFormatDescriptor> _formats;
    private readonly Dictionary<string, DataFileFormatDescriptor> _byName = new(StringComparer.OrdinalIgnoreCase);

    public DataFileFormatManager(IOptions<DataFileFormatOptions> options, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(options);

        _serviceProvider = serviceProvider;
        _formats = options.Value.Formats;

        foreach (var format in _formats)
        {
            _byName.TryAdd(format.Name, format);
        }
    }

    public IReadOnlyList<DataFileFormatDescriptor> GetFormats() => _formats;

    public DataFileFormatDescriptor GetDescriptor(string name)
    {
        return string.IsNullOrEmpty(name) ? null : _byName.GetValueOrDefault(name);
    }

    public DataFileFormatDescriptor GetDescriptorByFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrEmpty(extension))
        {
            return null;
        }

        return _formats.FirstOrDefault(format => string.Equals(format.Extension, extension, StringComparison.OrdinalIgnoreCase));
    }

    public IDataFileFormat GetFormat(string name)
    {
        return GetDescriptor(name) is { } descriptor
            ? (IDataFileFormat)_serviceProvider.GetRequiredService(descriptor.Type)
            : null;
    }
}
