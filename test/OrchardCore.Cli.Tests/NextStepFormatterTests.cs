using System.Text.Json;

namespace OrchardCore.Cli.Tests;

public class NextStepFormatterTests
{
    [Theory]
    [InlineData("parent", false)]
    [InlineData("PARENT", false)]
    [InlineData("other", true)]
    [InlineData(null, true)]
    public void NextCommand_QualifiesContextOnlyWhenNotCurrent(string? currentContext, bool expectedQualifier)
    {
        foreach (var (path, json, command) in new (string[], string, string)[]
        {
            (["tenants", "create"], """{"name":"Demo","state":"Uninitialized"}""", "tenants setup Demo"),
            (["tenants", "install"], """{"name":"Demo","state":"Running"}""", "tenants enable-remote-management Demo"),
            (["tenants", "create"], """{"name":"Demo","state":"Disabled"}""", "tenants start Demo"),
            (["context", "add"], """{"name":"parent"}""", "login"),
            (["login"], """{"context":"parent"}""", "--help"),
        })
        {
            using var data = JsonDocument.Parse(json);
            var text = NextStepFormatter.Format(new CommandOutput
            {
                Json = data.RootElement, CommandPath = path, ContextName = "parent", CurrentContextName = currentContext,
                Operation = path[0] == "tenants" ? TenantOperations.Get(path[1]) : null,
            });
            Assert.Contains(expectedQualifier ? $"pomi --context=parent {command}" : $"pomi {command}", text);
            if (!expectedQualifier)
            {
                Assert.DoesNotContain("--context", text);
            }
        }
    }

    [Fact]
    public void CreatedTenant_SuggestsSetupInParentContextWithoutInlinePassword()
    {
        var text = Format(["tenants", "create"], """{"name":"Demo","state":"Uninitialized"}""");
        Assert.Contains("pomi --context=parent tenants setup Demo", text);
        Assert.Contains("--site-name Demo --user-name admin --email '<admin-email>'", text);
        Assert.Contains("--recipe-name '<recipe-name>' --database-provider Sqlite", text);
        Assert.DoesNotContain("--password", text);
    }

    [Fact]
    public void CreatedTenant_KeepsConfiguredRecipeAndDatabase()
    {
        var text = Format(["tenants", "create"], """{"name":"Demo","state":"Uninitialized","recipeName":"Custom","databaseProvider":"SqlConnection","connectionString":"do-not-repeat"}""");
        Assert.DoesNotContain("--recipe-name", text);
        Assert.DoesNotContain("--database-provider", text);
        Assert.DoesNotContain("do-not-repeat", text);
    }

    [Theory]
    [InlineData("create")]
    [InlineData("setup")]
    [InlineData("install")]
    [InlineData("start")]
    public void RunningTenant_SuggestsRemoteManagementInParentContext(string verb)
    {
        Assert.Contains("pomi --context=parent tenants enable-remote-management Demo",
            Format(["tenants", verb], """{"name":"Demo","state":"Running"}"""));
    }

    [Fact]
    public void EnabledTenant_SuggestsExactUrlAndAvoidsContextNameCollision()
    {
        using var data = JsonDocument.Parse("""{"name":"Demo","state":"Running","url":"https://cms.example.com/nested/demo/"}""");
        var text = NextStepFormatter.Format(new CommandOutput
        {
            Json = data.RootElement, CommandPath = ["tenants", "enable-remote-management"], ContextName = "parent",
            Operation = TenantOperations.Get("enable-remote-management"),
            KnownContexts = [new TenantContextRecord { Name = "demo", TenantUrl = "https://another.example.com/" }],
        });
        Assert.Contains("pomi context list --output json", text);
        Assert.Contains("pomi context add Demo-2 https://cms.example.com/nested/demo/ --current", text);
    }

    [Fact]
    public void EnabledTenant_UsesNewContextEvenWhenAnExistingContextHasTheSameUrl()
    {
        using var data = JsonDocument.Parse("""{"name":"Demo","state":"Running","url":"https://cms.example.com/demo"}""");
        var text = NextStepFormatter.Format(new CommandOutput
        {
            Json = data.RootElement, CommandPath = ["tenants", "enable-remote-management"],
            Operation = TenantOperations.Get("enable-remote-management"),
            KnownContexts = [new TenantContextRecord { Name = "Demo", TenantUrl = "https://cms.example.com/demo/" }],
        });
        Assert.Contains("pomi context add Demo-2 https://cms.example.com/demo --current", text);
    }

    [Fact]
    public void InstalledSite_UsesDirectoryNameAndSkipsAllOccupiedNames()
    {
        using var data = JsonDocument.Parse("""{"directory":"MySite/","url":"https://localhost:53127/"}""");
        var text = NextStepFormatter.Format(new CommandOutput
        {
            Json = data.RootElement, CommandPath = ["install"],
            KnownContexts = [
                new TenantContextRecord { Name = "mysite", TenantUrl = "https://localhost:53127/" },
                new TenantContextRecord { Name = "MYSITE-2", TenantUrl = "https://localhost:53128/" },
            ],
        });
        Assert.Contains("Once the site is running", text);
        Assert.Contains("pomi context list --output json", text);
        Assert.Contains("pomi context add MySite-3 https://localhost:53127/ --current", text);
        Assert.DoesNotContain("context add default", text);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"https://user:secret@example.com/\"")]
    [InlineData("\"https://example.com/?token=secret\"")]
    public void EnabledTenant_UnknownOrUnsuitableUrlUsesPlaceholder(string url)
    {
        var text = Format(["tenants", "enable-remote-management"], $$"""{"name":"Demo","state":"Running","url":{{url}}}""");
        Assert.Contains("pomi context add Demo '<tenant-url>' --current", text);
        Assert.DoesNotContain("secret", text);
    }

    [Fact]
    public void ContextAdded_LoginTargetsNewContextRatherThanParent()
    {
        Assert.Contains("pomi --context=Demo login", Format(["context", "add"], """{"name":"Demo"}"""));
        Assert.Contains("pomi --context=Demo --help", Format(["login"], """{"context":"Demo"}"""));
    }

    [Theory]
    [InlineData("Initializing")]
    [InlineData("Invalid")]
    public void IncompleteSetup_DoesNotSuggestRemoteManagement(string state)
    {
        Assert.Null(Format(["tenants", "setup"], $$"""{"name":"Demo","state":"{{state}}"}"""));
    }

    [Fact]
    public void ShellQuoting_KeepsNamesLiteral()
    {
        const string name = "O'Brien; $(touch marker)";
        Assert.Equal("'O'\"'\"'Brien; $(touch marker)'", NextStepFormatter.QuoteArgument(name, powerShell: false));
        Assert.Equal("'O''Brien; $(touch marker)'", NextStepFormatter.QuoteArgument(name, powerShell: true));
        Assert.Null(Format(["tenants", "create"], """{"name":"Demo\nmalicious","state":"Uninitialized"}"""));
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(202, false)]
    public void FailedOrPendingOperations_DoNotSuggestNextMutation(int status, bool failed)
    {
        using var data = JsonDocument.Parse($$"""{"name":"Demo","state":"Running","success":{{(!failed).ToString().ToLowerInvariant()}}}""");
        var text = HumanOutputFormatter.Format(new CommandOutput
        {
            Json = data.RootElement, CommandPath = ["tenants", "setup"], HttpMethod = "POST", StatusCode = status,
            Operation = TenantOperations.Get("setup"),
        });
        Assert.DoesNotContain("Next:", text);
    }

    [Fact]
    public void ProvisionedContext_OperationWithProvisioningMetadata_SuggestsExploringIt()
    {
        foreach (var verb in new[] { "install", "enable-remote-management" })
        {
            Assert.Contains("pomi --context=demo --help", Format(["tenants", verb], """{"name":"Demo","state":"Running","context":"demo"}"""));
        }

        Assert.DoesNotContain("--context=demo --help", Format(["tenants", "start"], """{"name":"Demo","state":"Running","context":"demo"}"""));
    }

    [Fact]
    public void DisabledRemoteManagement_SuggestsEnablingAgain()
    {
        var text = Format(["tenants", "disable-remote-management"], """{"name":"Demo","state":"Running"}""");

        Assert.Contains("contexts saved for this tenant stop working", text);
        Assert.Contains("pomi --context=parent tenants enable-remote-management Demo", text);
    }

    [Fact]
    public void ReloadedTenant_HasNoNextStep()
    {
        Assert.Null(Format(["tenants", "reload"], """{"name":"Demo","state":"Running"}"""));
    }

    [Fact]
    public void OperationWithoutMetadata_SuggestsNothing()
    {
        using var data = JsonDocument.Parse("""{"name":"Demo","state":"Running"}""");

        Assert.Null(NextStepFormatter.Format(new CommandOutput { Json = data.RootElement, CommandPath = ["tenants", "start"] }));
    }

    [Fact]
    public void NextStepMetadata_OtherResource_BuildsItsCommand()
    {
        var operation = ParseOperation("""
            {"commandGroup":["widgets"],"verb":"create","nextSteps":[
              {"description":"skipped.","whenProperty":"state","whenValue":"Other","arguments":["widgets","skipped"]},
              {"description":"publish this widget.","whenProperty":"state","whenValue":"Draft","arguments":["widgets","publish","{id}"],
               "optionalArguments":[{"whenMissing":"owner","arguments":["--owner","<owner>"]},{"whenMissing":"category","arguments":["--category","none"]}],"note":"Publishing is immediate."}]}
            """);
        using var data = JsonDocument.Parse("""{"id":"My Widget","state":"Draft","category":"tools"}""");

        var text = NextStepFormatter.Format(new CommandOutput { Json = data.RootElement, CommandPath = ["widgets", "create"], Operation = operation });

        Assert.Equal("Next: publish this widget.\n  pomi widgets publish 'My Widget' --owner '<owner>'\nPublishing is immediate.", text);
    }

    [Theory]
    [InlineData("""{"description":"bad\u001b[2J.","arguments":["widgets"]}""")]
    [InlineData("""{"description":"fine.","note":"bad\nrm -rf /","arguments":["widgets"]}""")]
    [InlineData("""{"description":"fine.","arguments":["widgets","bad\nargument"]}""")]
    [InlineData("""{"description":"fine.","arguments":["widgets","{dashed}"]}""")]
    [InlineData("""{"description":"fine.","arguments":["widgets","{control}"]}""")]
    [InlineData("""{"description":"fine.","arguments":["widgets","{missing}"]}""")]
    [InlineData("""{"arguments":["widgets"]}""")]
    public void NextStepMetadata_UnsafeOrIncomplete_SuggestsNothing(string step)
    {
        var operation = ParseOperation($$"""{"commandGroup":["widgets"],"verb":"create","nextSteps":[{{step}}]}""");
        using var data = JsonDocument.Parse("""{"dashed":"--force","control":"a\nb"}""");

        Assert.Null(NextStepFormatter.Format(new CommandOutput { Json = data.RootElement, CommandPath = ["widgets", "create"], Operation = operation }));
    }

    [Fact]
    public void NextStepMetadata_ShellMetacharactersInValue_QuotesThem()
    {
        var operation = ParseOperation("""{"commandGroup":["widgets"],"verb":"create","nextSteps":[{"description":"show it.","arguments":["widgets","show","{id}"]}]}""");
        using var data = JsonDocument.Parse("""{"id":"a; $(touch marker)"}""");

        var text = NextStepFormatter.Format(new CommandOutput { Json = data.RootElement, CommandPath = ["widgets", "create"], Operation = operation });

        Assert.Contains($"pomi widgets show {NextStepFormatter.QuoteArgument("a; $(touch marker)")}", text);
    }

    private static OrchardCore.RemoteManagement.CliOperationMetadata ParseOperation(string extension) =>
        Assert.Single(OpenApiCliParser.Parse("""{"paths":{"/api/widgets":{"post":{"x-oc-cli":""" + extension + "}}}}")).CliMetadata;

    private static string? Format(string[] path, string json)
    {
        using var data = JsonDocument.Parse(json);
        return NextStepFormatter.Format(new CommandOutput
        {
            Json = data.RootElement, CommandPath = path, ContextName = "parent",
            Operation = path[0] == "tenants" ? TenantOperations.Get(path[1]) : null,
        });
    }
}
