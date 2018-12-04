using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Queries",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Queries",
    Name = "Queries",
    Description = "Provides querying capabilities.",
    Dependencies = new [] { "OrchardCore.Liquid" },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Queries.Sql",
    Name = "SQL Queries",
    Description = "Introduces a way to create custom Queries in pure SQL.",
    Dependencies = new [] { "OrchardCore.Queries" },
    Category = "Content Management"
)]
