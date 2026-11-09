using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.SourceGenerators;

namespace OrchardCore.Tests.SourceGenerators;

#nullable enable

public class PlacementCatalogGeneratorTests
{
    [Fact]
    public void Generate_ConstantLocations_ListsThoseWithATabOrACard()
    {
        // Arrange
        const string source = """
            public class Driver
            {
                private const string Location = "Content:2#General";

                public void Build(ShapeResult result, bool summary)
                {
                    result.Location("Content:1#Settings;2%Details");
                    result.Location("Detail", Location);
                    result.Location(summary ? "Parts:1#Summary" : "Parts:1");
                    result.Location("Content:5");
                }
            }
            """;

        // Act
        var (declaringType, catalog) = Assert.Single(GenerateCatalogs(source));

        // Assert
        Assert.Equal("Example.Driver", declaringType.FullName);
        Assert.Equal(["Content:1#Settings;2%Details", "Content:2#General", "Parts:1#Summary"], catalog.Locations);
        Assert.Empty(catalog.Tabs);
        Assert.Empty(catalog.Cards);
    }

    [Fact]
    public void Generate_BuilderNames_ListsTabsAndCards()
    {
        // Arrange
        const string source = """
            public class Driver
            {
                public void Build(ShapeResult result)
                    => result.Location(l => l.Zone("Content", "5").Tab("Advanced", "1").Card("Details"));
            }
            """;

        // Act
        var catalog = Assert.Single(GenerateCatalogs(source)).Value;

        // Assert
        Assert.Empty(catalog.Locations);
        Assert.Equal(["Advanced"], catalog.Tabs);
        Assert.Equal(["Details"], catalog.Cards);
    }

    [Fact]
    public void Generate_DeclaredLocations_ListsThemForTheirTypes()
    {
        // Arrange
        const string source = """
            [PlacementLocation(DefaultEditorLocation)]
            public class BaseDriver
            {
                private const string DefaultEditorLocation = "Content:7#Capabilities;9";

                protected virtual string EditorLocation => DefaultEditorLocation;

                public void Build(ShapeResult result) => result.Location(EditorLocation);
            }

            [PlacementLocation("Parameters:8#Tools;3")]
            public class DerivedDriver : BaseDriver
            {
                protected override string EditorLocation => "Parameters:8#Tools;3";
            }
            """;

        // Act
        var catalogs = GenerateCatalogs(source);

        // Assert
        Assert.Equal(["Content:7#Capabilities;9"], catalogs.Single(pair => pair.Key.Name == "BaseDriver").Value.Locations);
        Assert.Equal(["Parameters:8#Tools;3"], catalogs.Single(pair => pair.Key.Name == "DerivedDriver").Value.Locations);
    }

    [Fact]
    public void Generate_GenericNestedAndKeywordNamedTypes_NamesTheTypesThemselves()
    {
        // Arrange
        const string source = """
            public class GenericDriver<TModel>
            {
                public void Build(ShapeResult result) => result.Location("Content:1#Generic");
            }

            internal class Outer
            {
                private sealed class NestedDriver
                {
                    public void Build(ShapeResult result) => result.Location("Content:1#Nested");
                }

                private class Hidden
                {
                    public class PublicDriver
                    {
                        public void Build(ShapeResult result) => result.Location("Content:1#Hidden");
                    }
                }
            }

            public class @event
            {
                public void Build(ShapeResult result) => result.Location("Content:1#Keyword");
            }

            file class FileDriver
            {
                public void Build(ShapeResult result) => result.Location("Content:1#File");
            }
            """;

        // Act
        var catalogs = GenerateCatalogs(source);

        // Assert
        Assert.Equal(["Content:1#Generic"], catalogs.Single(pair => pair.Key.IsGenericTypeDefinition).Value.Locations);
        Assert.Equal(["Content:1#Nested"], catalogs.Single(pair => pair.Key.FullName == "Example.Outer+NestedDriver").Value.Locations);
        Assert.Equal(["Content:1#Hidden"], catalogs.Single(pair => pair.Key.FullName == "Example.Outer+Hidden+PublicDriver").Value.Locations);
        Assert.Equal(["Content:1#Keyword"], catalogs.Single(pair => pair.Key.FullName == "Example.event").Value.Locations);
        Assert.Equal(["Content:1#File"], catalogs.Single(pair => pair.Key.Name.EndsWith("FileDriver", StringComparison.Ordinal)).Value.Locations);
    }

    [Fact]
    public void Generate_NoConstantTabOrCard_GeneratesNothing()
    {
        // Arrange
        const string source = """
            public class Driver
            {
                public void Build(ShapeResult result, string location, int position)
                {
                    result.Location("Content:1");
                    result.Location(location);
                    result.Location("Actions:40." + position);
                    result.Location("Content:" + position + PlacementInfo.TabDelimiter + "Settings");
                    result.Location(l => l.Tab($"Section{position}"));
                }
            }
            """;

        // Act
        var result = RunGenerator(source, out _);

        // Assert
        Assert.Empty(result.GeneratedTrees);
    }

    private static Dictionary<Type, PlacementCatalogAttribute> GenerateCatalogs(string source)
    {
        var result = RunGenerator(source, out var compilation);
        Assert.Single(result.GeneratedTrees);
        Assert.Empty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        // The generated attributes are read back, and their type names resolved, as the runtime does.
        using var stream = new MemoryStream();
        Assert.True(compilation.Emit(stream).Success);
        var assembly = Assembly.Load(stream.ToArray());

        return assembly.GetCustomAttributes<PlacementCatalogAttribute>()
            .ToDictionary(catalog => assembly.GetType(catalog.DeclaringTypeName, throwOnError: true)!, catalog => catalog);
    }

    private static GeneratorDriverRunResult RunGenerator(string source, out Compilation compilation)
    {
        var driver = CSharpGeneratorDriver
            .Create([new PlacementCatalogGenerator().AsSourceGenerator()], parseOptions: PlacementTestCompilation.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(PlacementTestCompilation.Create(source), out compilation, out var diagnostics);

        Assert.Empty(diagnostics);

        return driver.GetRunResult();
    }
}
