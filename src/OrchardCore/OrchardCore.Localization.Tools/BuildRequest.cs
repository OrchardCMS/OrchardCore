using System.Text;
using OrchardCore.Localization.Extraction;

namespace OrchardCore.Localization.Tools;

public sealed class BuildRequest
{
    public required ExtractionOptions Options { get; init; }

    /// <summary>
    /// Gets the path of the template to write, next to the project it describes.
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// Gets the path of the cache entry that lets an unchanged extraction be skipped.
    /// </summary>
    public required string StampPath { get; init; }

    /// <summary>
    /// Gets whether the source references of the template carry line numbers.
    /// </summary>
    public bool LineNumbers { get; init; }

    /// <summary>
    /// Reads a single property of a request without materializing it, so the process can be bound to the SDK's
    /// compiler before any compiler type is loaded.
    /// </summary>
    public static string ReadProperty(string path, string name)
    {
        var prefix = "Property|" + name + "|";
        foreach (var line in File.ReadLines(path, Encoding.UTF8))
        {
            if (line.StartsWith(prefix, StringComparison.Ordinal))
            {
                return Decode(line.Substring(prefix.Length));
            }
        }

        return "";
    }

    public static BuildRequest Read(string path)
    {
        var properties = new Dictionary<string, string>(StringComparer.Ordinal);
        var records = new List<string[]>();
        var lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length == 0 || lines[0] != "Version|1")
        {
            throw new InvalidDataException("Unsupported localization request version.");
        }

        foreach (var line in lines.Skip(1).Where(line => line.Length > 0))
        {
            var record = line.Split('|');
            if (record[0] == "Property" && record.Length == 3)
            {
                properties.Add(record[1], Decode(record[2]));
            }
            else
            {
                records.Add(record);
            }
        }

        var projectDirectory = Path.GetFullPath(properties["ProjectDirectory"]);
        var options = new ExtractionOptions
        {
            ProjectDirectory = projectDirectory,
            AssemblyName = properties["AssemblyName"],
            RootNamespace = properties.GetValueOrDefault("RootNamespace", ""),
            ViewPrefix = properties.GetValueOrDefault("ViewPrefix", ""),
            LanguageVersion = properties.GetValueOrDefault("LanguageVersion", "14.0"),
            InterceptorsNamespaces = properties.GetValueOrDefault("InterceptorsNamespaces", ""),
            RoslynDirectory = properties.GetValueOrDefault("RoslynDirectory", ""),
            RazorCompilerPath = properties.GetValueOrDefault("RazorCompilerPath", ""),
            RazorLanguageVersion = properties.GetValueOrDefault("RazorLanguageVersion", "10.0"),
            RazorConfiguration = properties.GetValueOrDefault("RazorConfiguration", "MVC-3.0"),
            Nullable = properties.GetValueOrDefault("Nullable", "disable"),
            OutputType = properties.GetValueOrDefault("OutputType", "Library"),
            AllowUnsafe = string.Equals(properties.GetValueOrDefault("AllowUnsafe"), "true", StringComparison.OrdinalIgnoreCase),
            Strict = string.Equals(properties.GetValueOrDefault("Strict"), "true", StringComparison.OrdinalIgnoreCase),
        };
        foreach (var define in properties.GetValueOrDefault("Defines", "").Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            options.Defines.Add(define);
        }

        foreach (var record in records)
        {
            if (record.Length != 3)
            {
                throw new InvalidDataException("Invalid localization request record.");
            }

            var physical = Path.GetFullPath(Decode(record[1]), projectDirectory);
            var logical = Decode(record[2]);
            var file = new ExtractionFile(physical, logical);
            switch (record[0])
            {
                case "Source":
                    options.Sources.Add(file);
                    break;
                case "Razor":
                    options.RazorFiles.Add(file);
                    break;
                case "Liquid":
                    options.LiquidFiles.Add(file);
                    break;
                case "Reference":
                    options.References.Add(physical);
                    options.ReferenceAliases[physical] = logical;
                    break;
                case "Analyzer":
                    options.Analyzers.Add(physical);
                    break;
                default:
                    throw new InvalidDataException("Unknown localization request record: " + record[0]);
            }
        }

        var outputPath = Path.GetFullPath(properties["OutputPath"], projectDirectory);

        return new BuildRequest
        {
            Options = options,
            OutputPath = outputPath,
            StampPath = properties.TryGetValue("StampPath", out var stampPath) && stampPath.Length > 0 ? Path.GetFullPath(stampPath, projectDirectory) : outputPath + ".stamp.json",
            LineNumbers = string.Equals(properties.GetValueOrDefault("LineNumbers"), "true", StringComparison.OrdinalIgnoreCase),
        };
    }

    private static string Decode(string value) => new UTF8Encoding(false, true).GetString(Convert.FromBase64String(value));
}
