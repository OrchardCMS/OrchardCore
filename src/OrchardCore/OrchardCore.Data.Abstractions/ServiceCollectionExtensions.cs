using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Data
{
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
        /// <param name="sampleConnectionString">A sample connection string, e.g. Server={Server Name};Database={Database Name};IntegratedSecurity=true</param>
        /// <returns></returns>
        public static IServiceCollection TryAddDataProvider(this IServiceCollection services, string name, string value, bool hasConnectionString, bool hasTablePrefix, bool isDefault, string sampleConnectionString = "")
        {
            for (var i = services.Count - 1; i >= 0; i--)
            {
                var entry = services[i];
                if (entry.ImplementationInstance != null)
                {
                    var databaseProvider = entry.ImplementationInstance as DatabaseProvider;
                    if (databaseProvider != null && String.Equals(databaseProvider.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        services.RemoveAt(i);
                    }
                }
            }

            services.AddSingleton(new DatabaseProvider { Name = name, Value = value, HasConnectionString = hasConnectionString, HasTablePrefix = hasTablePrefix, IsDefault = isDefault, SampleConnectionString = sampleConnectionString });

            return services;
        }
    }
}
