using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Metadata",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "0.1.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Metadata",
    Name = "Metadata",
    Description = "Adds SEO and social metadata to your content types.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content"
)]
