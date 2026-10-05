using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OrchardCore.Localization.Extraction;

namespace OrchardCore.Localization.Extraction.Tests;

internal sealed class TestWorkspace : IDisposable
{
    public TestWorkspace()
    {
        DirectoryPath = Path.Combine(RepositoryRoot, ".vs", "localization-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DirectoryPath);
    }

    public static string RepositoryRoot { get; } = FindRepositoryRoot();
    public string DirectoryPath { get; }

    public string Write(string relativePath, string content)
    {
        var path = Path.Combine(DirectoryPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    public ExtractionOptions Options(string viewPrefix = "")
    {
        var options = new ExtractionOptions
        {
            ProjectDirectory = DirectoryPath,
            AssemblyName = "Tests",
            RootNamespace = "Tests",
            ViewPrefix = viewPrefix,
        };
        foreach (var path in ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator))
        {
            if (!Path.GetFileName(path).StartsWith("OrchardCore.", StringComparison.Ordinal))
            {
                options.References.Add(path);
            }
        }

        return options;
    }

    public static LocalizationCatalog ExtractCSharp(string source, bool includePlurals = false, string? viewContext = null, bool includeSources = false)
    {
        using var workspace = new TestWorkspace();
        var options = workspace.Options();
        var tree = CSharpSyntaxTree.ParseText(source, path: Path.Combine(workspace.DirectoryPath, "Messages.cs"));
        var trees = new List<SyntaxTree> { tree, CSharpSyntaxTree.ParseText("global using System;") };
        var attributePath = Path.Combine(RepositoryRoot, "src", "OrchardCore", "OrchardCore.Abstractions", "Localization", "SkipLocalizationExtractionAttribute.cs");
        trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(attributePath), path: attributePath));
        if (includeSources)
        {
            var directory = Path.Combine(RepositoryRoot, "src", "OrchardCore", "OrchardCore.Abstractions", "Localization");
            foreach (var path in new[] { "LocalizationSource.cs", "Extensions/StringLocalizerFactoryExtensions.cs", "Extensions/HtmlLocalizerFactoryExtensions.cs" })
            {
                var fullPath = Path.Combine(directory, path);
                trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(fullPath), path: fullPath));
            }
        }
        if (includePlurals)
        {
            var directory = Path.Combine(RepositoryRoot, "src", "OrchardCore", "OrchardCore.Localization.Abstractions");
            foreach (var path in new[] { "Extensions/StringLocalizerExtensions.cs", "Extensions/HtmlLocalizerExtensions.cs", "Extensions/ViewLocalizerExtensions.cs", "PluralizationArgument.cs" })
            {
                var fullPath = Path.Combine(directory, path);
                trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(fullPath), path: fullPath));
            }
            if (includeSources)
            {
                foreach (var path in new[] { "Extensions/StringLocalizerFactoryPluralExtensions.cs", "Extensions/HtmlLocalizerFactoryPluralExtensions.cs", "Extensions/LocalizationSourcePluralHelper.cs" })
                {
                    var fullPath = Path.Combine(directory, path);
                    trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(fullPath), path: fullPath));
                }
            }
        }

        var compilation = CSharpCompilation.Create("Tests", trees, options.References.Select(path => MetadataReference.CreateFromFile(path)), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Xunit.Assert.Empty(errors);
        var catalog = new LocalizationCatalog();
        var extractor = new CSharpLocalizationExtractor(compilation, options, catalog);
        foreach (var syntaxTree in includeSources ? trees : [tree])
        {
            extractor.Extract(syntaxTree, viewContext);
        }
        return catalog;
    }

    public void Dispose()
    {
        Directory.Delete(DirectoryPath, true);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "OrchardCore.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("The localization tests must run from the Orchard Core workspace.");
    }
}
