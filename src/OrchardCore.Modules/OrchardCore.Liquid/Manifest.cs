using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Liquid",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Liquid",
    Name = "Liquid",
    Description = "The liquid module enables content items to have liquid syntax.",
    Dependencies = ["OrchardCore.Liquid.Core"],
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Liquid.Core",
    Name = "Liquid Core Services",
    Description = "Provides liquid core services.",
    EnabledByDependencyOnly = true,
    IsAlwaysEnabled = true,
    Category = "Content Management"
)]
