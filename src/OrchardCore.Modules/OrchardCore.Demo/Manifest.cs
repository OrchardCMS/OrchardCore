using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Orchard Demo",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "Test",
    Category = "Samples"
)]

[assembly: Feature(
    Id = "OrchardCore.Demo",
    Name = "Orchard Demo",
    Description = "Test",
    Category = "Samples"
)]

[assembly: Feature(
    Id = "OrchardCore.Demo.Users.EntityFrameworkCore",
    Name = "Users Demo (Entity Framework Store)",
    Description = "Use Entity Framework Core ORM in order to store User data",
    Dependencies = new[] { "OrchardCore.Lucene" },
    Category = "Samples"
)]
