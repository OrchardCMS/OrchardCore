using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Setup",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "The setup module is creating the application's setup experience.",
    Dependencies = new[] { "OrchardCore.Recipes" },
    Category = "Infrastructure"
)]
