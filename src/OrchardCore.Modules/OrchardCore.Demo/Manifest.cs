using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Orchard Demo",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Test",
    Category = "Samples",
    Dependencies = new []{ "OrchardCore.Users", "OrchardCore.Contents" }
)]
