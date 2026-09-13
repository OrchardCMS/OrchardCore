using System.Text.Json.Serialization;

namespace OrchardCore.Cli;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    WriteIndented = true)]
[JsonSerializable(typeof(CliConfiguration))]
[JsonSerializable(typeof(CachedContentRecord))]
[JsonSerializable(typeof(StoredToken))]
[JsonSerializable(typeof(System.Text.Json.Nodes.JsonObject))]
[JsonSerializable(typeof(ContextListOutput))]
[JsonSerializable(typeof(ContextOutput))]
[JsonSerializable(typeof(ContextClearOutput))]
[JsonSerializable(typeof(LoginOutput))]
[JsonSerializable(typeof(DeviceLoginSession))]
[JsonSerializable(typeof(LogoutOutput))]
[JsonSerializable(typeof(RefreshOutput))]
[JsonSerializable(typeof(CompatibilityOutput))]
[JsonSerializable(typeof(CapabilityCompatibilityOutput))]
[JsonSerializable(typeof(DocumentationSearchOutput))]
[JsonSerializable(typeof(DocumentationSearchHit))]
[JsonSerializable(typeof(DocumentationShowOutput))]
[JsonSerializable(typeof(DoctorOutput))]
[JsonSerializable(typeof(LocalSiteInstallOutput))]
[JsonSerializable(typeof(string[]))]
internal sealed partial class CliJsonContext : JsonSerializerContext
{
}
