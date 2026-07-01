using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Data;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to add YesSQL <see cref="DatabaseProvider"/>s.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The database provider name.</param>
    /// <param name="value">The database provider value, i.e. SQL Server, MySQL.</param>
    /// <param name="hasConnectionString">Whether the database contains a connection string.</param>
    /// <param name="hasTablePrefix">The table prefix.</param>
    /// <param name="isDefault">Whether the data provider is the default one.</param>
    /// <param name="sampleConnectionString">A sample connection string, e.g. Server={Server Name};Database={Database Name};IntegratedSecurity=true.</param>
    /// <returns></returns>
    public static IServiceCollection TryAddDataProvider(this IServiceCollection services, string name, string value, bool hasConnectionString, bool hasTablePrefix, bool isDefault, string sampleConnectionString = "")
    {
        for (var i = services.Count - 1; i >= 0; i--)
        {
            var entry = services[i];
            var implementationInstance = entry.GetImplementationInstance();
            if (implementationInstance is not null)
            {
                var databaseProvider = implementationInstance as DatabaseProvider;
                if (databaseProvider is not null && string.Equals(databaseProvider.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    services.RemoveAt(i);
                }
            }
        }

        services.AddSingleton(new DatabaseProvider { Name = name, Value = value, HasConnectionString = hasConnectionString, HasTablePrefix = hasTablePrefix, IsDefault = isDefault, SampleConnectionString = sampleConnectionString });

        return services;
    }

    /// <summary>
    /// Pre-registers a document type with the store so that YesSql can resolve its persisted type
    /// name during a by-id or polymorphic read, even before that type has first been saved or
    /// queried in the current process. Call this from a module's <c>ConfigureServices</c> for every
    /// type it stores but may load through a base type or by id.
    /// </summary>
    /// <typeparam name="TDocument">The document type to pre-register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    public static IServiceCollection AddDocumentType<TDocument>(this IServiceCollection services)
        => services.AddDocumentType(typeof(TDocument));

    /// <summary>
    /// Pre-registers a document type with the store so that YesSql can resolve its persisted type
    /// name during a by-id or polymorphic read, even before that type has first been saved or
    /// queried in the current process. Call this from a module's <c>ConfigureServices</c> for every
    /// type it stores but may load through a base type or by id.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="documentType">The document type to pre-register.</param>
    public static IServiceCollection AddDocumentType(this IServiceCollection services, Type documentType)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        services.Configure<DocumentTypeOptions>(options => options.Types.Add(documentType));

        return services;
    }
}
