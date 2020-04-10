using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Admin",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "Creates an admin section for the site.",
    Category = "Infrastructure",
    Dependencies = new[]
    {
        "OrchardCore.Settings"
    }
)]
