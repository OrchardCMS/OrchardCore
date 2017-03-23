using Microsoft.Extensions.Configuration;
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
            
            return shellSettings;
        }
    }
}
