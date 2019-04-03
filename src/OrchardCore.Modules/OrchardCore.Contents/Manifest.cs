using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Contents",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Contents",
    Name = "Contents",
    Description = "The contents module enables the edition and rendering of content items.",
    Dependencies = new[]
    {
        "OrchardCore.Settings",
        "OrchardCore.Liquid"
    },
    Category = "Content Management"
)]

[assembly:Feature(
    Id = "OrchardCore.Contents.FileContentDefinition",
    Name = "File Content Definition",
    Description = "Stores Content Definition in a local file.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
