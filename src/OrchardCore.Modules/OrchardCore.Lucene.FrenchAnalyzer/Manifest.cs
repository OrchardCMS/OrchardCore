using OrchardCore.Modules.Manifest;

[assembly: Feature(
    Id = "OrchardCore.Lucene.FrenchAnalyzer",
    Name = "Lucene French Analyzer",
    Description = "Adds a french analyzer that supports sorting diacritics",
    Dependencies = new[]
    {
        "OrchardCore.Lucene"
    },
    Category = "Content Management"
)]
