using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Flows",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Flows",
    Name = "Flows",
    Description = "Provides a content part allowing users to edit their content based on Widgets.",
    Dependencies = new [] { "OrchardCore.Widgets" },
    Category = "Content"
)]
