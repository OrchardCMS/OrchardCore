using OrchardCore.Localization;

namespace OrchardCore.DataSources.Files;

/// <summary>
/// Describes a registered file format with the static members of its class, so formats are listed without creating
/// them. <c>services.AddDataFileFormat&lt;TFormat&gt;()</c> registers one per format.
/// </summary>
public sealed class DataFileFormatDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataFileFormatDescriptor"/> class.
    /// </summary>
    /// <param name="name">The stable technical name of the format.</param>
    /// <param name="displayName">The label shown to users.</param>
    /// <param name="extension">The file extension of the format, with its leading dot.</param>
    /// <param name="contentType">The media type of the format.</param>
    /// <param name="type">The class of the format, resolved from the services when a file is read or written.</param>
    public DataFileFormatDescriptor(string name, LocalizationSource displayName, string extension, string contentType, Type type)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(type);

        Name = name;
        DisplayName = displayName;
        Extension = extension;
        ContentType = contentType;
        Type = type;
    }

    /// <summary>
    /// Gets the stable technical name of the format.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the label shown to users, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    public LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets the file extension of the format, with its leading dot, such as <c>.csv</c>.
    /// </summary>
    public string Extension { get; }

    /// <summary>
    /// Gets the media type of the format, such as <c>text/csv</c>.
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Gets the class of the format, which implements <see cref="IDataFileFormat"/>.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets a value indicating whether the format writes several data sets to one file, as an
    /// <see cref="IDataWorkbookFormat"/>.
    /// </summary>
    public bool IsWorkbook => typeof(IDataWorkbookFormat).IsAssignableFrom(Type);

    /// <summary>
    /// Describes a format from the static members of its class.
    /// </summary>
    /// <typeparam name="TFormat">The class of the format.</typeparam>
    /// <returns>The descriptor.</returns>
    public static DataFileFormatDescriptor Create<TFormat>()
        where TFormat : class, IDataFileFormat
    {
        return new DataFileFormatDescriptor(TFormat.Name, TFormat.DisplayName, TFormat.Extension, TFormat.ContentType, typeof(TFormat));
    }
}

/// <summary>
/// Holds the registered file formats. <c>services.AddDataFileFormat&lt;TFormat&gt;()</c> adds them.
/// </summary>
public sealed class DataFileFormatOptions
{
    private readonly List<DataFileFormatDescriptor> _formats = [];

    /// <summary>
    /// Gets the registered formats, in the order they were registered.
    /// </summary>
    public IReadOnlyList<DataFileFormatDescriptor> Formats => _formats;

    /// <summary>
    /// Registers a format, unless its class is already registered.
    /// </summary>
    /// <param name="descriptor">The format.</param>
    /// <returns>The options.</returns>
    public DataFileFormatOptions Add(DataFileFormatDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (!_formats.Any(format => format.Type == descriptor.Type))
        {
            _formats.Add(descriptor);
        }

        return this;
    }
}
