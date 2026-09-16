using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Remote Management",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides secure remote management discovery for Orchard Core tenants.",
    Category = "Api"
)]

[assembly: Feature(
    Id = "OrchardCore.RemoteManagement",
    Name = "Remote Management",
    Description = "Enables shared remote management discovery, permissions, and authentication.",
    Category = "Api",
    Dependencies =
    [
        "OrchardCore.OpenApi",
        "OrchardCore.OpenId.RemoteManagement",
    ]
)]

[assembly: Feature(
    Id = "OrchardCore.RemoteManagement.Mcp",
    Name = "Remote Management MCP",
    Description = "Exposes tenant remote management operations as Model Context Protocol tools.",
    Category = "Api",
    Dependencies = ["OrchardCore.RemoteManagement", "OrchardCore.OpenId.RemoteManagement.Mcp"]
)]

[assembly: Feature(
    Id = "OrchardCore.RemoteManagement.Cli",
    Name = "Remote Management CLI",
    Description = "Enables Pomi CLI discovery, metadata, and OpenID application configuration.",
    Category = "Api",
    Dependencies = ["OrchardCore.RemoteManagement", "OrchardCore.OpenId.RemoteManagement.Cli"]
)]
