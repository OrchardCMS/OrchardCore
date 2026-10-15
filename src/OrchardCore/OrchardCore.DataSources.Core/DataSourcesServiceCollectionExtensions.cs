using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Expressions;
using OrchardCore.DataSources.Files;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers data sources, data file formats and the functions of formulas.
/// </summary>
public static class DataSourcesServiceCollectionExtensions
{
    /// <summary>
    /// Registers the data source manager, the data file format manager, the built-in CSV, JSON, JSON Lines and Excel
    /// formats, and the catalog of the functions of formulas with the built-in functions.
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

        services.TryAddSingleton<IExpressionFunctionCatalog, ExpressionFunctionCatalog>();
        services.AddExpressionFunctionProvider<BuiltInExpressionFunctionProvider>();

        return services;
    }

    /// <summary>
    /// Registers a provider of functions formulas can call, as a singleton.
    /// </summary>
    /// <typeparam name="TProvider">The provider type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddExpressionFunctionProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IExpressionFunctionProvider
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IExpressionFunctionProvider, TProvider>());

        return services;
    }

    /// <summary>
    /// Registers a function formulas can call. A function replaces a function of the same name registered before it,
    /// such as a built-in function.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="function">The function, created with <see cref="ExpressionFunction.CreateScalar"/> or
    /// <see cref="ExpressionFunction.CreateAggregate"/>.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddExpressionFunction(this IServiceCollection services, ExpressionFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        services.AddSingleton<IExpressionFunctionProvider>(new ExpressionFunctionProvider(function));

        return services;
    }

    private sealed class ExpressionFunctionProvider : IExpressionFunctionProvider
    {
        private readonly ExpressionFunction _function;

        public ExpressionFunctionProvider(ExpressionFunction function)
        {
            _function = function;
        }

        public IEnumerable<ExpressionFunction> GetFunctions() => [_function];
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
        services.TryAddScoped<TDataSource>();
        services.Configure<DataSourceOptions>(options => options.Add(DataSourceDescriptor.Create<TDataSource>()));

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
        services.TryAddSingleton<TFormat>();
        services.Configure<DataFileFormatOptions>(options => options.Add(DataFileFormatDescriptor.Create<TFormat>()));

        return services;
    }
}
