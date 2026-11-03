using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Workflows Sample",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Adds an 'Upper case' expression syntax and a 'Transient failure' activity to workflows, for the functional tests.",
    Dependencies = ["OrchardCore.Workflows"],
    Category = "Test"
)]
