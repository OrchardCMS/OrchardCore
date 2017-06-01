using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection TryAddDataProvider(this IServiceCollection services, string name, string value, bool hasConnectionString = false)
        {
            var service = services.SingleOrDefault(s => s.ImplementationInstance is DatabaseProvider databaseProvider &&
                string.Equals(databaseProvider.Name, name, StringComparison.OrdinalIgnoreCase));

            if (service != null)
            {
                var descriptor = new ServiceDescriptor(typeof(DatabaseProvider), service);
                services.Remove(descriptor);
                services.AddSingleton(new DatabaseProvider { Name = name, Value = value, HasConnectionString = hasConnectionString });
            }

            return services;
        }
    }
}
