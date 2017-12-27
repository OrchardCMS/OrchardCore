using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Antiforgery",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "Providers tenant-aware antiforgery services.",
    Category = "Security",
    Dependencies = new[] { "OrchardCore.DataProtection" }
)]
