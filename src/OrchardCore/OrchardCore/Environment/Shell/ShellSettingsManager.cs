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
        private readonly IEnumerable<IShellSettingsConfigurationProvider> _configurationProviders;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<ShellOptions> _options;

        public ShellSettingsManager(
            IEnumerable<IShellSettingsConfigurationProvider> configurationProviders,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> options)
        {
            _configurationProviders = configurationProviders.OrderBy(x => x.Order);
            _hostingEnvironment = hostingEnvironment;
            _options = options;
        }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            var tenantContainerPath = Path.Combine(
                _options.Value.ShellsApplicationDataPath,
                _options.Value.ShellsContainerName);

            if (!Directory.Exists(tenantContainerPath))
            {
                return Enumerable.Empty<ShellSettings>();
            }

            var tenantFolders = Directory.GetDirectories(tenantContainerPath);

            var shellSettings = new List<ShellSettings>();

            foreach (var tenantFolder in tenantFolders)
            {
                var configurationRoot = BuildConfiguration(tenantFolder, ignoreAppSettings: false);

                var shellSetting = new ShellSettings();

                configurationRoot.Bind(shellSetting);

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

            var tenantFolder = Path.Combine(
                _options.Value.ShellsApplicationDataPath,
                _options.Value.ShellsContainerName,
                settings.Name);

            var globalConfiguration = BuildConfiguration(tenantFolder, ignoreAppSettings: true);
            var localConfiguration = BuildConfiguration(tenantFolder, ignoreAppSettings: false);

            var localSettings = new ShellSettings();

            localSettings.Name = settings.Name;

            // We set app settings if the local settings have the settings or it's
            // not defined in the global ones.

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.ConnectionString)]) || String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.ConnectionString)]))
            {
                localSettings.ConnectionString = settings.ConnectionString;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.DatabaseProvider)]) || String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.DatabaseProvider)]))
            {
                localSettings.DatabaseProvider = settings.DatabaseProvider;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.RecipeName)]) || String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.RecipeName)]))
            {
                localSettings.RecipeName = settings.RecipeName;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.RequestUrlHost)]) || String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.RequestUrlHost)]))
            {
                localSettings.RequestUrlHost = settings.RequestUrlHost;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.RequestUrlPrefix)]) || String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.RequestUrlPrefix)]))
            {
                localSettings.RequestUrlPrefix = settings.RequestUrlPrefix;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.Secret)]) || String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.Secret)]))
            {
                localSettings.Secret = settings.Secret;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.State)]) || String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.State)]))
            {
                localSettings.State = settings.State;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.TablePrefix)]) || String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.TablePrefix)]))
            {
                localSettings.TablePrefix = settings.TablePrefix;
            }

            Directory.CreateDirectory(tenantFolder);

            File.WriteAllText(Path.Combine(tenantFolder, "appsettings.json"), JsonConvert.SerializeObject(localSettings));
        }

        private IConfigurationRoot BuildConfiguration(string tenantFolder, bool ignoreAppSettings)
        {
            var tenantName = Path.GetFileName(tenantFolder);

            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddEnvironmentVariables();
            configurationBuilder.AddEnvironmentVariables(tenantName.ToUpperInvariant() + "_");
            configurationBuilder.AddEnvironmentVariables(tenantName.ToUpperInvariant() + "_" + _hostingEnvironment.EnvironmentName);
            configurationBuilder.AddJsonFile("appsettings.tenants.json", optional: true);
            configurationBuilder.AddJsonFile($"appsettings.tenants.{_hostingEnvironment.EnvironmentName}.json", optional: true);

            if (!ignoreAppSettings)
            {
                configurationBuilder.AddJsonFile(Path.Combine(tenantFolder, "appsettings.json"), optional: true);
            }

            configurationBuilder.AddJsonFile(Path.Combine(tenantFolder, $"appsettings.{_hostingEnvironment.EnvironmentName}.json"), optional: true);

            foreach (var configurationProvider in _configurationProviders)
            {
                configurationProvider.Configure(tenantName, configurationBuilder);
            }

            return configurationBuilder.Build();
        }
    }
}