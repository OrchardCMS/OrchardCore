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
        var configuration = new ConfigurationBuilder()
            .Add(new UpdatableDataProvider(configurationData))
            .Build();

        using var disposable = configuration as IDisposable;

        return configuration.ToJObject();
    }

    public static async Task<IDictionary<string, string>> ToConfigurationDataAsync(this JObject jConfiguration)
    {
        var configurationString = await jConfiguration.ToStringAsync(Formatting.None);
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(configurationString));

        return await JsonConfigurationParser.ParseAsync(ms);
    }
}
