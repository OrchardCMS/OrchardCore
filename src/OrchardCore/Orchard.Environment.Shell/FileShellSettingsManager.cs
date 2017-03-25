using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Orchard.Environment.Shell
{
    public class FileShellSettingsManager : IShellSettingsManager
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;

        public FileShellSettingsManager(IHostingEnvironment hostingEnvironment, ILogger<ShellSettingsManager> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings()
        {
            var tenantsFile = _hostingEnvironment.ContentRootFileProvider.GetFileInfo("tenants.json");

            // If no tenants file is found, return a single default tenant
            if (!tenantsFile.Exists)
            {
                return new ShellSettings[] { new ShellSettingsWithTenants { Name = ShellHelper.DefaultShellName, State = Models.TenantState.Running } };
            }

            var shellSettings = JsonConvert.DeserializeObject<Dictionary<string, ShellSettingsWithTenants>>(File.ReadAllText(tenantsFile.PhysicalPath));
            
            foreach(var shellSetting in shellSettings)
            {
                shellSetting.Value.Name = shellSetting.Key;
            }

            return shellSettings.Values;
        }

        void IShellSettingsManager.SaveSettings(ShellSettings shellSettings)
        {
            
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Saved ShellSettings for tenant '{0}'", shellSettings.Name);
            }
        }
    }

    public class ShellSettingsWithTenants : ShellSettings
    {
        public string[] Features { get; set; } = Array.Empty<string>();
    }
}