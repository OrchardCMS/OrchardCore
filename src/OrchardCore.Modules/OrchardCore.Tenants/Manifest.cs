using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Tenants",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2"
)]

[assembly: Feature(
    Id = "OrchardCore.Tenants",
    Name = "Tenants",
    Description = "Provides a way to manage tenants from the admin.",
    Category = "Infrastructure",
    DefaultTenantOnly = true
)]

[assembly: Feature(
    Id = "OrchardCore.Tenants.FileProvider",
    Name = "Static File Provider",
    Description = "Provides a way to serve independent static files for each tenant.",
    Category = "Infrastructure",
    DefaultTenantOnly = false
)]
