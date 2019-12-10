using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Recipes",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0",
    Description = "Provides Orchard Recipes.",
    Dependencies = new []
    {
        "OrchardCore.Features",
        "OrchardCore.Scripting"
    },
    Category = "Infrastructure",
    IsAlwaysEnabled = true
)]
