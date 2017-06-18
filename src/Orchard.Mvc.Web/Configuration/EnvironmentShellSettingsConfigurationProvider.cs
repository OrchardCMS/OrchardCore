using System.Collections.Generic;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell;

namespace OrchardCore.Mvc.Configuration
{
    /*
        1. Still use built in Site configuration in app_data
        2. Environmnet config can be used to override tenants.
    */
    public class EnvironmentShellSettingsConfigurationProvider : IShellSettingsConfigurationProvider
    {
        public int Order => 1;


        public void AddSource(IConfigurationBuilder builder)
        {
            var config = builder.Build();

            // Get all tenant names
            // Get all Tenants with a tenant:AWS
            // Add AWS KeyManageement to each one
            // done.

            foreach (var tenant in config.GetChildren())
            {
                builder.AddEnvironmentVariables(tenant.Key);
            }
        }

        public void SaveToSource(string name, IDictionary<string, string> configuration)
        {
            // No saving back to Environment
        }
    }

    public static class TenantExtensions
    {
        public static ModularServiceCollection WithEnvironmentConfiguration(this ModularServiceCollection modules)
        {
            modules.Configure(services =>
            {
                services.AddScoped<IShellSettingsConfigurationProvider, EnvironmentShellSettingsConfigurationProvider>();
            });

            return modules;
        }
    }
}
