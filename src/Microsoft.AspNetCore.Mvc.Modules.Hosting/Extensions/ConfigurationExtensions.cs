using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Hosting.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IEnumerable<string> GetConfigurationArray(this IConfiguration configuration, string key)
        {
            if (configuration != null)
            {
                int index = 0;
                string extraModuleFolder = configuration[key + ":" + index];
                while (extraModuleFolder != null)
                {
                    yield return extraModuleFolder;
                    index++;
                    extraModuleFolder = configuration[key + ":" + index];
                }
            }
        }

    }
}
