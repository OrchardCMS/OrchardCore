using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell.Configuration.Internal;

public static class ConfigurationDataExtensions
{
    public static JObject ToJObject(this IDictionary<string, string> configurationData)
    {
        var config = new ConfigurationBuilder()
            .Add(new UpdatableDataProvider(configurationData))
            .Build();

        using var disposable = config as IDisposable;

        return config.ToJObject();
    }

    public static async Task<IDictionary<string, string>> ToConfigurationDataAsync(this JObject jConfiguration)
    {
        var configString = await jConfiguration.ToStringAsync(Formatting.None);
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(configString));

        return await JsonConfigurationParser.ParseAsync(ms);
    }
}
