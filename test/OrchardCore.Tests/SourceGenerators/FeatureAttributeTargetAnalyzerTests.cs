using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using OrchardCore.DisplayManagement.SourceGenerators;

namespace OrchardCore.Tests.SourceGenerators;

#nullable enable

public class FeatureAttributeTargetAnalyzerTests
{
    private static readonly MetadataReference[] s_metadataReferences = GetMetadataReferences();

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenTargetingLocalExplicitFeature()
    {
        const string source = """
            using ModuleFeature = OrchardCore.Modules.FeatureAttribute;
            using OrchardCore.Modules.Manifest;

            [assembly: Module(Name = "Test Module")]
            [assembly: OrchardCore.Modules.Manifest.Feature(Id = "TestModule.FeatureA", Name = "Feature A")]

            [ModuleFeature("TestModule.FeatureA")]
            public class SampleType
            {
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source, assemblyName: "TestModule");

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ReportsDiagnostic_WhenTargetingFeatureFromDifferentModule()
    {
        const string source = """
            using ModuleFeature = OrchardCore.Modules.FeatureAttribute;
            using OrchardCore.Modules.Manifest;

            [assembly: Module(Name = "Test Module")]
            [assembly: OrchardCore.Modules.Manifest.Feature(Id = "TestModule.FeatureA", Name = "Feature A")]

            [ModuleFeature("OtherModule.FeatureA")]
            public class SampleType
            {
            }
            """;

        var diagnostic = Assert.Single(await GetDiagnosticsAsync(source, assemblyName: "TestModule"));

        Assert.Equal(FeatureAttributeTargetAnalyzer.DiagnosticId, diagnostic.Id);
        Assert.Contains("OtherModule.FeatureA", diagnostic.GetMessage(), StringComparison.Ordinal);
        Assert.Contains("TestModule.FeatureA", diagnostic.GetMessage(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenTargetingImplicitModuleFeature()
    {
        const string source = """
            using ModuleFeature = OrchardCore.Modules.FeatureAttribute;
            using OrchardCore.Modules.Manifest;

            [assembly: Module(Name = "Test Module")]

            [ModuleFeature("TestModule")]
            public class SampleType
            {
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source, assemblyName: "TestModule");

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ReportsDiagnostic_WhenTargetingFeatureOutsideImplicitModuleFeature()
    {
        const string source = """
            using ModuleFeature = OrchardCore.Modules.FeatureAttribute;
            using OrchardCore.Modules.Manifest;

            [assembly: Module(Id = "ModuleAlias", Name = "Test Module")]

            [ModuleFeature("OtherModule.FeatureA")]
            public class SampleType
            {
            }
            """;

        var diagnostic = Assert.Single(await GetDiagnosticsAsync(source, assemblyName: "TestModule"));

        Assert.Equal(FeatureAttributeTargetAnalyzer.DiagnosticId, diagnostic.Id);
        Assert.Contains("ModuleAlias", diagnostic.GetMessage(), StringComparison.Ordinal);
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source, string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            s_metadataReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var compilationErrors = compilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.Empty(compilationErrors);

        var analyzer = new FeatureAttributeTargetAnalyzer();
        var diagnostics = await compilation
            .WithAnalyzers([analyzer])
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

        references.Add(MetadataReference.CreateFromFile(typeof(global::OrchardCore.Modules.FeatureAttribute).Assembly.Location));

        return references.ToArray();
    }
}
