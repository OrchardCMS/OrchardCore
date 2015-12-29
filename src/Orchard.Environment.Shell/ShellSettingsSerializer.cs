using Microsoft.Extensions.Configuration;
using Orchard.Environment.Shell.Models;
using System;

namespace Orchard.Environment.Shell
{
    public static class ShellSettingsSerializer
    {
        public static ShellSettings ParseSettings(IConfigurationRoot configuration)
        {
            var shellSettings = new ShellSettings();
            shellSettings.Name = configuration["Name"];

            TenantState state;
            shellSettings.State = Enum.TryParse(configuration["State"], true, out state) ? state : TenantState.Uninitialized;

            shellSettings.RequestUrlHost = configuration["RequestUrlHost"];
            shellSettings.RequestUrlPrefix = configuration["RequestUrlPrefix"];
            shellSettings.ConnectionString = configuration["ConnectionString"];
            shellSettings.TablePrefix = configuration["TablePrefix"];
            shellSettings.DatabaseProvider = configuration["DatabaseProvider"];
            
            return shellSettings;
        }
    }
}
