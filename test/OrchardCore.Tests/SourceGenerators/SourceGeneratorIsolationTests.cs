using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.SourceGenerators;

namespace OrchardCore.Tests.SourceGenerators;

public class SourceGeneratorIsolationTests
{
    private static readonly MetadataReference[] s_metadataReferences = GetMetadataReferences();
    private static readonly CSharpParseOptions s_parseOptions = new CSharpParseOptions(LanguageVersion.Latest)
        .WithFeatures([new KeyValuePair<string, string>("InterceptorsNamespaces", "OrchardCore.DisplayManagement.Generated")]);

    [Theory]
    [InlineData("factory.CreateAsync<Shared.Model>()")]
    [InlineData("new OrchardCore.DisplayManagement.Handlers.DisplayDriverBase().Initialize<Shared.Model>()")]
    public void Generate_SharedModelInReferencedAssembly_IsolatesPublicShapeTypes(string invocation)
    {
        const string modelSource = """
            namespace Shared
            {
                public class Model { }
            }
            """;
        var callSource = $$"""
            using OrchardCore.DisplayManagement;

            public static class Calls
            {
                public static void Create(IShapeFactory factory) => _ = {{invocation}};
            }
            """;

        var producer = Generate(new ShapeFactoryGenerator(), "Producer", callSource + System.Environment.NewLine + modelSource);
        var producerShape = Assert.Single(GetGeneratedShapeTypes(producer));
        using var assembly = new MemoryStream();
        var emitResult = producer.Emit(assembly, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(emitResult.Success, string.Join(System.Environment.NewLine, emitResult.Diagnostics));

        var consumer = Generate(new ShapeFactoryGenerator(), "Consumer", callSource.Replace("class Calls", "class ConsumerCalls", StringComparison.Ordinal),
            MetadataReference.CreateFromImage(assembly.ToArray()));
        var consumerShape = Assert.Single(GetGeneratedShapeTypes(consumer));

        Assert.NotEqual(producerShape.Name, consumerShape.Name);
        Assert.Equal(Accessibility.Public, producerShape.DeclaredAccessibility);
        Assert.Equal(Accessibility.Public, consumerShape.DeclaredAccessibility);
        Assert.Equal("Shared.Model", producerShape.BaseType.ToDisplayString());
        Assert.Equal("Shared.Model", consumerShape.BaseType.ToDisplayString());
        Assert.Equal("Producer", consumerShape.BaseType.ContainingAssembly.Name);
        AssertCompilesWithoutWarnings(consumer);
    }

    [Fact]
    public void Generate_SameModelNameInDifferentNamespaces_GeneratesDistinctShapeTypes()
    {
        const string source = """
            using OrchardCore.DisplayManagement;

            namespace First
            {
                public class Model { }
            }
            namespace Second
            {
                public class Model { }
            }
            public static class Calls
            {
                public static void Create(IShapeFactory factory)
                {
                    _ = factory.CreateAsync<First.Model>();
                    _ = factory.CreateAsync<Second.Model>();
                }
            }
            """;

        var compilation = Generate(new ShapeFactoryGenerator(), "Shapes", source);
        var shapes = GetGeneratedShapeTypes(compilation);

        Assert.Equal(2, shapes.Length);
        Assert.NotEqual(shapes[0].Name, shapes[1].Name);
        Assert.Contains(shapes, shape => shape.BaseType.ToDisplayString() == "First.Model");
        Assert.Contains(shapes, shape => shape.BaseType.ToDisplayString() == "Second.Model");
    }

    [Fact]
    public void Generate_SameAssemblyAndModel_ProducesStableShapeNames()
    {
        const string source = """
            using OrchardCore.DisplayManagement;

            public class Model { }
            public static class Calls
            {
                public static void Create(IShapeFactory factory) => _ = factory.CreateAsync<Model>();
            }
            """;

        var first = Generate(new ShapeFactoryGenerator(), "Shapes", source);
        var second = Generate(new ShapeFactoryGenerator(), "Shapes", source);

        Assert.Equal(Assert.Single(GetGeneratedShapeTypes(first)).Name, Assert.Single(GetGeneratedShapeTypes(second)).Name);
    }

    [Fact]
    public void Generate_ArgumentsInterceptorsInReferencedAssembly_KeepsHelpersFileLocal()
    {
        const string source = """
            using OrchardCore.DisplayManagement;

            public static class Calls
            {
                public static INamedEnumerable<object> Create() => Arguments.From(new { Title = "Test" });
            }
            """;

        var producer = Generate(new ArgumentsFromInterceptor(), "Producer", source);
        using var assembly = new MemoryStream();
        var emitResult = producer.Emit(assembly, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(emitResult.Success, string.Join(System.Environment.NewLine, emitResult.Diagnostics));

        var consumer = Generate(new ArgumentsFromInterceptor(), "Consumer",
            source.Replace("class Calls", "class ConsumerCalls", StringComparison.Ordinal),
            MetadataReference.CreateFromImage(assembly.ToArray()));
        var helperTypes = consumer.SyntaxTrees.Skip(1)
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            .ToArray();

        Assert.NotEmpty(helperTypes);
        Assert.All(helperTypes, type => Assert.Contains(type.Modifiers, modifier => modifier.IsKind(SyntaxKind.FileKeyword)));
        AssertCompilesWithoutWarnings(consumer);
    }

    [Fact]
    public void Generate_ArgumentsModelsWithSameName_PreservesNamespaces()
    {
        const string source = """
            using OrchardCore.DisplayManagement;

            namespace First
            {
                [GenerateArguments]
                public partial class Model
                {
                    public int Count { get; set; }
                }
            }
            namespace Second
            {
                [GenerateArguments]
                public partial class Model
                {
                    public string Title { get; set; }
                }
            }
            """;

        var compilation = Generate(new ArgumentsProviderGenerator(), "ArgumentsModels", source);

        Assert.Equal(typeof(PropertyBasedNamedEnumerable).FullName, compilation.GetTypeByMetadataName("First.Model").BaseType.ToDisplayString());
        Assert.Equal(typeof(PropertyBasedNamedEnumerable).FullName, compilation.GetTypeByMetadataName("Second.Model").BaseType.ToDisplayString());
    }

    private static Compilation Generate(IIncrementalGenerator generator, string assemblyName, string source, params MetadataReference[] additionalReferences)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source, s_parseOptions, path: "Source.cs")],
            s_metadataReferences.Concat(additionalReferences),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create([generator.AsSourceGenerator()], parseOptions: s_parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

        Assert.Empty(diagnostics);
        Assert.NotEmpty(Assert.Single(driver.GetRunResult().Results).GeneratedSources);
        AssertCompilesWithoutWarnings(output);

        return output;
    }

    private static INamedTypeSymbol[] GetGeneratedShapeTypes(Compilation compilation)
        => compilation.SyntaxTrees.Skip(1)
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Where(type => type.Identifier.ValueText.StartsWith("GeneratedShape_", StringComparison.Ordinal))
                .Select(type => compilation.GetSemanticModel(tree).GetDeclaredSymbol(type)))
            .ToArray();

    private static void AssertCompilesWithoutWarnings(Compilation compilation)
        => Assert.Empty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning));

    private static MetadataReference[] GetMetadataReferences()
    {
        var trustedPlatformAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        Assert.False(string.IsNullOrEmpty(trustedPlatformAssemblies));

        return trustedPlatformAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Append(typeof(IShape).Assembly.Location)
            .Distinct(StringComparer.Ordinal)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    }
}
