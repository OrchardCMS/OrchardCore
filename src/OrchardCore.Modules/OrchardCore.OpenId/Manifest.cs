using OrchardCore.Modules.Manifest;
using OrchardCore.OpenId;

[assembly: Module(
    Name = "OpenID Connect",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = OpenIdConstants.Features.Core,
    Name = "OpenID Connect Core Services",
    Description = "Provides the foundational services for all OpenID Connect features.",
    Category = "OpenID Connect",
    EnabledByDependencyOnly = true
)]

[assembly: Feature(
    Id = OpenIdConstants.Features.Client,
    Name = "OpenID Connect Client Integration",
    Description = "Allows authentication of users through an external OpenID Connect authorization server (also known as an identity provider).",
    Category = "OpenID Connect",
    Dependencies =
    [
        OpenIdConstants.Features.Core,
        "OrchardCore.Users.ExternalAuthentication",
    ]
)]

[assembly: Feature(
    Id = OpenIdConstants.Features.Management,
    Name = "OpenID Connect Management UI",
    Description = "Adds a user interface for managing OpenID Connect applications, scopes and permissions.",
    Category = "OpenID Connect",
    Dependencies =
    [
        OpenIdConstants.Features.Core,
    ]
)]

[assembly: Feature(
    Id = OpenIdConstants.Features.Server,
    Name = "OpenID Connect Authorization Server",
    Description = "Enables Orchard Core to function as an OpenID Connect authorization server/identity provider, supporting authentication and token issuance using OpenID Connect and OAuth 2.0 standards. To enable token validation, activate the 'OpenID Connect Token Validation' feature.",
    Category = "OpenID Connect",
    Dependencies =
    [
        OpenIdConstants.Features.Core,
        OpenIdConstants.Features.Management,
    ]
)]

[assembly: Feature(
    Id = OpenIdConstants.Features.Validation,
    Name = "OpenID Connect Token Validation",
    Description = "Validates tokens issued by the local OpenID Connect authorization server or other trusted servers supporting JWT and OpenID Connect discovery.",
    Category = "OpenID Connect",
    Dependencies =
    [
        OpenIdConstants.Features.Core,
    ]
)]
