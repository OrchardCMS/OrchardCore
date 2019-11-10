using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Microsoft Azure Tenants",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Tenants.FileProvider.Azure",
    Name = "Azure Blob Storage Static File Provider",
    Description = "Provides a way to serve independent static files from Azure Blob Storage for each tenant.",
    Category = "Infrastructure",
    Dependencies = new[] { "OrchardCore.Tenants.FileProvider" },
    DefaultTenantOnly = false
)]