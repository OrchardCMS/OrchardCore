using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly string _defaultTenantFolder;
        private readonly IEnumerable<string> _configuredTenants;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShellSettingsManager(
            IHttpContextAccessor httpContextAccessor,
            IConfiguration applicationConfiguration,
            IEnumerable<ITenantsGlobalConfigurationSource> globalConfigurationSources,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> options)
        {
            _httpContextAccessor = httpContextAccessor;

            _defaultTenantFolder = Path.Combine(
                options.Value.ShellsApplicationDataPath,
                options.Value.ShellsContainerName,
                ShellHelper.DefaultShellName);

            Directory.CreateDirectory(_defaultTenantFolder);

            var environmentName = hostingEnvironment.EnvironmentName.ToUpperInvariant();
            var applicationName = hostingEnvironment.ApplicationName.Replace('.', '_').ToUpperInvariant();
            var appsettingsPath = Path.Combine(options.Value.ShellsApplicationDataPath, "appsettings");

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(hostingEnvironment.ContentRootPath)

                .AddEnvironmentVariables($"{applicationName}_SETTINGS_")
                .AddEnvironmentVariables($"{applicationName}_{environmentName}_SETTINGS_")

                .AddJsonFile($"{appsettingsPath}.json", optional: true)
                .AddJsonFile($"{appsettingsPath}.{hostingEnvironment.EnvironmentName}.json", optional: true);

            var commandLineProvider = (applicationConfiguration as IConfigurationRoot)?
                .Providers.FirstOrDefault(p => p is CommandLineConfigurationProvider);

            foreach (var source in globalConfigurationSources.OrderBy(s => s.Order))
            {
                configurationBuilder.Add(source);
            }

            if (commandLineProvider != null)
            {
                configurationBuilder.AddConfiguration(new ConfigurationRoot(new[] { commandLineProvider }));
            }

            GlobalConfiguration = configurationBuilder.Build();

            _configuredTenants = GlobalConfiguration.GetChildren()
                .Where(section => section.GetValue<string>("State") != null)
                .Select(section => section.Key)
                .Distinct()
                .ToArray();
        }

        public IConfiguration GlobalConfiguration { get; }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            var defaultSettings = LoadDefaultSettings();

            var shellHost = _httpContextAccessor.HttpContext.RequestServices.GetService<IShellHost>();

            var scope = defaultSettings.State == TenantState.Running ? shellHost
                .GetScopeAsync(defaultSettings).GetAwaiter().GetResult() : null;

            using (scope)
            {
                var configurationBuilder = new ConfigurationBuilder();

                if (scope != null)
                {
                    var sources = scope.ServiceProvider.GetServices<ITenantsLocalConfigurationSource>();

                    foreach (var source in sources.OrderBy(s => s.Order))
                    {
                        configurationBuilder.Add(source);
                    }
                }

                var localConfiguration = configurationBuilder.Build();

                var tenants = _configuredTenants
                    .Concat(localConfiguration.GetChildren().Select(section => section.Key))
                    .Distinct();

                var shellSettings = new List<ShellSettings>(new[] { defaultSettings });

                foreach (var tenant in tenants)
                {
                    if (tenant == ShellHelper.DefaultShellName)
                    {
                        continue;
                    }

                    var shellSetting = new ShellSettings() { Name = tenant };

                    // Bind root and section settings.
                    GlobalConfiguration.Bind(shellSetting);
                    GlobalConfiguration.Bind(tenant, shellSetting);
                    localConfiguration.Bind(tenant, shellSetting);

                    shellSettings.Add(shellSetting);
                }

                return shellSettings;
            }
        }

        private ShellSettings LoadDefaultSettings()
        {
            var localConfiguration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(_defaultTenantFolder, "appsettings.json"), optional: true)
                .Build();

            var shellSettings = new ShellSettings() { Name = ShellHelper.DefaultShellName };

            // Bind root and section settings.
            GlobalConfiguration.Bind(shellSettings);
            GlobalConfiguration.Bind(ShellHelper.DefaultShellName, shellSettings);
            localConfiguration.Bind(shellSettings);

            return shellSettings;
        }

        public void SaveSettings(ShellSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var globalSettings = new ShellSettings() { Name = settings.Name };

            // Bind root and section settings.
            GlobalConfiguration.Bind(globalSettings);
            GlobalConfiguration.Bind(settings.Name, globalSettings);

            var localObject = JObject.FromObject(settings);
            var globalObject = JObject.FromObject(globalSettings);

            foreach (var property in globalObject)
            {
                if (property.Key != "Name")
                {
                    var localValue = localObject.Value<string>(property.Key);
                    var globalValue = globalObject.Value<string>(property.Key);

                    // Only keep non null values and that override the global configuration.
                    // Allow e.g to setup a tenant in a pre-configured 'Uninitialized' state.

                    if (localValue == null || globalValue == localValue)
                    {
                        localObject.Remove(property.Key);
                    }
                }
            }

            if (settings.Name != ShellHelper.DefaultShellName)
            {
                var shellHost = _httpContextAccessor.HttpContext.RequestServices.GetService<IShellHost>();

                if (!shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var defaultSettings) ||
                    defaultSettings.State != TenantState.Running)
                {
                    return;
                }

                // Locking for unit tests using 'Sqlite' that doesn't support concurrent writes.
                lock (this)
                {
                    using (var scope = shellHost.GetScopeAsync(ShellHelper.DefaultShellName).GetAwaiter().GetResult())
                    {
                        var sources = scope.ServiceProvider.GetServices<ITenantsLocalConfigurationSource>();

                        foreach (var source in sources)
                        {
                            source.SaveSettings(settings.Name, localObject);
                        }
                    }
                }

                return;
            }

            try
            {
                File.WriteAllText(Path.Combine(_defaultTenantFolder, "appsettings.json"), localObject.ToString());
            }

            catch (IOException)
            {
                // The file may be own by another process or already exists when trying to create a new one.
            }
        }
    }
}