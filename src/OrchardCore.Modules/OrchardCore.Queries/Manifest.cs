using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Queries",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Queries",
    Name = "Queries",
    Description = "Provides querying capabilities.",
    Dependencies = new[] { "OrchardCore.Liquid" },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Queries.Sql",
    Name = "SQL Queries",
    Description = "Introduces a way to create custom Queries in pure SQL.",
    Dependencies = new[] { "OrchardCore.Queries" },
    Category = "Content Management"
)]
