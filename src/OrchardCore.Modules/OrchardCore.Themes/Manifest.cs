using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Themes",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "Themes.",
    Dependencies = new[] { "OrchardCore.Admin" },
    Category = "Theming"
)]
