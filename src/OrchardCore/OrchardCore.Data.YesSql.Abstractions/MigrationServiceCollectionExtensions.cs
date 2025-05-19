using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Data.Migration;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to add YesSql migration <see cref="IDataMigration"/>.
/// </summary>
public static class MigrationServiceCollectionExtensions
{
    public static IServiceCollection AddDataMigration<TDataMigration>(this IServiceCollection services)
        where TDataMigration : class, IDataMigration
    {
        services.TryAddEnumerable(new[]
        {
            ServiceDescriptor.Scoped<IDataMigration, TDataMigration>(),
        });

        return services;
    }
}
