using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli;

internal class CliException : Exception
{
    public CliException(string message)
        : base(message)
    {
    }
}

internal static class CliUtilities
{
    public static string CliVersion => typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";

    public static OutputFormat ParseOutputFormat(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            value = "auto";
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "auto" => Console.IsOutputRedirected ? OutputFormat.Json : OutputFormat.Human,
            "human" => OutputFormat.Human,
            "json" => OutputFormat.Json,
            "table" => OutputFormat.Table,
            "csv" => OutputFormat.Csv,
            "tsv" => OutputFormat.Tsv,
            "yaml" => OutputFormat.Yaml,
            "toml" => OutputFormat.Toml,
            "none" => OutputFormat.None,
            _ => throw new CliException($"Unsupported output format '{value}'.")
        };
    }

    public static string ToCliName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var builder = new StringBuilder(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character is '-' or '_' || char.IsWhiteSpace(character))
            {
                if (builder.Length > 0 && builder[^1] != '-')
                {
                    builder.Append('-');
                }

                continue;
            }

            if (char.IsUpper(character)
                && builder.Length > 0
                && builder[^1] != '-'
                && (char.IsLower(value[index - 1])
                    || char.IsDigit(value[index - 1])
                    || (index + 1 < value.Length && char.IsLower(value[index + 1]))))
            {
                builder.Append('-');
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString().TrimEnd('-');
    }

    public static string DetermineOperatingSystem()
    {
        if (OperatingSystem.IsWindows())
        {
            return "windows";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "macos";
        }

        if (OperatingSystem.IsLinux())
        {
            return "linux";
        }

        return "unknown";
    }

    public static JsonElement ToJsonElement<T>(T value, JsonTypeInfo<T> typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        return JsonSerializer.SerializeToElement(value, typeInfo);
    }

    public static JsonElement ToJsonElement(JsonNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        using var document = JsonDocument.Parse(node.ToJsonString());
        return document.RootElement.Clone();
    }

    public static string SerializeToken(StoredToken token) => JsonSerializer.Serialize(token, CliJsonContext.Default.StoredToken);

    public static StoredToken DeserializeToken(string json) =>
        JsonSerializer.Deserialize(json, CliJsonContext.Default.StoredToken)
        ?? throw new CliException("Stored token content is invalid.");

    public static RemoteManagementManifest ParseManifest(string json)
    {
        using var document = JsonDocument.Parse(json);
        return ParseManifest(document.RootElement);
    }

    public static RemoteManagementManifest ParseManifest(JsonElement element)
    {
        var manifest = new RemoteManagementManifest
        {
            ProtocolMajorVersion = ReadInt32(element, "protocolMajorVersion") ?? RemoteManagementConstants.ProtocolMajorVersion,
            ProtocolMinorVersion = ReadInt32(element, "protocolMinorVersion") ?? RemoteManagementConstants.ProtocolMinorVersion,
            ProductVersion = ReadString(element, "productVersion"),
            TenantId = ReadString(element, "tenantId"),
            ManagementManifestUrl = ReadUri(element, "managementManifestUrl"),
            OpenApiUrl = ReadUri(element, "openApiUrl"),
            OpenApiETag = ReadString(element, "openApiETag"),
            JsonSchemaDialect = ReadUri(element, "jsonSchemaDialect"),
            MinimumCliVersion = ReadString(element, "minimumCliVersion"),
            RecommendedCliVersion = ReadString(element, "recommendedCliVersion"),
            DocumentationIndexUrl = ReadUri(element, "documentationIndexUrl"),
        };

        if (element.TryGetProperty("authentication", out var authenticationElement) && authenticationElement.ValueKind == JsonValueKind.Object)
        {
            manifest.Authentication.Authority = ReadUri(authenticationElement, "authority");
            manifest.Authentication.ClientId = ReadString(authenticationElement, "clientId") ?? RemoteManagementConstants.CliClientId;

            foreach (var value in ReadStringArray(authenticationElement, "grantTypes"))
            {
                manifest.Authentication.GrantTypes.Add(value);
            }

            foreach (var value in ReadStringArray(authenticationElement, "scopes"))
            {
                manifest.Authentication.Scopes.Add(value);
            }
        }

        if (element.TryGetProperty("capabilities", out var capabilitiesElement) && capabilitiesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var capabilityElement in capabilitiesElement.EnumerateArray())
            {
                manifest.Capabilities.Add(new RemoteManagementCapability
                {
                    Id = ReadString(capabilityElement, "id"),
                    Version = ReadString(capabilityElement, "version"),
                    DisplayName = ReadString(capabilityElement, "displayName"),
                });
            }
        }

        return manifest;
    }

    public static OidcDiscoveryDocument ParseDiscoveryDocument(string json)
    {
        using var document = JsonDocument.Parse(json);
        return new OidcDiscoveryDocument
        {
            Issuer = ReadRequiredString(document.RootElement, "issuer"),
            AuthorizationEndpoint = ReadString(document.RootElement, "authorization_endpoint"),
            TokenEndpoint = ReadRequiredString(document.RootElement, "token_endpoint"),
            DeviceAuthorizationEndpoint = ReadString(document.RootElement, "device_authorization_endpoint"),
            RevocationEndpoint = ReadString(document.RootElement, "revocation_endpoint"),
        };
    }

    public static DeviceAuthorizationResponse ParseDeviceAuthorizationResponse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return new DeviceAuthorizationResponse
        {
            DeviceCode = ReadRequiredString(document.RootElement, "device_code"),
            UserCode = ReadRequiredString(document.RootElement, "user_code"),
            VerificationUri = ReadRequiredString(document.RootElement, "verification_uri"),
            VerificationUriComplete = ReadString(document.RootElement, "verification_uri_complete"),
            ExpiresIn = ReadInt32(document.RootElement, "expires_in") ?? 900,
            Interval = ReadInt32(document.RootElement, "interval") ?? 5,
        };
    }

    public static TokenEndpointResponse ParseTokenResponse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return new TokenEndpointResponse
        {
            AccessToken = ReadString(document.RootElement, "access_token") ?? string.Empty,
            RefreshToken = ReadString(document.RootElement, "refresh_token"),
            TokenType = ReadString(document.RootElement, "token_type") ?? "Bearer",
            ExpiresIn = ReadInt32(document.RootElement, "expires_in") ?? 3600,
            Scope = ReadString(document.RootElement, "scope"),
            Error = ReadString(document.RootElement, "error"),
            ErrorDescription = ReadString(document.RootElement, "error_description"),
        };
    }

    public static DocumentationIndex ParseDocumentationIndex(string json)
    {
        using var document = JsonDocument.Parse(json);
        var index = new DocumentationIndex();

        if (!document.RootElement.TryGetProperty("docs", out var docsElement) || docsElement.ValueKind != JsonValueKind.Array)
        {
            return index;
        }

        var id = 1;
        foreach (var docElement in docsElement.EnumerateArray())
        {
            index.Docs.Add(new DocumentationEntry
            {
                Id = id++,
                Location = ReadString(docElement, "location") ?? string.Empty,
                Title = ReadString(docElement, "title") ?? string.Empty,
                Text = NormalizeWhitespace(System.Net.WebUtility.HtmlDecode(StripHtml(ReadString(docElement, "text") ?? string.Empty))),
            });
        }

        return index;
    }

    public static StoredToken CreateStoredToken(TokenEndpointResponse response, OidcDiscoveryDocument discovery)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(discovery);

        // This is an OAuth resource client. Identity claims from an ID token are
        // neither used nor persisted; the authority comes from validated discovery.
        if (string.IsNullOrWhiteSpace(response.AccessToken))
        {
            throw new CliException("The token response did not contain an access token.");
        }

        return new StoredToken
        {
            AccessToken = response.AccessToken ?? throw new CliException("The token response did not contain an access token."),
            RefreshToken = response.RefreshToken,
            TokenType = string.IsNullOrWhiteSpace(response.TokenType) ? "Bearer" : response.TokenType,
            Scope = response.Scope,
            Issuer = discovery.Issuer,
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn > 0 ? response.ExpiresIn : 3600),
        };
    }

    public static void EnsureIssuerMatches(string expectedIssuer, string actualIssuer)
    {
        if (!Uri.TryCreate(expectedIssuer, UriKind.Absolute, out var expected) || !Uri.TryCreate(actualIssuer, UriKind.Absolute, out var actual))
        {
            throw new CliException("The OpenID issuer is invalid.");
        }

        var expectedValue = expected.AbsoluteUri.TrimEnd('/');
        var actualValue = actual.AbsoluteUri.TrimEnd('/');
        if (!string.Equals(expectedValue, actualValue, StringComparison.Ordinal))
        {
            throw new CliException($"Issuer validation failed. Expected '{expectedValue}', received '{actualValue}'.");
        }
    }

    public static byte[] DecodeBase64Url(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Replace('-', '+').Replace('_', '/');
        var padding = normalized.Length % 4;
        if (padding > 0)
        {
            normalized = normalized.PadRight(normalized.Length + (4 - padding), '=');
        }

        return Convert.FromBase64String(normalized);
    }

    public static string Base64UrlEncode(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static string NormalizeWhitespace(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var builder = new StringBuilder(value.Length);
        var seenWhitespace = false;

        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character))
            {
                if (!seenWhitespace)
                {
                    builder.Append(' ');
                    seenWhitespace = true;
                }
            }
            else
            {
                builder.Append(character);
                seenWhitespace = false;
            }
        }

        return builder.ToString().Trim();
    }

    public static string StripHtml(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var builder = new StringBuilder(value.Length);
        var insideTag = false;

        foreach (var character in value)
        {
            if (character == '<')
            {
                insideTag = true;
                continue;
            }

            if (character == '>')
            {
                insideTag = false;
                builder.Append(' ');
                continue;
            }

            if (!insideTag)
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    public static JsonNode? ConvertToJsonNode(string? value, string schemaType)
    {
        if (value is null)
        {
            return null;
        }

        return schemaType switch
        {
            "integer" => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue)
                ? JsonValue.Create(intValue)
                : throw new CliException($"'{value}' is not a valid integer."),
            "number" => double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var doubleValue)
                ? JsonValue.Create(doubleValue)
                : throw new CliException($"'{value}' is not a valid number."),
            "boolean" => bool.TryParse(value, out var boolValue)
                ? JsonValue.Create(boolValue)
                : throw new CliException($"'{value}' is not a valid boolean."),
            "array" => value.TrimStart().StartsWith('[') ? JsonNode.Parse(value) : CreateArrayNode(value),
            "object" => JsonNode.Parse(value),
            _ => JsonValue.Create(value),
        };
    }

    public static JsonNode CreateArrayNode(string value)
    {
        var items = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return JsonNode.Parse(JsonSerializer.Serialize(items, CliJsonContext.Default.StringArray))
            ?? throw new CliException("Failed to create an array JSON value.");
    }

    public static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.String => property.GetString(),
            _ => property.ToString(),
        };
    }

    public static string ReadRequiredString(JsonElement element, string propertyName) =>
        ReadString(element, propertyName) ?? throw new CliException($"Missing required JSON property '{propertyName}'.");

    public static int? ReadInt32(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
            ? value
            : null;
    }

    public static Uri? ReadUri(JsonElement element, string propertyName)
    {
        var value = ReadString(element, propertyName);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            ? uri
            : throw new CliException($"JSON property '{propertyName}' must be an absolute URI.");
    }

    public static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var values = new List<string>();
        foreach (var item in property.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String && item.GetString() is { } stringValue)
            {
                values.Add(stringValue);
            }
        }

        return values;
    }
}
