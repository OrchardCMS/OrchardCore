using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Data.Migration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to add YesSql migration <see cref="IDataMigration"/>.
/// </summary>
public static class MigrationServiceCollectionExtensions
{
    public static IServiceCollection AddDataMigration<TDataMigration>(this IServiceCollection services)
        where TDataMigration : class, IDataMigration
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataMigration, TDataMigration>());

        return services;
    }
}
