using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using OrchardCore.DisplayManagement.SourceGenerators;

namespace OrchardCore.Tests.SourceGenerators;

#nullable enable

public class PlacementLocationAnalyzerTests
{
    [Theory]
    [InlineData("result.Location(location)")]
    [InlineData("result.Location(\"Detail\", location)")]
    [InlineData("result.Location($\"Content:1#{location}\")")]
    [InlineData("result.Location(\"Content:1#\" + location)")]
    [InlineData("result.Location(l => l.Tab(location))")]
    [InlineData("result.Location(l => l.Zone(\"Content\").Card(location))")]
    public async Task Analyze_NonConstantPlacement_ReportsNonConstantPlacement(string expression)
    {
        // Arrange
        var source = $$"""
            public class Driver
            {
                public void Build(ShapeResult result, string location, int position) => {{expression}};
            }
            """;

        // Act
        var diagnostic = Assert.Single(await GetDiagnosticsAsync(source));

        // Assert
        Assert.Equal(PlacementLocationAnalyzer.NonConstantPlacementDiagnosticId, diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
    }

    [Theory]
    [InlineData("result.Location(\"Content:1#Settings\")")]
    [InlineData("result.Location(\"Detail\", Location)")]
    [InlineData("result.Location(PlacementInfo.HiddenLocation)")]
    [InlineData("result.Location(position > 0 ? \"Content:1#Settings\" : \"Content:1\")")]
    [InlineData("result.Location(position switch { 0 => \"Content:1#A\", _ => \"Content:1#B\" })")]
    [InlineData("result.Location(\"Actions:40.\" + position)")]
    [InlineData("result.Location($\"Parts:{position}\")")]
    [InlineData("result.Location(l => l.Zone(\"Content\", position.ToString()).Tab(\"Settings\").Card(\"Details\"))")]
    public async Task Analyze_ConstantPlacement_ReportsNothing(string expression)
    {
        // Arrange
        var source = $$"""
            public class Driver
            {
                private const string Location = "Content:2#General";

                public void Build(ShapeResult result, string location, int position) => {{expression}};
            }
            """;

        // Act & Assert
        Assert.Empty(await GetDiagnosticsAsync(source));
    }

    [Fact]
    public async Task Analyze_DeclaredLocations_ReportsNothing()
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

            public class DerivedDriver : BaseDriver
            {
                public void BuildAgain(ShapeResult result) => result.Location(EditorLocation);
            }
            """;

        // Act & Assert
        Assert.Empty(await GetDiagnosticsAsync(source));
    }

    [Fact]
    public async Task Analyze_SkippedMember_ReportsNothing()
    {
        // Arrange
        const string source = """
            public static class PlacementHelper
            {
                [SkipLocalizationExtraction]
                public static ShapeResult Place(ShapeResult result, string location) => result.Location(location);
            }
            """;

        // Act & Assert
        Assert.Empty(await GetDiagnosticsAsync(source));
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var diagnostics = await PlacementTestCompilation.Create(source)
            .WithAnalyzers([new PlacementLocationAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();

        return diagnostics.OrderBy(diagnostic => diagnostic.Location.SourceSpan.Start).ToImmutableArray();
    }
}
