using System.Text;
using OrchardCore.Localization.Extraction;

namespace OrchardCore.Localization.Tools;

public sealed class BuildRequest
{
    public required ExtractionOptions Options { get; init; }
    public required string OutputPath { get; init; }

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

        return new BuildRequest
        {
            Options = options,
            OutputPath = Path.GetFullPath(properties["OutputPath"], projectDirectory),
        };
    }

    private static string Decode(string value) => new UTF8Encoding(false, true).GetString(Convert.FromBase64String(value));
}
