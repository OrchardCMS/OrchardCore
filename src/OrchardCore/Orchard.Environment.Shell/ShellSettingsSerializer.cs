using Microsoft.Extensions.Configuration;
using Orchard.Environment.Shell.Models;
using System;
using System.Linq;

namespace Orchard.Environment.Shell
{
    public static class ShellSettingsSerializer
    {
        public static ShellSettings ParseSettings(IConfigurationRoot configuration)
        {
            var shellSettings = new ShellSettings();
            
            foreach (var setting in configuration.AsEnumerable().ToDictionary(k => k.Key, v => v.Value)) 
            {
                shellSettings[setting.Key] = setting.Value;
            }

            TenantState state;
            shellSettings.State = Enum.TryParse(configuration["State"], true, out state) ? state : TenantState.Uninitialized;

            return shellSettings;
        }
    }
}
