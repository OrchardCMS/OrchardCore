using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "SMTP Email Secrets",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Email.Smtp.Secrets",
    Name = "SMTP Email Secrets",
    Description = "Allows storing SMTP credentials as secrets instead of in settings.",
    Dependencies =
    [
        "OrchardCore.Email.Smtp",
        "OrchardCore.Secrets",
    ],
    Category = "Email"
)]
