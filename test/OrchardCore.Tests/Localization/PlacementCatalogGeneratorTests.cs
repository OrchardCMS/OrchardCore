using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Localization.Analyzers;

namespace OrchardCore.Tests.Localization;

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
        var catalog = Assert.Single(GenerateCatalogs(source));

        // Assert
        Assert.Equal("Example.Driver", catalog.DeclaringType.FullName);
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
        var catalog = Assert.Single(GenerateCatalogs(source));

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
        Assert.Equal(["Content:7#Capabilities;9"], catalogs.Single(catalog => catalog.DeclaringType.Name == "BaseDriver").Locations);
        Assert.Equal(["Parameters:8#Tools;3"], catalogs.Single(catalog => catalog.DeclaringType.Name == "DerivedDriver").Locations);
    }

    [Fact]
    public void Generate_GenericAndPrivateNestedTypes_ListsTheAccessibleTypeDefinitions()
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
            }
            """;

        // Act
        var catalogs = GenerateCatalogs(source);

        // Assert
        Assert.Equal(["Content:1#Generic"], catalogs.Single(catalog => catalog.DeclaringType.IsGenericTypeDefinition).Locations);
        Assert.Equal(["Content:1#Nested"], catalogs.Single(catalog => catalog.DeclaringType.Name == "Outer").Locations);
    }

    [Fact]
    public void Generate_NoTabOrCard_GeneratesNothing()
    {
        // Arrange
        const string source = """
            public class Driver
            {
                public void Build(ShapeResult result, string location)
                {
                    result.Location("Content:1");
                    result.Location(location);
                }
            }
            """;

        // Act
        var result = RunGenerator(source, out _);

        // Assert
        Assert.Empty(result.GeneratedTrees);
    }

    private static PlacementCatalogAttribute[] GenerateCatalogs(string source)
    {
        var result = RunGenerator(source, out var compilation);
        Assert.Single(result.GeneratedTrees);
        Assert.Empty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        // The generated attributes are read back as the runtime reads them.
        using var stream = new MemoryStream();
        Assert.True(compilation.Emit(stream).Success);

        return Assembly.Load(stream.ToArray()).GetCustomAttributes<PlacementCatalogAttribute>().ToArray();
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
