using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Metadata",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "0.0.1",
    Description = "Adds metadata to your content types.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
