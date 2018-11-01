using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lucene",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Lucene",
    Name = "Lucene",
    Description = "Creates Lucene indexes to support search scenarios, introduces a preconfigured container-enabled content type.",
    Dependencies = new[]
    {
        "OrchardCore.Indexing",
        "OrchardCore.Liquid"
    },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Lucene.Worker",
    Name = "Lucene Worker",
    Description = "Provides a background task to keep local indices in sync with other instances.",
    Dependencies = new[] { "OrchardCore.Lucene" },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Lucene.ContentPicker",
    Name = "Lucene Content Picker",
    Description = "Provides a Lucene content picker field editor.",
    Dependencies = new[] { "OrchardCore.Lucene", "OrchardCore.ContentFields" },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Lucene.Distributed",
    Name = "Lucene Distributed",
    Description = "Fire content items events through a Message Bus, so that indices are updated by the instance owning the lucene writer.",
    Dependencies = new[] { "OrchardCore.Lucene" },
    Category = "Content Management"
)]
