using Microsoft.Extensions.DependencyInjection;
using Orchard.Data.Migration;

namespace Orchard.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services)
        {
            services.AddScoped<IDataMigrationManager, DataMigrationManager>();

            return services;
        }
    }
}