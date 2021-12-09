using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Fields",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.ContentFields",
    Name = "Content Fields",
    Category = "Content Management",
    Description = "Content Fields module adds common content fields to be used with your custom types.",
    Dependencies = new[] { "OrchardCore.ContentTypes", "OrchardCore.Shortcodes" }
)]

[assembly: Feature(
    Id = "OrchardCore.ContentFields.Indexing.SQL",
    Name = "Content Fields Indexing (SQL)",
    Category = "Content Management",
    Description = "Content Fields Indexing module adds database indexing for content fields.",
    Dependencies = new[] { "OrchardCore.ContentFields" }
)]

[assembly: Feature(
    Id = "OrchardCore.ContentFields.Indexing.SQL.UserPicker",
    Name = "Content Fields Indexing (SQL) - User Picker",
    Category = "Content Management",
    Description = "User Picker Content Fields Indexing module adds database indexing for user picker fields.",
    Dependencies = new[] { "OrchardCore.ContentFields", "OrchardCore.Users" }
)]
