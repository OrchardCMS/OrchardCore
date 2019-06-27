using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "China AliYun Oss",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.AliYun.Storage",
    Name = "AliYun Media Storage",
    Description = "Enables support for storing media files in, and serving them to clients directly from, AliYun Oss",
    Category = "Hosting"
)]
