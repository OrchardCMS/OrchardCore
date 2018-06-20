using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Forms",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Forms",
    Name = "Forms",
    Description = "Provides widgets and activities to implement forms.",
    Dependencies = new [] { "OrchardCore.Widgets", "OrchardCore.Flows" },
    Category = "Content"
)]