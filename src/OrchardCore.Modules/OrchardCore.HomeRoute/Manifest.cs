using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Home Route",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.HomeRoute",
    Name = "Home Route",
    Description = "Provides a way to set the route corresponding to the homepage of the site",
    Category = "Infrastructure"
)]
