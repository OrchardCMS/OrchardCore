using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lists",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2"
)]
[assembly: Feature(
    Id = "OrchardCore.Lists",
    Name = "Lists",
    Description = "Introduces a preconfigured container-enabled content type.",
    Dependencies = new[] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]
