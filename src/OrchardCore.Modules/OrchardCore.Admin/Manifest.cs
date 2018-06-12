using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Admin",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "Creates an admin section for the site.",
    Category = "Infrastructure",
    Dependencies = new[]
    {
        "OrchardCore.Settings"
    }
)]
