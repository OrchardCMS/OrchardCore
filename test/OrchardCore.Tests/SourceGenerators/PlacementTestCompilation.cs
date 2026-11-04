using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Tests.SourceGenerators;

#nullable enable

/// <summary>
/// Compiles test sources against the placement types of Orchard Core.
/// </summary>
internal static class PlacementTestCompilation
{
    public static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

    private static readonly MetadataReference[] s_metadataReferences = GetMetadataReferences();

    public const string Preamble = """
        using OrchardCore.DisplayManagement.Descriptors;
        using OrchardCore.DisplayManagement.Views;
        using OrchardCore.Localization;
        namespace Example;
        """;

    public static CSharpCompilation Create(string source)
    {
        var compilation = CSharpCompilation.Create(
            "Tests",
            [CSharpSyntaxTree.ParseText(Preamble + source, ParseOptions)],
            s_metadataReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.Empty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        return compilation;
    }

    private static MetadataReference[] GetMetadataReferences()
    {
        var trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        Assert.False(string.IsNullOrEmpty(trustedPlatformAssemblies));

        // The placement types and the Orchard Core assemblies their signatures use.
        return trustedPlatformAssemblies!
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Concat(Directory.GetFiles(Path.GetDirectoryName(typeof(ShapeResult).Assembly.Location)!, "OrchardCore*.dll"))
            .DistinctBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    }
}
