namespace OrchardCore.Cli.Tests;

public class CliUtilitiesTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("auto")]
    public void ParseOutputFormat_AutomaticFormat_FollowsOutputRedirection(string? value)
    {
        Assert.Equal(Console.IsOutputRedirected ? OutputFormat.Json : OutputFormat.Human,
            CliUtilities.ParseOutputFormat(value));
    }

    [Theory]
    [InlineData("array", "[1,2]", "[1,2]")]
    [InlineData("array", "Editor,Author", "[\"Editor\",\"Author\"]")]
    [InlineData("object", "{\"limit\":3}", "{\"limit\":3}")]
    [InlineData("integer", "2147483648", "2147483648")]
    public void ConvertToJsonNode_StructuredOptions_PreservesTypes(string type, string input, string expected)
    {
        Assert.Equal(expected, CliUtilities.ConvertToJsonNode(input, type)!.ToJsonString());
    }

    [Fact]
    public void CreateStoredToken_UntrustedIdToken_IsNotUsedAsIdentity()
    {
        var response = CliUtilities.ParseTokenResponse("""{"access_token":"access","id_token":"untrusted"}""");
        var token = CliUtilities.CreateStoredToken(response, new OidcDiscoveryDocument
        {
            Issuer = "https://example.test/",
            TokenEndpoint = "https://example.test/connect/token",
        });
        Assert.Equal("https://example.test/", token.Issuer);
        Assert.DoesNotContain("untrusted", System.Text.Json.JsonSerializer.Serialize(token, CliJsonContext.Default.StoredToken));
    }

    [Theory]
    [InlineData("json", "Json")]
    [InlineData("human", "Human")]
    [InlineData("table", "Table")]
    [InlineData("csv", "Csv")]
    [InlineData("tsv", "Tsv")]
    [InlineData("yaml", "Yaml")]
    [InlineData("toml", "Toml")]
    [InlineData("none", "None")]
    public void ParseOutputFormat_SupportedFormat_ReturnsFormat(string? value, string expected)
    {
        Assert.Equal(expected, CliUtilities.ParseOutputFormat(value).ToString());
    }

    [Theory]
    [InlineData("jsonc")]
    public void ParseOutputFormat_RemovedFormat_ThrowsFriendlyCliException(string value)
    {
        var exception = Assert.Throws<CliException>(() => CliUtilities.ParseOutputFormat(value));

        Assert.Equal($"Unsupported output format '{value}'.", exception.Message);
    }

    [Theory]
    [InlineData("PageSize", "page-size")]
    [InlineData("featureId", "feature-id")]
    [InlineData("URLValue", "url-value")]
    [InlineData("already-kebab", "already-kebab")]
    [InlineData("snake_case", "snake-case")]
    public void ToCliName_OpenApiName_ReturnsLowerKebabCase(string value, string expected)
    {
        Assert.Equal(expected, CliUtilities.ToCliName(value));
    }

    [Fact]
    public void ParseManifest_NullOptionalUris_LeavesUrisUnset()
    {
        var manifest = CliUtilities.ParseManifest("""
            {
              "protocolMajorVersion": 1,
              "protocolMinorVersion": 0,
              "authentication": {
                "authority": "https://example.com/",
                "clientId": "orchardcore-cli",
                "grantTypes": ["authorization_code"],
                "scopes": ["orchardcore.management"]
              },
              "managementManifestUrl": "https://example.com/api/management/manifest",
              "openApiUrl": null,
              "jsonSchemaDialect": null,
              "documentationIndexUrl": null
            }
            """);

        Assert.Equal("https://example.com/api/management/manifest", manifest.ManagementManifestUrl.AbsoluteUri);
        Assert.Null(manifest.OpenApiUrl);
        Assert.Null(manifest.JsonSchemaDialect);
        Assert.Null(manifest.DocumentationIndexUrl);
    }

    [Fact]
    public void ParseManifest_InvalidUri_ThrowsFriendlyCliException()
    {
        var exception = Assert.Throws<CliException>(() => CliUtilities.ParseManifest("""
            {
              "managementManifestUrl": "not a URI"
            }
            """));

        Assert.Equal("JSON property 'managementManifestUrl' must be an absolute URI.", exception.Message);
    }
}
