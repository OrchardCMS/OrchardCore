using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Data Pipelines",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Design pipelines that read, transform and deliver data.",
    Category = "Data"
)]

[assembly: Feature(
    Id = "OrchardCore.DataPipelines",
    Name = "Data Pipelines",
    Description = "Design pipelines that read data from any data source, transform it, and deliver it to FTP and SFTP servers, web APIs and download links.",
    Dependencies =
    [
        "OrchardCore.DataSources",
        "OrchardCore.Users",
    ],
    Category = "Data"
)]

[assembly: Feature(
    Id = "OrchardCore.DataPipelines.Media",
    Name = "Data Pipelines - Media",
    Description = "Lets data pipelines read data files from the media library and save the files they create to it.",
    Dependencies =
    [
        "OrchardCore.DataPipelines",
        "OrchardCore.Media",
    ],
    Category = "Data"
)]

[assembly: Feature(
    Id = "OrchardCore.DataPipelines.Email",
    Name = "Data Pipelines - Email",
    Description = "Lets data pipelines send the files they create by email.",
    Dependencies =
    [
        "OrchardCore.DataPipelines",
        "OrchardCore.Email",
    ],
    Category = "Data"
)]

[assembly: Feature(
    Id = "OrchardCore.DataPipelines.Contents",
    Name = "Data Pipelines - Content Items",
    Description = "Lets data pipelines create and update content items from rows.",
    Dependencies =
    [
        "OrchardCore.DataPipelines",
        "OrchardCore.Contents",
    ],
    Category = "Data"
)]

[assembly: Feature(
    Id = "OrchardCore.DataPipelines.Workflows",
    Name = "Data Pipelines - Workflows",
    Description = "Lets workflows run data pipelines and react to the end of their runs.",
    Dependencies =
    [
        "OrchardCore.DataPipelines",
        "OrchardCore.Workflows",
    ],
    Category = "Data"
)]
