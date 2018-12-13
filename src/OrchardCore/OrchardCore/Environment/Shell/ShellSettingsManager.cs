using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace OrchardCore.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly string _tenantsContainerPath;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<ShellOptions> _options;

        public ShellSettingsManager(
            IEnumerable<ITenantsConfigurationProvider> configurationProviders,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> options)
        {
            _hostingEnvironment = hostingEnvironment;
            _options = options;

            _tenantsContainerPath = Path.Combine(
                _options.Value.ShellsApplicationDataPath,
                _options.Value.ShellsContainerName);

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(_hostingEnvironment.ContentRootPath)
                .AddEnvironmentVariables("APPSETTINGS_TENANTS_")
                .AddEnvironmentVariables($"APPSETTINGS_{_hostingEnvironment.EnvironmentName.ToUpperInvariant()}_TENANTS_")
                .AddJsonFile("appsettings.tenants.json", optional: true)
                .AddJsonFile($"appsettings.{_hostingEnvironment.EnvironmentName}.tenants.json", optional: true);

            foreach (var configurationProvider in configurationProviders.OrderBy(p => p.Order))
            {
                configurationBuilder.AddConfiguration(configurationProvider.Configuration);
            }

            Configuration = configurationBuilder.Build();
        }

        public IConfiguration Configuration { get; }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            var configuredTenants = Configuration.GetChildren()
                .Where(section => section.GetValue<string>("State") != null)
                .Select(section => section.Key).ToArray();

            foreach (var tenant in configuredTenants)
            {
                Directory.CreateDirectory(Path.Combine(_tenantsContainerPath, tenant));
            }

            if (!Directory.Exists(Path.Combine(_tenantsContainerPath, ShellHelper.DefaultShellName)))
            {
                return Enumerable.Empty<ShellSettings>();
            }

            var tenantFolders = Directory.GetDirectories(_tenantsContainerPath);

            var shellSettings = new List<ShellSettings>();

            foreach (var tenantFolder in tenantFolders)
            {
                var tenantName = Path.GetFileName(tenantFolder);

                var localConfiguration = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(tenantFolder, "settings.json"), optional: true)
                    .Build();

                var shellSetting = new ShellSettings() { Name = tenantName };

                Configuration.Bind(shellSetting);
                Configuration.Bind(tenantName, shellSetting);
                localConfiguration.Bind(shellSetting);

                shellSettings.Add(shellSetting);
            }

            return shellSettings;
        }

        public void SaveSettings(ShellSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var tenantFolder = Path.Combine(_tenantsContainerPath, settings.Name);
            Directory.CreateDirectory(tenantFolder);

            var shellSettings = new ShellSettings() { Name = settings.Name };

            Configuration.Bind(shellSettings);
            Configuration.Bind(settings.Name, shellSettings);

            if (shellSettings.RequestUrlHost != settings.RequestUrlHost)
            {
                shellSettings.RequestUrlHost = settings.RequestUrlHost;
            }

            if (shellSettings.RequestUrlPrefix != settings.RequestUrlPrefix)
            {
                shellSettings.RequestUrlPrefix = settings.RequestUrlPrefix;
            }

            if (shellSettings.DatabaseProvider != settings.DatabaseProvider)
            {
                shellSettings.DatabaseProvider = settings.DatabaseProvider;
            }

            if (shellSettings.TablePrefix != settings.TablePrefix)
            {
                shellSettings.TablePrefix = settings.TablePrefix;
            }

            if (shellSettings.ConnectionString != settings.ConnectionString)
            {
                shellSettings.ConnectionString = settings.ConnectionString;
            }

            if (shellSettings.RecipeName != settings.RecipeName)
            {
                shellSettings.RecipeName = settings.RecipeName;
            }

            if (shellSettings.Secret != settings.Secret)
            {
                shellSettings.Secret = settings.Secret;
            }

            if (shellSettings.State != settings.State)
            {
                shellSettings.State = settings.State;
            }

            try
            {
                File.WriteAllText(Path.Combine(tenantFolder, "settings.json"), JsonConvert.SerializeObject(shellSettings, Formatting.Indented));
            }

            catch (IOException) { }
        }
    }
}