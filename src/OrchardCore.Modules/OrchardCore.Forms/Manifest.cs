using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Forms",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Forms",
    Name = "Forms",
    Description = "Provides widgets and activities to implement forms.",
    Dependencies = new[] { "OrchardCore.Widgets", "OrchardCore.Flows" },
    Category = "Content"
)]
