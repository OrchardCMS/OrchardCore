using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lists",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Lists",
    Name = "Lists",
    Description = "Introduces a preconfigured container-enabled content type.",
    Dependencies = new[] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]
