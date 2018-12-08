using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Linq;
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
            var tenantSettingsPath = Path.Combine(
                        _options.Value.ShellsApplicationDataPath,
                        _options.Value.ShellsContainerName);

            if (!Directory.Exists(tenantSettingsPath))
            {
                return Enumerable.Empty<ShellSettings>();
            }

            var tenantFolders = Directory.GetDirectories(tenantSettingsPath);

            var shellSettings = new List<ShellSettings>();

            foreach(var tenantFolder in tenantFolders)
            {
                var tenantName = Path.GetDirectoryName(tenantFolder);

                var configurationRoot = BuildConfiguration(tenantName, ignoreAppSettings: false);

                var shellSetting = new ShellSettings();

                configurationRoot.Bind(shellSetting);
            }

            return shellSettings;
        }

        public void SaveSettings(ShellSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var tenantSettingsFilename = Path.Combine(
                        _options.Value.ShellsApplicationDataPath,
                        _options.Value.ShellsContainerName,
                        settings.Name,
                        "appsettings.json");

            var globalConfiguration = BuildConfiguration(settings.Name, true);
            var localConfiguration = BuildConfiguration(settings.Name, false);

            var localSettings = new ShellSettings();

            localSettings.Name = settings.Name;

            // We set app settings if the local settings have the settings or it's
            // not defined in the global ones.

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.ConnectionString)]) || !String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.ConnectionString)]) )
            {
                localSettings.ConnectionString = settings.ConnectionString;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.DatabaseProvider)]) || !String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.DatabaseProvider)]) )
            {
                localSettings.DatabaseProvider = settings.DatabaseProvider;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.RecipeName)]) || !String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.RecipeName)]) )
            {
                localSettings.RecipeName = settings.RecipeName;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.RequestUrlHost)]) || !String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.RequestUrlHost)]) )
            {
                localSettings.RequestUrlHost = settings.RequestUrlHost;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.RequestUrlPrefix)]) || !String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.RequestUrlPrefix)]) )
            {
                localSettings.RequestUrlPrefix = settings.RequestUrlPrefix;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.Secret)]) || !String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.Secret)]) )
            {
                localSettings.Secret = settings.Secret;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.State)]) || !String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.State)]) )
            {
                localSettings.State = settings.State;
            }

            if (!String.IsNullOrEmpty(localConfiguration[nameof(ShellSettings.TablePrefix)]) || !String.IsNullOrEmpty(globalConfiguration[nameof(ShellSettings.TablePrefix)]) )
            {
                localSettings.TablePrefix = settings.TablePrefix;
            }

            File.WriteAllText(tenantSettingsFilename, JsonConvert.SerializeObject(localSettings));
        }

        private IConfigurationRoot BuildConfiguration(string tenantName, bool ignoreAppSettings)
        {
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddEnvironmentVariables();
            configurationBuilder.AddEnvironmentVariables(tenantName.ToUpperInvariant() + "_");
            configurationBuilder.AddEnvironmentVariables(tenantName.ToUpperInvariant() + "_" + _hostingEnvironment.EnvironmentName);
            configurationBuilder.AddJsonFile("appsettings.tenants.json", optional: true);
            configurationBuilder.AddJsonFile($"appsettings.tenants.{_hostingEnvironment.EnvironmentName}.json", optional: true);

            if (!ignoreAppSettings)
            {
                configurationBuilder.AddJsonFile(Path.Combine(tenantName, "appsettings.json"), optional: true);
            }

            configurationBuilder.AddJsonFile(Path.Combine(tenantName, $"appsettings.{_hostingEnvironment.EnvironmentName}.json"), optional: true);

            foreach(var configurationProvider in _configurationProviders)
            {
                configurationProvider.Configure(tenantName, configurationBuilder);
            }

            return configurationBuilder.Build();
        }
    }
}