using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Widgets",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Widgets",
    Name = "Widgets",
    Description = "Provides a part allowing content items to render Widgets in theme zones.",
    Dependencies = new[] { "OrchardCore.ContentTypes" },
    Category = "Content"
)]
