using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell.Configuration.Internal;

public static class ConfigurationJObjectExtensions
{
    public static async Task<string> ToStringAsync(this JObject jConfiguration, Formatting formatting = Formatting.Indented)
    {
        using var sw = new StringWriter(CultureInfo.InvariantCulture);
        using var jw = new JsonTextWriter(sw) { Formatting = Formatting.None };

        await jConfiguration.WriteToAsync(jw);

        return sw.ToString();
    }
}
