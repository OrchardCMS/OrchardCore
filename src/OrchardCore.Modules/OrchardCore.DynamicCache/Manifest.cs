using OrchardCore.DynamicCache;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Dynamic Cache",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "Dynamic Cache.",
    Category = "Performance"
)]

[assembly: Feature(
    Id = Startup.FeatureName,
    Name = "Dynamic Cache",
    Category = "Performance",
    Description = "Dynamic Cache."
)]

[assembly: Feature(
    Id = AntiforgeryStartup.FeatureName,
    Name = "Dynamic Cache For Antiforgery Tokens",
    Category = "Performance",
    Description = "Adds support for antiforgery tokens to be cached with the Dynamic Cache",
    Dependencies = new[] { "OrchardCore.DynamicCache" }
)]
