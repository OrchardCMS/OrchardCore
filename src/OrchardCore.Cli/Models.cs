using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.RemoteManagement;

using System.CommandLine;

namespace OrchardCore.Cli;

internal enum OutputFormat
{
    Human,
    Json,
    Table,
    Csv,
    Tsv,
    Yaml,
    Toml,
    None,
}

internal enum CacheKind
{
    Manifest,
    OpenApi,
    Documentation,
}

internal sealed class CliConfiguration
{
    public string? CurrentContext { get; set; }

    public List<TenantContextRecord> Contexts { get; set; } = [];
}

internal sealed class TenantContextRecord
{
    public string Name { get; set; } = string.Empty;

    public string TenantUrl { get; set; } = string.Empty;

    public string? TenantId { get; set; }

    public string? ProductVersion { get; set; }

    public string? Authority { get; set; }

    public string? ClientId { get; set; }

    public string? ManagementManifestUrl { get; set; }

    public string? OpenApiUrl { get; set; }

    public string? DocumentationIndexUrl { get; set; }

    public List<string> GrantTypes { get; set; } = [];

    public List<string> Scopes { get; set; } = [];

    public DateTimeOffset AddedAt { get; set; }
}

internal sealed class StoredToken
{
    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string AccessToken { get; set; } = string.Empty;

    public string? RefreshToken { get; set; }

    public string TokenType { get; set; } = "Bearer";

    public string? Scope { get; set; }

    public string Issuer { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }
}

internal sealed class CachedContentRecord
{
    public string Url { get; set; } = string.Empty;

    public string? ETag { get; set; }

    public string? ApiRevision { get; set; }

    public DateTimeOffset FetchedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public string Content { get; set; } = string.Empty;
}

internal sealed class CachedContentResult
{
    public CachedContentRecord CacheRecord { get; set; } = null!;

    public bool FromCache { get; set; }

    public bool IsStale { get; set; }
}

internal sealed class OidcDiscoveryDocument
{
    public string Issuer { get; set; } = string.Empty;

    public string? AuthorizationEndpoint { get; set; }

    public string TokenEndpoint { get; set; } = string.Empty;

    public string? DeviceAuthorizationEndpoint { get; set; }

    public string? RevocationEndpoint { get; set; }
}

internal sealed class DeviceAuthorizationResponse
{
    public string DeviceCode { get; set; } = string.Empty;

    public string UserCode { get; set; } = string.Empty;

    public string VerificationUri { get; set; } = string.Empty;

    public string? VerificationUriComplete { get; set; }

    public int ExpiresIn { get; set; }

    public int Interval { get; set; } = 5;
}

internal sealed class TokenEndpointResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string? RefreshToken { get; set; }

    public string TokenType { get; set; } = string.Empty;

    public int ExpiresIn { get; set; }

    public string? Scope { get; set; }

    public string? Error { get; set; }

    public string? ErrorDescription { get; set; }
}

internal sealed class RequestBodyPropertyDefinition
{
    public List<string> AllowedTypes { get; set; } = [];

    public string Name { get; set; } = string.Empty;

    public string? Type { get; set; }

    public bool Required { get; set; }

    public string? Description { get; set; }

    public List<string> AllowedValues { get; set; } = [];

    public int? MinimumLength { get; set; }

    public int? MaximumLength { get; set; }

    public double? Minimum { get; set; }

    public double? Maximum { get; set; }
}

internal sealed class SecretBodyPropertyOptions
{
    public required RequestBodyPropertyDefinition Property { get; init; }

    public required Option<string?> EnvironmentVariableOption { get; init; }

    public required Option<FileInfo?> FileOption { get; init; }

    public required Option<bool> StdinOption { get; init; }
}

internal sealed class OpenApiParameterDefinition
{
    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = "query";

    public string Type { get; set; } = "string";

    public bool Required { get; set; }

    public int? ArgumentPosition { get; set; }

    public string? Description { get; set; }
}

internal sealed class OpenApiOperationDefinition
{
    public string Method { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string? OperationId { get; set; }

    public string? Summary { get; set; }

    public string? Description { get; set; }

    public CliOperationMetadata CliMetadata { get; set; } = null!;

    public List<OpenApiParameterDefinition> Parameters { get; set; } = [];

    public List<RequestBodyPropertyDefinition> RequestBodyProperties { get; set; } = [];

    public bool HasJsonRequestBody { get; set; }

    public bool HasBinaryRequestBody { get; set; }

    public bool RequestBodyRequired { get; set; }

    public string? RequestContentType { get; set; }

    public JsonNode? RequestBodySchema { get; set; }
}

internal sealed class DocumentationIndex
{
    public List<DocumentationEntry> Docs { get; set; } = [];
}

internal sealed class DocumentationEntry
{
    public int Id { get; set; }

    public string Location { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}

internal sealed class CommandOutput
{
    public JsonElement Json { get; init; }

    public IReadOnlyList<CliTableColumnMetadata>? TableColumns { get; init; }

    public IReadOnlyList<string> CommandPath { get; init; } = [];

    public string? HttpMethod { get; init; }

    public int? StatusCode { get; init; }

    public string? ContextName { get; init; }

    public string? CurrentContextName { get; init; }

    public IReadOnlyList<TenantContextRecord> KnownContexts { get; init; } = [];
}

internal sealed class ContextListOutput
{
    public string? CurrentContext { get; set; }

    public List<ContextOutput> Contexts { get; set; } = [];
}

internal sealed class ContextOutput
{
    public string Name { get; set; } = string.Empty;

    public string TenantUrl { get; set; } = string.Empty;

    public string? TenantId { get; set; }

    public string? ProductVersion { get; set; }

    public bool IsCurrent { get; set; }

    public bool HasStoredCredentials { get; set; }
}

internal sealed class ContextClearOutput
{
    public bool Cleared { get; set; }

    public int DeletedContexts { get; set; }

    public int DeletedCredentials { get; set; }
}

internal sealed class LoginOutput
{
    public string Context { get; set; } = string.Empty;

    public string GrantType { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public string Issuer { get; set; } = string.Empty;
}

internal sealed class LogoutOutput
{
    public string Context { get; set; } = string.Empty;

    public bool Removed { get; set; }

    public bool Revoked { get; set; }
}

internal sealed class RefreshOutput
{
    public string Context { get; set; } = string.Empty;

    public bool ManifestFromCache { get; set; }

    public bool OpenApiFromCache { get; set; }

    public CompatibilityOutput Compatibility { get; set; } = null!;
}

internal sealed class CompatibilityOutput
{
    public string CliVersion { get; set; } = string.Empty;

    public int ExpectedProtocolMajor { get; set; }

    public int ExpectedProtocolMinor { get; set; }

    public int ManifestProtocolMajor { get; set; }

    public int ManifestProtocolMinor { get; set; }

    public bool ProtocolCompatible { get; set; }

    public bool ServerUsesNewerMinor { get; set; }

    public string? MinimumCliVersion { get; set; }

    public bool MinimumVersionSatisfied { get; set; }

    public string? RecommendedCliVersion { get; set; }

    public bool RecommendedVersionSatisfied { get; set; }

    public List<CapabilityCompatibilityOutput> Capabilities { get; set; } = [];
}

internal sealed class CapabilityCompatibilityOutput
{
    public string Id { get; set; } = string.Empty;

    public string? Version { get; set; }

    public bool MajorCompatible { get; set; }
}

internal sealed class DocumentationSearchOutput
{
    public string Query { get; set; } = string.Empty;

    public List<DocumentationSearchHit> Results { get; set; } = [];
}

internal sealed class DocumentationSearchHit
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Snippet { get; set; } = string.Empty;

    public int Score { get; set; }
}

internal sealed class DocumentationShowOutput
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}

internal sealed class DoctorOutput
{
    public string? LocalInstallSdk { get; set; }

    public string? LocalInstallWarning { get; set; }

    public string CliVersion { get; set; } = string.Empty;

    public string RuntimeIdentifier { get; set; } = string.Empty;

    public string OperatingSystem { get; set; } = string.Empty;

    public string ConfigDirectory { get; set; } = string.Empty;

    public string CacheDirectory { get; set; } = string.Empty;

    public string CredentialStore { get; set; } = string.Empty;

    public string? CurrentContext { get; set; }

    public bool HasOpenApiCache { get; set; }

    public bool HasManifestCache { get; set; }

    public bool HasDocumentationCache { get; set; }
}
