namespace OrchardCore.DataOrchestrator.Exporting;

/// <summary>
/// Provides access to the registered <see cref="IEtlExportFormat"/> instances.
/// </summary>
public interface IEtlExportFormatProvider
{
    /// <summary>
    /// Returns all the registered export formats.
    /// </summary>
    IEnumerable<IEtlExportFormat> ListFormats();

    /// <summary>
    /// Returns the export format with the specified name, or <c>null</c> when no match is found.
    /// </summary>
    IEtlExportFormat GetFormat(string name);
}
