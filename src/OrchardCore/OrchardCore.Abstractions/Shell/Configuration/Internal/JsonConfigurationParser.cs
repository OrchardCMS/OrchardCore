using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

#nullable enable

namespace OrchardCore.Environment.Shell.Configuration.Internal;

public sealed class JsonConfigurationParser
{
    private JsonConfigurationParser() { }

    private readonly Dictionary<string, string?> _data = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string> _paths = new();

    public static IDictionary<string, string?> Parse(Stream utf8Json)
        => new JsonConfigurationParser().ParseStream(utf8Json);

    public static IDictionary<string, string?> Parse(string document)
        => new JsonConfigurationParser().ParseDocument(document);

    public static Task<IDictionary<string, string?>> ParseAsync(Stream utf8Json)
        => new JsonConfigurationParser().ParseStreamAsync(utf8Json);

    private Dictionary<string, string?> ParseStream(Stream utf8Json)
    {
        try
        {
            using (var doc = JsonDocument.Parse(utf8Json, JOptions.Document))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException($"Top-level JSON element must be an object. Instead, '{doc.RootElement.ValueKind}' was found.");
                }

                VisitObjectElement(doc.RootElement);
            }

            return _data;
        }
        catch (JsonException e)
        {
            throw new FormatException("Could not parse the JSON document.", e);
        }
    }

    private Dictionary<string, string?> ParseDocument(string document)
    {
        try
        {
            using (var doc = JsonDocument.Parse(document, JOptions.Document))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException($"Top-level JSON element must be an object. Instead, '{doc.RootElement.ValueKind}' was found.");
                }

                VisitObjectElement(doc.RootElement);
            }

            return _data;
        }
        catch (JsonException e)
        {
            throw new FormatException("Could not parse the JSON document.", e);
        }
    }

    private async Task<IDictionary<string, string?>> ParseStreamAsync(Stream input)
    {
        try
        {
            // Use JOptions.Document to allow comments and trailing commas
            using (var doc = await JsonDocument.ParseAsync(input, JOptions.Document))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException($"Top-level JSON element must be an object. Instead, '{doc.RootElement.ValueKind}' was found.");
                }

                VisitObjectElement(doc.RootElement);
            }

            return _data;
        }
        catch (JsonException e)
        {
            throw new FormatException("Could not parse the JSON document.", e);
        }
    }

    private void VisitObjectElement(JsonElement element)
    {
        var isEmpty = true;

        foreach (var property in element.EnumerateObject())
        {
            isEmpty = false;
            EnterContext(property.Name);
            VisitValue(property.Value);
            ExitContext();
        }

        SetNullIfElementIsEmpty(isEmpty);
    }

    private void VisitArrayElement(JsonElement element)
    {
        var index = 0;

        foreach (var arrayElement in element.EnumerateArray())
        {
            EnterContext(index.ToString());
            VisitValue(arrayElement, visitArray: true);
            ExitContext();
            index++;
        }

        SetNullIfElementIsEmpty(isEmpty: index == 0);
    }

    private void SetNullIfElementIsEmpty(bool isEmpty)
    {
        if (isEmpty && _paths.Count > 0)
        {
            _data[_paths.Peek()] = null;
        }
    }

    private void VisitValue(JsonElement value, bool visitArray = false)
    {
        Debug.Assert(_paths.Count > 0);

        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                VisitObjectElement(value);
                break;

            case JsonValueKind.Array:
                VisitArrayElement(value);
                break;

            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:

                // Skipping null values is useful to override array items,
                // it allows to keep non null items at the right position.
                if (visitArray && value.ValueKind == JsonValueKind.Null)
                {
                    break;
                }

                var key = _paths.Peek();
                if (_data.ContainsKey(key))
                {
                    throw new FormatException($"A duplicate key '{key}' was found.");
                }
                _data[key] = value.ToString();
                break;

            default:
                throw new FormatException($"Unsupported JSON token '{value.ValueKind}' was found.");
        }
    }

    private void EnterContext(string context) =>
        _paths.Push(_paths.Count > 0 ?
            _paths.Peek() + ConfigurationPath.KeyDelimiter + context :
            context);

    private void ExitContext() => _paths.Pop();
}
