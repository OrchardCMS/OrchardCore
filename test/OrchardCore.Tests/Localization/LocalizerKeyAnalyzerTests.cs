using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using OrchardCore.Localization;
using OrchardCore.Localization.Analyzers;

namespace OrchardCore.Tests.Localization;

#nullable enable

public class LocalizerKeyAnalyzerTests
{
    private static readonly MetadataReference[] s_metadataReferences = GetMetadataReferences();

    private const string Preamble = """
        using Microsoft.AspNetCore.Mvc.Localization;
        using Microsoft.Extensions.Localization;
        using OrchardCore.Localization;
        namespace Example;
        public class Messages
        {
            public const string Constant = "Constant message";
            private readonly IStringLocalizer<Messages> S = null!;
            private readonly IHtmlLocalizer<Messages> H = null!;
            private readonly IViewLocalizer T = null!;
            private readonly IStringLocalizerFactory _factory = null!;
            private static readonly LocalizationSource s_item = LocalizationSource.Create<Messages>("{0} item");
        """;

    [Theory]
    [InlineData("S[key]")]
    [InlineData("S[key, count]")]
    [InlineData("S[\"Prefix \" + key]")]
    [InlineData("S[$\"Count {count}\"]")]
    [InlineData("H[key]")]
    [InlineData("T[key]")]
    [InlineData("S.GetString(key)")]
    [InlineData("S.GetString(key, count)")]
    [InlineData("H.GetHtml(key)")]
    [InlineData("T.GetHtml(key, count)")]
    [InlineData("S.Plural(count, key, \"{0} items\")")]
    [InlineData("S.Plural(count, \"{0} item\", key)")]
    [InlineData("H.Plural(count, forms)")]
    [InlineData("T.Plural(count, forms, count)")]
    [InlineData("_factory.Plural(count, s_item, key)")]
    public async Task Analyze_NonConstantKey_ReportsNonConstantKey(string expression)
    {
        var source = Preamble + $$"""
                public object Get(string key, string[] forms, int count) => {{expression}};
            }
            """;

        var diagnostic = Assert.Single(await GetDiagnosticsAsync(source));

        Assert.Equal(LocalizerKeyAnalyzer.NonConstantKeyDiagnosticId, diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        var start = source.IndexOf(expression, StringComparison.Ordinal);
        Assert.InRange(diagnostic.Location.SourceSpan.Start, start, start + expression.Length);
    }

    [Theory]
    [InlineData("S[\"Message\"]")]
    [InlineData("S[\"Message {0}\", count]")]
    [InlineData("S[Constant]")]
    [InlineData("S[\"Message \" + nameof(Messages)]")]
    [InlineData("H[\"<strong>Message</strong>\"]")]
    [InlineData("T[\"Message\"]")]
    [InlineData("S.GetString(\"Message\")")]
    [InlineData("H.GetHtml(\"Message\", count)")]
    [InlineData("S.Plural(count, \"{0} item\", \"{0} items\")")]
    [InlineData("S.Plural(count, [\"{0} item\", \"{0} items\"])")]
    [InlineData("H.Plural(count, new[] { \"{0} item\", \"{0} items\" }, count)")]
    [InlineData("_factory.Plural(count, s_item, \"{0} items\")")]
    [InlineData("new System.Collections.Generic.Dictionary<string, string>()[key]")]
    public async Task Analyze_ConstantKeys_ReportsNothing(string expression)
    {
        var source = Preamble + $$"""
                public object Get(string key, int count) => {{expression}};
            }
            """;

        Assert.Empty(await GetDiagnosticsAsync(source));
    }

    [Fact]
    public async Task Analyze_SkippedDeclarations_ReportsNothing()
    {
        const string source = """
            using Microsoft.Extensions.Localization;
            using OrchardCore.Localization;
            namespace Example;
            [SkipLocalizationExtraction]
            public class Forwarder
            {
                public LocalizedString Get(IStringLocalizer localizer, string key) => localizer[key];
            }
            public class Messages
            {
                [SkipLocalizationExtraction]
                public LocalizedString Forward(IStringLocalizer localizer, string key) => localizer.GetString(key);
            }
            """;

        Assert.Empty(await GetDiagnosticsAsync(source));
    }

    [Fact]
    public async Task Analyze_LocalizerImplementation_ReportsForwardedKeys()
    {
        const string source = """
            using System.Collections.Generic;
            using Microsoft.Extensions.Localization;
            namespace Example;
            public class Wrapper : IStringLocalizer
            {
                private readonly IStringLocalizer _inner = null!;
                public LocalizedString this[string name] => _inner[name];
                public LocalizedString this[string name, params object[] arguments] => _inner[name, arguments];
                public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => _inner.GetAllStrings(includeParentCultures);
            }
            """;

        Assert.Equal(2, (await GetDiagnosticsAsync(source)).Length);
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var compilation = CSharpCompilation.Create(
            "Tests",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))],
            s_metadataReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.Empty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        var diagnostics = await compilation
            .WithAnalyzers([new LocalizerKeyAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();

        return diagnostics.OrderBy(diagnostic => diagnostic.Location.SourceSpan.Start).ToImmutableArray();
    }

    private static MetadataReference[] GetMetadataReferences()
    {
        var trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        Assert.False(string.IsNullOrEmpty(trustedPlatformAssemblies));

        var references = trustedPlatformAssemblies!
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToList();

        references.Add(MetadataReference.CreateFromFile(typeof(LocalizationSource).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(PluralizationArgument).Assembly.Location));

        return references.ToArray();
    }
}
