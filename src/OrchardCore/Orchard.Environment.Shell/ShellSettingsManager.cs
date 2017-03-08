using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Parser;
using Orchard.Parser.Yaml;
using System.Collections.Generic;
using System.IO;
using System;

namespace Orchard.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<ShellOptions> _optionsAccessor;
        private readonly ILogger _logger;

        private const string SettingsFileNameFormat = "Settings.{0}";

        public ShellSettingsManager(
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> optionsAccessor,
            ILogger<ShellSettingsManager> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _optionsAccessor = optionsAccessor;
            _logger = logger;
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings()
        {
            var shellSettings = new List<ShellSettings>();

            foreach (var tenant in 
                _hostingEnvironment.ContentRootFileProvider.GetDirectoryContents(
                    Path.Combine(
                        _optionsAccessor.Value.ShellsRootContainerName, 
                        _optionsAccessor.Value.ShellsContainerName)))
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("ShellSettings found in '{0}', attempting to load.", tenant.Name);
                }
                var configurationContainer =
                    new ConfigurationBuilder()
                        .SetBasePath(tenant.PhysicalPath)
                        .AddJsonFile(string.Format(SettingsFileNameFormat, "json"),
                            true)
                        .AddXmlFile(string.Format(SettingsFileNameFormat, "xml"),
                            true)
                        .AddYamlFile(string.Format(SettingsFileNameFormat, "txt"),
                            false);

                var config = configurationContainer.Build();
                var shellSetting = ShellSettingsSerializer.ParseSettings(config);
                shellSettings.Add(shellSetting);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Loaded ShellSettings for tenant '{0}'", shellSetting.Name);
                }
            }

            return shellSettings;
        }

        void IShellSettingsManager.SaveSettings(ShellSettings shellSettings)
        {
            if (shellSettings == null)
            {
                throw new ArgumentNullException(nameof(shellSettings));
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Saving ShellSettings for tenant '{0}'", shellSettings.Name);
            }

            var tenantPath = 
                Path.Combine(
                    _hostingEnvironment.ContentRootPath,
                    _optionsAccessor.Value.ShellsRootContainerName,
                    _optionsAccessor.Value.ShellsContainerName, 
                    shellSettings.Name, 
                    string.Format(SettingsFileNameFormat, "txt"));

            var configurationProvider = new YamlConfigurationProvider(new YamlConfigurationSource
            {
                Path = tenantPath,
                Optional = false
            });

            foreach (var key in shellSettings.Keys)
            {
                if (!string.IsNullOrEmpty(shellSettings[key]))
                {
                    configurationProvider.Set(key, shellSettings[key]);
                }
            }

            configurationProvider.Commit();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Saved ShellSettings for tenant '{0}'", shellSettings.Name);
            }
        }
    }
}