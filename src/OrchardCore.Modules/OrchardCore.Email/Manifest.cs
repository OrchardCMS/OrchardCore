using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Email",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Email",
    Name = "Email",
    Description = "Provides email settings configuration.",
    Dependencies = ["OrchardCore.Resources"],
    Category = "Messaging"
)]

[assembly: Feature(
    Id = "OrchardCore.Email.Smtp",
    Name = "SMTP Email",
    Description = "Provides email settings configuration and a default email service based on SMTP",
    Dependencies = ["OrchardCore.Email"],
    Category = "Messaging"
)]
