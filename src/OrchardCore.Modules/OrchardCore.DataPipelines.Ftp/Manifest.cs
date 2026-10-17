using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Data Pipelines - FTP",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Lets data pipelines upload the files they create to FTP servers.",
    Dependencies =
    [
        "OrchardCore.DataPipelines",
    ],
    Category = "Data"
)]
