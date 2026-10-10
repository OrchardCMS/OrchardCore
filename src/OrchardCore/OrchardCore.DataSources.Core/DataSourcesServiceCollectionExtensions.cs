using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Files;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers data sources and data file formats.
/// </summary>
public static class DataSourcesServiceCollectionExtensions
{
    /// <summary>
    /// Registers the data source manager, the data file format manager and the built-in CSV, JSON, JSON Lines and
    /// Excel formats.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataSourcesCore(this IServiceCollection services)
    {
        services.TryAddScoped<IDataSourceManager, DataSourceManager>();
        services.TryAddSingleton<IDataFileFormatManager, DataFileFormatManager>();

        services.AddDataFileFormat<CsvDataFileFormat>();
        services.AddDataFileFormat<JsonDataFileFormat>();
        services.AddDataFileFormat<JsonLinesDataFileFormat>();
        services.AddDataFileFormat<ExcelDataFileFormat>();

        return services;
    }

    /// <summary>
    /// Registers a data source, as a scoped service.
    /// </summary>
    /// <typeparam name="TDataSource">The data source type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataSource<TDataSource>(this IServiceCollection services)
        where TDataSource : class, IDataSource
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSource, TDataSource>());

        return services;
    }

    /// <summary>
    /// Registers a data file format, as a singleton.
    /// </summary>
    /// <typeparam name="TFormat">The format type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataFileFormat<TFormat>(this IServiceCollection services)
        where TFormat : class, IDataFileFormat
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDataFileFormat, TFormat>());

        return services;
    }
}
