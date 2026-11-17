using Microsoft.Extensions.FileProviders;

namespace OrchardCore.DisplayManagement.Liquid;

/// <summary>
/// Provides programmatic configuration for the <see cref="LiquidViewTemplate"/>.
/// </summary>
public class LiquidViewOptions
{
    private IFileProvider _fileProvider;

    /// <summary>
    /// Gets the sequence of <see cref="IFileProvider" /> instances used by <see cref="LiquidViewTemplate"/> to
    /// locate Liquid files.
    /// </summary>
    /// <remarks>
    /// At startup, this is initialized to include an instance of <see cref="PhysicalFileProvider"/> that is
    /// rooted at the application root.
    /// </remarks>
    public List<IFileProvider> FileProviders { get; } = [];

    /// <summary>
    /// Gets the <see cref="IFileProvider"/> locating Liquid files, which combines the <see cref="FileProviders"/>.
    /// </summary>
    public IFileProvider FileProvider => _fileProvider ??= FileProviders.Count switch
    {
        0 => new NullFileProvider(),
        1 => FileProviders[0],
        _ => new CompositeFileProvider(FileProviders),
    };

    public List<Action<LiquidViewParser>> LiquidViewParserConfiguration { get; } = [];
}
