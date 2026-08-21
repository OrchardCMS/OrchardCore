using OrchardCore.DataOrchestrator.Exporting;

namespace OrchardCore.DataOrchestrator.Services;

/// <summary>
/// Default implementation of <see cref="IEtlExportFormatProvider"/> backed by the
/// <see cref="IEtlExportFormat"/> instances registered in the service container.
/// </summary>
public sealed class EtlExportFormatProvider : IEtlExportFormatProvider
{
    private readonly IEnumerable<IEtlExportFormat> _formats;

    public EtlExportFormatProvider(IEnumerable<IEtlExportFormat> formats)
    {
        _formats = formats;
    }

    /// <inheritdoc />
    public IEnumerable<IEtlExportFormat> ListFormats()
    {
        return _formats
            .OrderBy(x => x.DisplayText, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <inheritdoc />
    public IEtlExportFormat GetFormat(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return _formats.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
