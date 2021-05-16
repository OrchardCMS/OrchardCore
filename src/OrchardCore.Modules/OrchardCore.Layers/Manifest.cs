using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Layers",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Layers",
    Name = "Layers",
    Description = "Enables users to render Widgets across pages of the site based on conditions.",
    Dependencies = new[]
    {
        "OrchardCore.Widgets",
        "OrchardCore.Scripting",
        "OrchardCore.Rules"
    },
    Category = "Content"
)]
