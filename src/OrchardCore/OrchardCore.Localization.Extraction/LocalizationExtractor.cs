using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace OrchardCore.Localization.Extraction;

public static class LocalizationExtractor
{
    public static LocalizationCatalog Extract(ExtractionOptions options, CancellationToken cancellationToken = default)
    {
        if (!LanguageVersionFacts.TryParse(options.LanguageVersion, out var languageVersion))
        {
            throw new ArgumentException("Unsupported C# language version: " + options.LanguageVersion, nameof(options));
        }

        var parseOptions = new CSharpParseOptions(languageVersion, preprocessorSymbols: options.Defines);
        if (options.InterceptorsNamespaces.Length > 0)
        {
            parseOptions = parseOptions.WithFeatures([new KeyValuePair<string, string>("InterceptorsNamespaces", options.InterceptorsNamespaces)]);
        }
        var trees = options.Sources.Select(file => CSharpSyntaxTree.ParseText(SourceText.From(File.ReadAllText(file.Path), Encoding.UTF8), parseOptions, file.Path, cancellationToken)).ToArray();
        var references = options.References.Distinct(StringComparer.Ordinal).Select(path =>
        {
            var properties = MetadataReferenceProperties.Assembly;
            if (options.ReferenceAliases.TryGetValue(path, out var aliases) && aliases.Length > 0)
            {
                properties = properties.WithAliases(aliases.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToImmutableArray());
            }

            return MetadataReference.CreateFromFile(path, properties);
        });
        var nullable = options.Nullable.ToLowerInvariant() switch
        {
            "enable" => NullableContextOptions.Enable,
            "annotations" => NullableContextOptions.Annotations,
            "warnings" => NullableContextOptions.Warnings,
            _ => NullableContextOptions.Disable,
        };
        var outputKind = options.OutputType.ToUpperInvariant() switch
        {
            "EXE" => OutputKind.ConsoleApplication,
            "WINEXE" => OutputKind.WindowsApplication,
            _ => OutputKind.DynamicallyLinkedLibrary,
        };
        var compilation = CSharpCompilation.Create(options.AssemblyName, trees, references, new CSharpCompilationOptions(outputKind, allowUnsafe: options.AllowUnsafe, nullableContextOptions: nullable));
        var catalog = new LocalizationCatalog();
        var updated = RazorLocalizationExtractor.Extract(compilation, options, catalog, cancellationToken);
        var generators = options.Analyzers.Distinct(StringComparer.Ordinal)
            .Where(path => !string.Equals(Path.GetFileName(path), "Microsoft.CodeAnalysis.Razor.Compiler.dll", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path =>
            {
                var reference = new AnalyzerFileReference(path, GeneratorAssemblyLoader.Instance);
                reference.AnalyzerLoadFailed += (_, failure) => catalog.Diagnostics.Add(new ExtractionDiagnostic("OCLOC008", "Extraction generator '" + Path.GetFileName(path) + "' could not be loaded: " + failure.Message, new SourceReference("")));
                return reference.GetGenerators(LanguageNames.CSharp);
            })
            .Where(generator => !generator.GetGeneratorType().FullName!.StartsWith("Microsoft.NET.Sdk.Razor.", StringComparison.Ordinal))
            .ToArray();
        if (generators.Length > 0)
        {
            CSharpGeneratorDriver.Create(generators, parseOptions: parseOptions)
                .RunGeneratorsAndUpdateCompilation(updated, out var generated, out var diagnostics, cancellationToken);
            updated = (CSharpCompilation)generated;
            foreach (var diagnostic in diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            {
                catalog.Diagnostics.Add(new ExtractionDiagnostic("OCLOC008", "Extraction generator: " + diagnostic.GetMessage(), new SourceReference(""), true));
            }
        }

        var extractor = new CSharpLocalizationExtractor(updated, options, catalog);
        var annotations = new DataAnnotationsLocalizationExtractor(updated, options, catalog);
        foreach (var tree in trees)
        {
            extractor.Extract(tree, cancellationToken: cancellationToken);
            annotations.Extract(tree, cancellationToken);
        }

        foreach (var diagnostic in updated.GetDiagnostics(cancellationToken).Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Take(100))
        {
            var span = diagnostic.Location.GetMappedLineSpan();
            catalog.Diagnostics.Add(new ExtractionDiagnostic("OCLOC008", "Extraction compilation: " + diagnostic.Id + ": " + diagnostic.GetMessage(), options.GetSource(span.Path, span.StartLinePosition.Line + 1), true));
        }

        var liquid = new LiquidLocalizationExtractor();
        foreach (var file in options.LiquidFiles)
        {
            liquid.Extract(File.ReadAllText(file.Path), file, options, catalog, cancellationToken);
        }

        return catalog;
    }
}
