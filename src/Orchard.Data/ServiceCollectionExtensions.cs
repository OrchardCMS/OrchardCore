using Microsoft.Extensions.DependencyInjection;
using Orchard.Data.Migration;
using Orchard.Environment.Shell;

namespace Orchard.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services)
        {
            services.AddScoped<IDataMigrationManager, DataMigrationManager>();

            services.AddScoped<AutomaticDataMigrations>();
            services.AddScoped<IOrchardShellEvents>(sp => sp.GetRequiredService<AutomaticDataMigrations>());

            return services;
        }
    }
}