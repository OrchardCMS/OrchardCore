using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace OrchardCore.Tests.Shell.Configuration;

/// <summary>
/// Validates the 'ConfigurationSchema.json' files describing the configuration sections of the projects,
/// which provide IntelliSense and validation for the 'appsettings.json' files.
/// </summary>
public partial class ConfigurationSchemaTests
{
    private const string SchemaFileName = "ConfigurationSchema.json";

    [Fact]
    public void ConfigurationSchemas_AreOpenSchemasOfTheOrchardCoreSection()
    {
        foreach (var (path, schema) in GetConfigurationSchemas())
        {
            Assert.True(schema["properties"]?["OrchardCore"]?["properties"] is JsonObject, $"'{path}' must describe the 'OrchardCore' section.");
            Assert.Null(schema["$schema"]);

            foreach (var node in Descendants(schema))
            {
                // The schemas are merged, so they must not reject the properties described by other schemas.
                Assert.False(node["additionalProperties"] is JsonValue value && !value.GetValue<bool>(), $"'{path}' must not use 'additionalProperties: false'.");
                Assert.Null(node["$ref"]);
            }
        }
    }

    [Fact]
    public void ConfigurationSchemas_DeprecatedSectionsReferenceDescribedSections()
    {
        var sections = MergeOrchardCoreSections();
        var deprecatedSections = sections.Where(section => section.Value["deprecated"]?.GetValue<bool>() == true).ToArray();

        Assert.NotEmpty(deprecatedSections);

        foreach (var (name, schema) in deprecatedSections)
        {
            var message = schema["deprecationMessage"]?.GetValue<string>();
            Assert.False(string.IsNullOrEmpty(message), $"The deprecated '{name}' section must provide a deprecation message.");

            var match = CurrentSectionRegex().Match(message);
            Assert.True(match.Success, $"The deprecation message of the '{name}' section must reference the section to use instead.");

            JsonNode node = new JsonObject { ["properties"] = sections.DeepClone() };
            foreach (var segment in match.Groups[1].Value.Split(':'))
            {
                node = node["properties"]?[segment];
                Assert.True(node is not null, $"The '{match.Groups[1].Value}' section referenced by the deprecated '{name}' section must be described.");
            }
        }
    }

    [Fact]
    public void ConfigurationSchemas_SharedSectionsHaveSameValues()
    {
        // Visual Studio merges the schemas and fails when they define different values for the same keyword,
        // e.g. different descriptions for a parent section like 'Email' shared by several schemas.
        var values = new Dictionary<string, (string Value, string Path)>();

        foreach (var (path, schema) in GetConfigurationSchemas())
        {
            foreach (var (keyword, value) in Values(schema, string.Empty))
            {
                if (values.TryGetValue(keyword, out var existing))
                {
                    Assert.True(existing.Value == value, $"'{keyword}' is '{existing.Value}' in '{existing.Path}' but '{value}' in '{path}'.");
                }
                else
                {
                    values[keyword] = (value, path);
                }
            }
        }
    }

    private static JsonObject MergeOrchardCoreSections()
    {
        var sections = new JsonObject();

        foreach (var (_, schema) in GetConfigurationSchemas())
        {
            foreach (var (name, section) in schema["properties"]["OrchardCore"]["properties"].AsObject())
            {
                Merge(sections, name, section);
            }
        }

        return sections;
    }

    private static void Merge(JsonObject target, string name, JsonNode source)
    {
        if (target[name] is not JsonObject existing)
        {
            target[name] = source.DeepClone();
            return;
        }

        if (source["properties"] is not JsonObject properties)
        {
            return;
        }

        if (existing["properties"] is not JsonObject existingProperties)
        {
            existingProperties = [];
            existing["properties"] = existingProperties;
        }

        foreach (var (propertyName, property) in properties)
        {
            Merge(existingProperties, propertyName, property);
        }
    }

    private static IEnumerable<(string Path, string Value)> Values(JsonNode node, string path)
    {
        if (node is JsonObject obj)
        {
            foreach (var (name, child) in obj)
            {
                foreach (var value in Values(child, $"{path}/{name}"))
                {
                    yield return value;
                }
            }
        }
        else if (node is not null)
        {
            yield return (path, node.ToJsonString());
        }
    }

    private static IEnumerable<JsonObject> Descendants(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            yield return obj;

            foreach (var (_, child) in obj)
            {
                foreach (var descendant in Descendants(child))
                {
                    yield return descendant;
                }
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array)
            {
                foreach (var descendant in Descendants(child))
                {
                    yield return descendant;
                }
            }
        }
    }

    private static (string Path, JsonNode Schema)[] GetConfigurationSchemas()
    {
        var root = GetRepositoryRoot();

        var schemas = new[] { "src/OrchardCore", "src/OrchardCore.Modules" }
            .SelectMany(folder => Directory.EnumerateDirectories(Path.Combine(root, folder)))
            .Select(project => Path.Combine(project, SchemaFileName))
            .Where(File.Exists)
            .Select(path => (path, JsonNode.Parse(File.ReadAllText(path))))
            .ToArray();

        Assert.NotEmpty(schemas);

        return schemas;
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OrchardCore.slnx")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            Assert.Skip("The repository sources are not available.");
        }

        return directory.FullName;
    }

    [GeneratedRegex("'OrchardCore:([^']+)'")]
    private static partial Regex CurrentSectionRegex();
}
