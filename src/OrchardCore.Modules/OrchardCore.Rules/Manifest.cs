using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Rules",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Rules",
    Name = "Rules",
    Description = "The Rules module adds rule building capabilities.",
    Dependencies = new[]
    {
        "OrchardCore.Scripting"
    },
    Category = "Infrastructure"
)]
