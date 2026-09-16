using System.Text.Json;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli.Tests;

public class OutputFormatterTests
{
    [Theory]
    [InlineData("human", "items")]
    [InlineData("table", "items")]
    [InlineData("human", "files")]
    [InlineData("table", "files")]
    public async Task MediaFiles_PathAndLongDirectUrl_RemainUsable(string format, string collection)
    {
        var url = "https://cdn.example.com/" + new string('a', 180) + "/logo.png?v=abc%2F123";
        using var document = JsonDocument.Parse($$"""{"{{collection}}":[{"filePath":"images/logo.png","url":"{{url}}"}]}""");
        using var writer = new StringWriter();
        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement,
            CommandPath = collection == "items" ? ["media", "items", "list"] : ["media", "files", "move-batch"],
            HttpMethod = collection == "items" ? "GET" : "POST",
            TableColumns = [new(collection + "[].filePath", "Path"), new(collection + "[].url", "URL")],
        }, CliUtilities.ParseOutputFormat(format), writer, TestContext.Current.CancellationToken);
        Assert.Contains("images/logo.png", writer.ToString());
        Assert.Contains(url, writer.ToString());
    }

    [Fact]
    public async Task Human_TenantCreated_ReportsSuccessAndCompleteSetupUrl()
    {
        var setupUrl = "https://example.test/Demo/setup?token=" + new string('a', 240);
        using var document = JsonDocument.Parse($$"""{"name":"Demo","state":"Uninitialized","setupUrl":"{{setupUrl}}","canDelete":false,"featureProfiles":[]} """);
        using var writer = new StringWriter();
        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement, CommandPath = ["tenants", "create"], HttpMethod = "POST", StatusCode = 201,
        }, OutputFormat.Human, writer, TestContext.Current.CancellationToken);
        var text = writer.ToString();
        Assert.StartsWith("Tenant 'Demo' created successfully.", text);
        Assert.Contains("Setup URL: " + setupUrl, text);
        Assert.Contains("State: Uninitialized", text);
        Assert.DoesNotContain("Can delete", text);
        Assert.DoesNotContain(" | ", text);
    }

    [Theory]
    [InlineData("{\"name\":\"Demo\"}", 200, "completed successfully.")]
    [InlineData("{\"name\":\"Demo\"}", 202, "Request accepted.")]
    [InlineData("{\"success\":false,\"message\":\"Import failed\"}", 200, "unsuccessful result.")]
    [InlineData("{\"errors\":{\"name\":[\"Name is required\"]}}", 200, "unsuccessful result.")]
    public async Task Human_Mutation_DoesNotOverstateResult(string json, int statusCode, string expected)
    {
        using var document = JsonDocument.Parse(json);
        using var writer = new StringWriter();
        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement, CommandPath = ["tenants", "create"], HttpMethod = "POST", StatusCode = statusCode,
        }, OutputFormat.Human, writer, TestContext.Current.CancellationToken);
        Assert.Contains(expected, writer.ToString());
        Assert.DoesNotContain("created successfully", writer.ToString());
        if (document.RootElement.TryGetProperty("message", out var message))
        {
            Assert.Contains(message.GetString()!, writer.ToString());
        }
        if (document.RootElement.TryGetProperty("errors", out _))
        {
            Assert.Contains("Name is required", writer.ToString());
        }
    }

    [Fact]
    public async Task Human_Install_PrioritizesSiteAndRestartInstructions()
    {
        using var document = JsonDocument.Parse("""{"directory":"/sites/My Site","url":"https://localhost:5001/","listenUrl":"https://localhost:5001;http://localhost:5000","tenant":"Default","tenantState":"Running","sdkVersion":"10.0.400"}""");
        using var writer = new StringWriter();
        await OutputFormatter.WriteAsync(new CommandOutput { Json = document.RootElement, CommandPath = ["install"] },
            OutputFormat.Human, writer, TestContext.Current.CancellationToken);
        Assert.StartsWith("Site created and initialized successfully.", writer.ToString());
        Assert.Contains("Directory: /sites/My Site", writer.ToString());
        Assert.Contains("--urls \"https://localhost:5001;http://localhost:5000\"", writer.ToString());
        Assert.DoesNotContain("10.0.400", writer.ToString());
        Assert.DoesNotContain("Running", writer.ToString());
    }

    [Fact]
    public async Task Human_NoContentMutation_ReportsCompletionWithoutTransportFields()
    {
        using var document = JsonDocument.Parse("""{"statusCode":204,"body":"","contentType":null}""");
        using var writer = new StringWriter();
        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement, CommandPath = ["tenants", "delete"], HttpMethod = "DELETE", StatusCode = 204,
        }, OutputFormat.Human, writer, TestContext.Current.CancellationToken);
        Assert.Equal("Tenant removed successfully.", writer.ToString().Trim());
    }

    [Fact]
    public async Task Human_EscapesUntrustedTerminalSequencesWithoutTruncatingValues()
    {
        using var document = JsonDocument.Parse("""{"name":"Demo\u001b[2J","description":"first\nsecond"}""");
        using var writer = new StringWriter();
        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement, CommandPath = ["tenants", "show"], HttpMethod = "GET", StatusCode = 200,
        }, OutputFormat.Human, writer, TestContext.Current.CancellationToken);
        Assert.DoesNotContain('\u001b', writer.ToString());
        Assert.Contains("Demo\\u001b[2J", writer.ToString());
        Assert.Contains("first\\u000asecond", writer.ToString());
    }

    [Fact]
    public async Task Human_ContextClearCancelled_DoesNotClaimSuccess()
    {
        using var document = JsonDocument.Parse("""{"cleared":false,"deletedContexts":0}""");
        using var writer = new StringWriter();
        await OutputFormatter.WriteAsync(new CommandOutput { Json = document.RootElement, CommandPath = ["context", "clear"] },
            OutputFormat.Human, writer, TestContext.Current.CancellationToken);
        Assert.StartsWith("Cancelled. No contexts were removed.", writer.ToString());
    }

    [Fact]
    public async Task Human_Schema_KeepsJsonSchemaUsable()
    {
        using var document = JsonDocument.Parse("""{"type":"object","properties":{"name":{"type":"string"}}}""");
        using var writer = new StringWriter();
        await OutputFormatter.WriteAsync(new CommandOutput { Json = document.RootElement, CommandPath = ["tenants", "schema"] },
            OutputFormat.Human, writer, TestContext.Current.CancellationToken);
        using var result = JsonDocument.Parse(writer.ToString());
        Assert.Equal("object", result.RootElement.GetProperty("type").GetString());
    }

    [Theory]
    [InlineData("human")]
    [InlineData("table")]
    public async Task ListTables_KeepLongUrlsUsable(string format)
    {
        var url = "https://example.test/" + new string('a', 240);
        using var document = JsonDocument.Parse($$"""[{"url":"{{url}}"}]""");
        using var writer = new StringWriter();
        await OutputFormatter.WriteAsync(new CommandOutput { Json = document.RootElement, CommandPath = ["tenants", "list"] },
            CliUtilities.ParseOutputFormat(format), writer, TestContext.Current.CancellationToken);
        Assert.Contains(url, writer.ToString());
    }

    [Fact]
    public async Task WriteAsync_TableWithControlCharacters_EscapesTerminalSequences()
    {
        using var document = JsonDocument.Parse("""[{"name":"hello\u001b[2J\nworld"}]""");
        using var writer = new StringWriter();
        await OutputFormatter.WriteAsync(new CommandOutput { Json = document.RootElement.Clone() }, OutputFormat.Table, writer, CancellationToken.None);
        Assert.DoesNotContain('\u001b', writer.ToString());
        Assert.Contains("\\u001b", writer.ToString());
        Assert.Contains("\\u000a", writer.ToString());
    }

    [Fact]
    public async Task WriteAsync_EmptyTable_ExplainsEmptyResult()
    {
        using var document = JsonDocument.Parse("[]");
        using var writer = new StringWriter();
        await OutputFormatter.WriteAsync(new CommandOutput { Json = document.RootElement.Clone() }, OutputFormat.Table, writer, CancellationToken.None);
        Assert.Equal("No results.", writer.ToString().Trim());
    }

    [Fact]
    public async Task WriteAsync_WhenTableFormatIsRequested_UsesProvidedColumns()
    {
        using var document = JsonDocument.Parse("[{\"id\":\"a1\",\"name\":\"Alpha\"}]");
        using var writer = new StringWriter();

        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement.Clone(),
            TableColumns = [new CliTableColumnMetadata("id", "Id"), new CliTableColumnMetadata("name", "Name")],
        }, OutputFormat.Table, writer, CancellationToken.None);

        var output = writer.ToString();
        Assert.Contains("Id", output, StringComparison.Ordinal);
        Assert.Contains("Alpha", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WriteAsync_WhenYamlFormatIsRequested_FormatsNestedObject()
    {
        using var document = JsonDocument.Parse("{\"name\":\"tenant\",\"flags\":{\"enabled\":true}}");
        using var writer = new StringWriter();

        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement.Clone(),
        }, OutputFormat.Yaml, writer, CancellationToken.None);

        var output = writer.ToString();
        Assert.Contains("\"name\": \"tenant\"", output, StringComparison.Ordinal);
        Assert.Contains("\"enabled\": true", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WriteAsync_WhenCsvFormatIsRequested_EscapesValues()
    {
        using var document = JsonDocument.Parse("[{\"name\":\"Alpha, Inc.\",\"note\":\"He said \\\"hello\\\"\"}]");
        using var writer = new StringWriter();

        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement.Clone(),
            TableColumns = [new CliTableColumnMetadata("name", "Name"), new CliTableColumnMetadata("note", "Note")],
        }, OutputFormat.Csv, writer, CancellationToken.None);

        var output = writer.ToString();
        Assert.Contains("\"Alpha, Inc.\"", output, StringComparison.Ordinal);
        Assert.Contains("\"He said \"\"hello\"\"\"", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WriteAsync_WhenTsvFormatIsRequested_UsesTabSeparators()
    {
        using var document = JsonDocument.Parse("[{\"id\":\"a1\",\"name\":\"Alpha\"}]");
        using var writer = new StringWriter();

        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement.Clone(),
            TableColumns = [new CliTableColumnMetadata("id", "Id"), new CliTableColumnMetadata("name", "Name")],
        }, OutputFormat.Tsv, writer, CancellationToken.None);

        Assert.Contains("Id\tName", writer.ToString(), StringComparison.Ordinal);
        Assert.Contains("a1\tAlpha", writer.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task WriteAsync_WhenTomlFormatIsRequested_FormatsNestedObjectAndOmitsNulls()
    {
        using var document = JsonDocument.Parse(
            "{\"name\":\"tenant\",\"missing\":null,\"tags\":[\"one\",\"two\"],\"flags\":{\"enabled\":true}}");
        using var writer = new StringWriter();

        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement.Clone(),
        }, OutputFormat.Toml, writer, CancellationToken.None);

        var output = writer.ToString();
        Assert.Contains("\"name\" = \"tenant\"", output, StringComparison.Ordinal);
        Assert.Contains("\"tags\" = [\"one\", \"two\"]", output, StringComparison.Ordinal);
        Assert.Contains("[\"flags\"]", output, StringComparison.Ordinal);
        Assert.Contains("\"enabled\" = true", output, StringComparison.Ordinal);
        Assert.DoesNotContain("missing", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WriteAsync_WhenTomlRootArrayIsRequested_UsesItemsKey()
    {
        using var document = JsonDocument.Parse("[{\"id\":\"a1\"},{\"id\":\"a2\"}]");
        using var writer = new StringWriter();

        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement.Clone(),
        }, OutputFormat.Toml, writer, CancellationToken.None);

        Assert.Contains("items = [{ \"id\" = \"a1\" }, { \"id\" = \"a2\" }]", writer.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("items.id")]
    [InlineData("items[].id")]
    public async Task WriteAsync_WhenTableUsesPagedItems_ExpandsItemRows(string propertyPath)
    {
        using var document = JsonDocument.Parse("{\"page\":1,\"items\":[{\"id\":\"a1\"},{\"id\":\"a2\"}]}");
        using var writer = new StringWriter();

        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement.Clone(),
            TableColumns = [new CliTableColumnMetadata(propertyPath, "Id")],
        }, OutputFormat.Table, writer, CancellationToken.None);

        var output = writer.ToString();
        Assert.Contains("a1", output, StringComparison.Ordinal);
        Assert.Contains("a2", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WriteAsync_WhenTableUsesNestedArray_ExpandsArrayRows()
    {
        using var document = JsonDocument.Parse(
            "{\"currentContext\":\"a\",\"contexts\":[{\"name\":\"a\"},{\"name\":\"b\"}]}");
        using var writer = new StringWriter();

        await OutputFormatter.WriteAsync(new CommandOutput
        {
            Json = document.RootElement.Clone(),
            TableColumns = [new CliTableColumnMetadata("contexts[].name", "Name")],
        }, OutputFormat.Table, writer, CancellationToken.None);

        var output = writer.ToString();
        Assert.Contains("a", output, StringComparison.Ordinal);
        Assert.Contains("b", output, StringComparison.Ordinal);
    }
}
