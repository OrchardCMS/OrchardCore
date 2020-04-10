using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Forms",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2"
)]
[assembly: Feature(
    Id = "OrchardCore.Forms",
    Name = "Forms",
    Description = "Provides widgets and activities to implement forms.",
    Dependencies = new[] { "OrchardCore.Widgets", "OrchardCore.Flows" },
    Category = "Content"
)]
