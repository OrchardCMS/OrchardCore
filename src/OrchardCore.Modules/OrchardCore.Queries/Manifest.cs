using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Queries",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Queries.Core",
    Name = "Queries Core Services",
    Description = "Provides querying capability services.",
    Dependencies =
    [
        "OrchardCore.Liquid",
    ],
    Category = "Content Management",
    EnabledByDependencyOnly = true
)]

[assembly: Feature(
    Id = "OrchardCore.Queries",
    Name = "Queries",
    Description = "Provides querying capabilities.",
    Dependencies =
    [
        "OrchardCore.Queries.Core",
    ],
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Queries.Sql",
    Name = "SQL Queries",
    Description = "Introduces a way to create custom Queries in pure SQL.",
    Dependencies =
    [
        "OrchardCore.Queries",
    ],
    Category = "Content Management"
)]
