using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Recipes",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "Provides Orchard Recipes.",
    Dependencies = new[]
    {
        "OrchardCore.Features",
        "OrchardCore.Scripting"
    },
    Category = "Infrastructure",
    IsAlwaysEnabled = true
)]
