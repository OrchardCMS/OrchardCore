using OrchardCore.Features;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Features",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = FeaturesConstants.FeatureId,
    Name = "Features",
    Description = "The Features module enables the administrator of the site to manage the installed modules as well as activate and de-activate features.",
    Dependencies = new[] { "OrchardCore.Resources" },
    Category = "Infrastructure",
    IsAlwaysEnabled = true
)]
