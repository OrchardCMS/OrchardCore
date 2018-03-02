using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OpenID",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The OpenID module enables authentication of external apps through OpenID Connect.",
    Dependencies = new string[]
    {
        "OrchardCore.Authentication",
        "OrchardCore.Users",
        "OrchardCore.Roles",
        "OrchardCore.Settings"
    },
    Category = "Security"
)]
