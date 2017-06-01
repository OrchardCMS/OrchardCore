using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection TryAddDataProvider(this IServiceCollection services, string name, string value, bool hasConnectionString = false)
        {
            var serviceProvider = services.BuildServiceProvider();
            var databaseProvider = serviceProvider.GetServices<DatabaseProvider>()
                .SingleOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

            if (databaseProvider != null)
            {
                var serviceDescriptor = new ServiceDescriptor(typeof(DatabaseProvider), databaseProvider);
                services.Remove(serviceDescriptor);
                services.AddSingleton(new DatabaseProvider { Name = name, Value = value, HasConnectionString = hasConnectionString });
            }

            return services;
        }
    }
}
