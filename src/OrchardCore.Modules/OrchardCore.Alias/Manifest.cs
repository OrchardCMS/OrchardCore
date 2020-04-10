using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Alias",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "The alias module enables content items to have custom logical identifier.",
    Dependencies = new[] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]
