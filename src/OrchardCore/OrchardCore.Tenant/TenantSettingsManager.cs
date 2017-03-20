using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Parser;
using Orchard.Parser.Yaml;
using System.Collections.Generic;
using System.IO;
using System;

namespace OrchardCore.Tenant
{
    public class TenantSettingsManager : ITenantSettingsManager
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<TenantOptions> _optionsAccessor;
        private readonly ILogger _logger;

        private const string SettingsFileNameFormat = "Settings.{0}";

        public TenantSettingsManager(
            IHostingEnvironment hostingEnvironment,
            IOptions<TenantOptions> optionsAccessor,
            ILogger<TenantSettingsManager> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _optionsAccessor = optionsAccessor;
            _logger = logger;
        }

        IEnumerable<TenantSettings> ITenantSettingsManager.LoadSettings()
        {
            var tenantSettings = new List<TenantSettings>();

            foreach (var tenant in
                _hostingEnvironment.ContentRootFileProvider.GetDirectoryContents(
                    Path.Combine(
                        _optionsAccessor.Value.TenantsRootContainerName,
                        _optionsAccessor.Value.TenantsContainerName)))
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("TenantSettings found in '{0}', attempting to load.", tenant.Name);
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
                var tenantSetting = TenantSettingsSerializer.ParseSettings(config);
                tenantSettings.Add(tenantSetting);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Loaded TenantSettings for tenant '{0}'", tenantSetting.Name);
                }
            }

            return tenantSettings;
        }

        void ITenantSettingsManager.SaveSettings(TenantSettings tenantSettings)
        {
            if (tenantSettings == null)
            {
                throw new ArgumentNullException(nameof(tenantSettings));
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Saving TenantSettings for tenant '{0}'", tenantSettings.Name);
            }

            var tenantPath =
                Path.Combine(
                    _hostingEnvironment.ContentRootPath,
                    _optionsAccessor.Value.TenantsRootContainerName,
                    _optionsAccessor.Value.TenantsContainerName,
                    tenantSettings.Name,
                    string.Format(SettingsFileNameFormat, "txt"));

            var configurationProvider = new YamlConfigurationProvider(new YamlConfigurationSource
            {
                Path = tenantPath,
                Optional = false
            });

            foreach (var key in tenantSettings.Keys)
            {
                if (!string.IsNullOrEmpty(tenantSettings[key]))
                {
                    configurationProvider.Set(key, tenantSettings[key]);
                }
            }

            configurationProvider.Commit();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Saved TenantSettings for tenant '{0}'", tenantSettings.Name);
            }
        }
    }
}