using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Flows",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Flows",
    Name = "Flows",
    Description = "Provides a content part allowing users to edit their content based on Widgets.",
    Dependencies = new[] { "OrchardCore.Widgets" },
    Category = "Content"
)]
