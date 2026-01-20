using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell.Configuration.Internal;

public static class ConfigurationExtensions
{
    public static JsonObject ToJsonObject(this IConfiguration configuration)
    {
        var jsonNode = ToJsonNode(configuration);
        if (jsonNode is not JsonObject jObject)
        {
            throw new FormatException($"Top level JSON element must be an object. Instead, {jsonNode.GetValueKind()} was found.");
        }

        return jObject;
    }

    public static JsonNode ToJsonNode(this IConfiguration configuration)
    {
        JsonArray jArray = null;
        JsonObject jObject = null;

        foreach (var child in configuration.GetChildren())
        {
            if (int.TryParse(child.Key, out var index))
            {
                if (jObject is not null)
                {
                    throw new FormatException($"Can't use the numeric key '{child.Key}' inside an object.");
                }

                jArray ??= [];
                if (index > jArray.Count)
                {
                    // Inserting null values is useful to override arrays items,
                    // it allows to keep non null items at the right position.
                    for (var i = jArray.Count; i < index; i++)
                    {
                        jArray.Add(null);
                    }
                }

                if (child.GetChildren().Any())
                {
                    jArray.Add(ToJsonNode(child));
                }
                else
                {
                    jArray.Add(child.Value);
                }
            }
            else
            {
                if (jArray is not null)
                {
                    throw new FormatException($"Can't use the non numeric key '{child.Key}' inside an array.");
                }

                jObject ??= [];
                if (child.GetChildren().Any())
                {
                    jObject.Add(child.Key, ToJsonNode(child));
                }
                else
                {
                    jObject.Add(child.Key, child.Value);
                }
            }
        }

        return jArray as JsonNode ?? jObject ?? [];
    }

    public static JsonObject ToJsonObject(this IDictionary<string, string> configurationData)
    {
        var configuration = new ConfigurationBuilder()
            .Add(new UpdatableDataProvider(configurationData))
            .Build();

        using var disposable = configuration as IDisposable;

        return configuration.ToJsonObject();
    }

    public static Task<IDictionary<string, string>> ToConfigurationDataAsync(this JsonObject jConfiguration)
    {
        if (jConfiguration is null)
        {
            return Task.FromResult<IDictionary<string, string>>(new Dictionary<string, string>());
        }

        var configurationString = jConfiguration.ToJsonString(JOptions.Default);
        return Task.FromResult(JsonConfigurationParser.Parse(configurationString));
    }
}
