using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Flows",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2"
)]

[assembly: Feature(
    Id = "OrchardCore.Flows",
    Name = "Flows",
    Description = "Provides a content part allowing users to edit their content based on Widgets.",
    Dependencies = new[] { "OrchardCore.Widgets" },
    Category = "Content"
)]
