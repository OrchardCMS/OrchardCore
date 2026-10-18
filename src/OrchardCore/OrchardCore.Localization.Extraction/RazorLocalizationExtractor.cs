using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.NET.Sdk.Razor.SourceGenerators;

namespace OrchardCore.Localization.Extraction;

public sealed class RazorLocalizationExtractor
{
    public static Compilation Extract(Compilation compilation, ExtractionOptions options, LocalizationCatalog catalog, CancellationToken cancellationToken = default)
    {
        if (options.RazorFiles.Count == 0)
        {
            return compilation;
        }

        var files = options.RazorFiles.Select(file => new RazorText(file)).ToImmutableArray<AdditionalText>();
        if (!LanguageVersionFacts.TryParse(options.LanguageVersion, out var languageVersion))
        {
            throw new ArgumentException("Unsupported C# language version: " + options.LanguageVersion, nameof(options));
        }
        var parseOptions = new CSharpParseOptions(languageVersion, preprocessorSymbols: options.Defines);
        if (options.InterceptorsNamespaces.Length > 0)
        {
            parseOptions = parseOptions.WithFeatures([new KeyValuePair<string, string>("InterceptorsNamespaces", options.InterceptorsNamespaces)]);
        }
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new RazorSourceGenerator().AsSourceGenerator()],
            files,
            parseOptions,
            new RazorOptionsProvider(options));
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out var diagnostics, cancellationToken);
        foreach (var diagnostic in diagnostics)
        {
            var span = diagnostic.Location.GetMappedLineSpan();
            catalog.Diagnostics.Add(new ExtractionDiagnostic("OCLOC006", diagnostic.Id + ": " + diagnostic.GetMessage(), options.GetSource(span.Path, span.StartLinePosition.Line + 1), diagnostic.Severity == DiagnosticSeverity.Error));
        }

        var extractor = new CSharpLocalizationExtractor(updated, options, catalog);
        foreach (var result in driver.GetRunResult().Results)
        {
            if (result.Exception is not null)
            {
                catalog.Diagnostics.Add(new ExtractionDiagnostic("OCLOC006", "Razor generation failed: " + result.Exception.Message, new SourceReference(""), true));
                continue;
            }

            foreach (var generated in result.GeneratedSources)
            {
                var root = generated.SyntaxTree.GetRoot(cancellationToken);
                var identifier = root.DescendantNodes().OfType<AttributeSyntax>()
                    .Where(attribute => attribute.Name.ToString().EndsWith("RazorCompiledItemAttribute", StringComparison.Ordinal))
                    .Select(attribute => attribute.ArgumentList?.Arguments.LastOrDefault()?.Expression)
                    .OfType<LiteralExpressionSyntax>()
                    .Select(literal => literal.Token.ValueText)
                    .FirstOrDefault();
                if (identifier is not null)
                {
                    extractor.Extract(generated.SyntaxTree, options.GetViewContext(identifier), cancellationToken);
                }
            }
        }

        return updated;
    }

    private sealed class RazorText : AdditionalText
    {
        public RazorText(ExtractionFile file)
        {
            File = file;
        }

        public ExtractionFile File { get; }
        public override string Path => File.Path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SourceText.From(System.IO.File.ReadAllText(Path), Encoding.UTF8);
        }
    }

    private sealed class RazorOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions _global;

        public RazorOptionsProvider(ExtractionOptions options)
        {
            _global = new DictionaryOptions(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["build_property.RazorConfiguration"] = options.RazorConfiguration,
                ["build_property.RazorLangVersion"] = options.RazorLanguageVersion,
                ["build_property.RootNamespace"] = options.RootNamespace,
                ["build_property.MSBuildProjectDirectory"] = options.ProjectDirectory,
                ["build_property.GenerateRazorMetadataSourceChecksumAttributes"] = "false",
                ["build_property.SupportLocalizedComponentNames"] = "false",
            });
        }

        public override AnalyzerConfigOptions GlobalOptions => _global;
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => DictionaryOptions.Empty;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            var file = (RazorText)textFile;
            return new DictionaryOptions(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["build_metadata.AdditionalFiles.TargetPath"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(file.File.LogicalPath.Replace('\\', '/').TrimStart('/'))),
            });
        }
    }

    private sealed class DictionaryOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _values;

        public DictionaryOptions(Dictionary<string, string> values)
        {
            _values = values;
        }

        public static DictionaryOptions Empty { get; } = new([]);

        public override bool TryGetValue(string key, out string value) => _values.TryGetValue(key, out value!);
    }
}
