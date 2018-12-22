using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly string _tenantsFilePath;
        private readonly IEnumerable<string> _configuredTenants;

        public ShellSettingsManager(
            IConfiguration applicationConfiguration,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> options)
        {
            var appDataPath = options.Value.ShellsApplicationDataPath;

            var containerPath = Path.Combine(appDataPath, options.Value.ShellsContainerName);
            Directory.CreateDirectory(containerPath);

            _tenantsFilePath = Path.Combine(containerPath, "tenants.json");

            var environment = hostingEnvironment.EnvironmentName;
            var ENVIRONMENT = environment.ToUpperInvariant();
            var appsettings = Path.Combine(appDataPath, "appsettings");

            var configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(applicationConfiguration)

                .AddJsonFile($"{appsettings}.json", optional: true)
                .AddJsonFile($"{appsettings}.{environment}.json", optional: true)

                .AddEnvironmentVariables("ORCHARDCORE_")
                .AddEnvironmentVariables($"ORCHARDCORE_{ENVIRONMENT}_");

            var commandLineProvider = (applicationConfiguration as IConfigurationRoot)?
                .Providers.FirstOrDefault(p => p is CommandLineConfigurationProvider);

            if (commandLineProvider != null)
            {
                configurationBuilder.AddConfiguration(new ConfigurationRoot(new[] { commandLineProvider }));
            }

            Configuration = configurationBuilder.Build();

            _configuredTenants = Configuration.GetSection("Tenants").GetChildren()
                .Where(section => section.GetValue<string>("State") != null)
                .Select(section => section.Key).Distinct().ToArray();
        }

        public IConfiguration Configuration { get; }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            var localConfiguration = new ConfigurationBuilder()
                .AddJsonFile(_tenantsFilePath, optional: true)
                .Build();

            var localTenants = localConfiguration.GetChildren().Select(section => section.Key);
            var tenants = _configuredTenants.Concat(localTenants).Distinct().ToArray();

            var shellsSettings = new ConcurrentBag<ShellSettings>();

            Parallel.ForEach(tenants, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (tenant) =>
            {
                var shellSettings = new ShellSettings() { Name = tenant };

                Configuration.Bind("Tenants", shellSettings);
                Configuration.GetSection("Tenants").Bind(tenant, shellSettings);
                localConfiguration.Bind(tenant, shellSettings);

                shellsSettings.Add(shellSettings);
            });

            return shellsSettings;
        }

        public void SaveSettings(ShellSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var shellSettings = new ShellSettings() { Name = settings.Name };

            Configuration.Bind("Tenants", shellSettings);
            Configuration.GetSection("Tenants").Bind(settings.Name, shellSettings);

            var localObject = JObject.FromObject(settings);
            var globalObject = JObject.FromObject(shellSettings);

            localObject.Remove("Name");

            if (settings.State != Models.TenantState.Uninitialized)
            {
                localObject.Remove("Secret");
                localObject.Remove("RecipeName");
            }

            foreach (var property in globalObject)
            {
                var localValue = localObject.Value<string>(property.Key);
                var globalValue = globalObject.Value<string>(property.Key);

                if (localValue == null || globalValue == localValue)
                {
                    localObject.Remove(property.Key);
                }
            }

            lock (this)
            {
                try
                {
                    var localConfiguration = JObject.Parse(File.ReadAllText(_tenantsFilePath));

                    localConfiguration[settings.Name] = localObject;

                    File.WriteAllText(_tenantsFilePath, localConfiguration.ToString());
                }

                catch (IOException)
                {
                }
            }
        }
    }
}