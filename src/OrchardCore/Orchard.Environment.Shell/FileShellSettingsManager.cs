using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            var preShellSettings = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(tenantsFile.PhysicalPath));

            var shellSettings = new List<ShellSettingsWithTenants>();
            foreach (var preShellSetting in preShellSettings)
            {
                var shellSetting = JObject.FromObject(preShellSetting.Value).ToObject<ShellSettingsWithTenants>();

                shellSetting.Name = preShellSetting.Key;

                foreach (var vals in preShellSetting.Value)
                {
                    shellSetting[vals.Key] = vals.Value.ToString();
                }

                shellSettings.Add(shellSetting);
            }

            return shellSettings;
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