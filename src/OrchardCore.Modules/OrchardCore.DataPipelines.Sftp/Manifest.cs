using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Data Pipelines - SFTP",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Lets data pipelines upload the files they create to SFTP servers, over SSH.",
    Dependencies =
    [
        "OrchardCore.DataPipelines",
    ],
    Category = "Data"
)]
