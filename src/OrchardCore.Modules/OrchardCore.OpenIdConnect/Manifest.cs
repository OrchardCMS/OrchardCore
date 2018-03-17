using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OpenIDConnect",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The OpenIDConnect module enables authentication on external OIDC provicers.",
    Dependencies = new []
    {
        "OrchardCore.Users",
        "OrchardCore.Roles",
        "OrchardCore.Settings"
        },
    Category = "Security"
)]