using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell.Configuration.Internal;

public static class JsonConfigurationExtensions
{
    public static JObject ToJObject(this IDictionary<string, string> configData)
    {
        var configuration = new ConfigurationBuilder()
            .Add(new UpdatableDataProvider(configData))
            .Build();

        using var disposable = configuration as IDisposable;

        return configuration.ToJObject();
    }

    public static JObject ToJObject(this IConfiguration configuration)
    {
        var jToken = ToJToken(configuration);
        if (jToken is not JObject jObject)
        {
            throw new FormatException($"Top level JSON element must be an object. Instead, {jToken.Type} was found.");
        }

        return jObject;
    }

    public static JToken ToJToken(this IConfiguration configuration)
    {
        JArray jArray = null;
        JObject jObject = null;

        foreach (var child in configuration.GetChildren())
        {
            if (int.TryParse(child.Key, out var index))
            {
                if (jObject is not null)
                {
                    throw new FormatException($"Can't use the numeric key '{child.Key}' inside an object.");
                }

                jArray ??= new JArray();
                if (index > jArray.Count)
                {
                    // OC: Inserting null values is useful to override arrays,
                    // it allows to keep non null items at the right position.
                    for (var i = jArray.Count; i < index; i++)
                    {
                        jArray.Add(JValue.CreateNull());
                    }
                }

                if (child.GetChildren().Any())
                {
                    jArray.Add(ToJToken(child));
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

                jObject ??= new JObject();
                if (child.GetChildren().Any())
                {
                    jObject.Add(child.Key, ToJToken(child));
                }
                else
                {
                    jObject.Add(child.Key, child.Value);
                }
            }
        }

        return jArray as JToken ?? jObject ?? new JObject();
    }
}
