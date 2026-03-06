using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Tenants",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
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

[assembly: Feature(
    Id = "OrchardCore.Tenants.Distributed",
    Name = "Distributed Tenants",
    Description = "Keeps tenant states in sync. Requires a distributed cache (e.g., Redis Cache) and stateless configuration. See: https://docs.orchardcore.net/en/latest/reference/modules/Shells/index.html",
    Category = "Distributed",
    DefaultTenantOnly = true
)]

[assembly: Feature(
    Id = "OrchardCore.Tenants.FeatureProfiles",
    Name = "Tenant Feature Profiles",
    Description = "Provides a way to manage available features for each tenant.",
    Category = "Infrastructure",
    DefaultTenantOnly = true
)]
