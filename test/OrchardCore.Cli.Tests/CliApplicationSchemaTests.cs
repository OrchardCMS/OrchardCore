using System.Text.Json.Nodes;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli.Tests;

public class CliApplicationSchemaTests
{
    [Fact]
    public void SelectSchemaOperation_IdenticalSchemasWithoutSelection_ReturnsFirstOperation()
    {
        var create = CreateOperation("create", "string");
        var update = CreateOperation("update", "string");

        var selected = CliApplication.SelectSchemaOperation([create, update], null);

        Assert.Same(create, selected);
    }

    [Fact]
    public void SelectSchemaOperation_DifferentSchemasWithoutSelection_RequiresOperation()
    {
        var operations = new[]
        {
            CreateOperation("create", "string"),
            CreateOperation("update", "integer"),
        };

        var exception = Assert.Throws<CliException>(() => CliApplication.SelectSchemaOperation(operations, null));

        Assert.Contains("--operation", exception.Message);
        Assert.Contains("create, update", exception.Message);
    }

    [Fact]
    public void SelectSchemaOperation_ExplicitAlias_ReturnsMatchingOperation()
    {
        var create = CreateOperation("create", "string");
        create.CliMetadata.Aliases.Add("add");

        var selected = CliApplication.SelectSchemaOperation([create], "ADD");

        Assert.Same(create, selected);
    }

    [Fact]
    public void CreateRequiredBodyMessage_Operation_IncludesExactSchemaCommand()
    {
        var operation = CreateOperation("create", "string");

        var message = CliApplication.CreateRequiredBodyMessage(operation, includeInputOptions: true);

        Assert.Equal(
            "A JSON request body is required. Provide --body, --body-file, or --stdin. Run 'pomi widgets schema --operation create' to inspect its schema.",
            message);
    }

    [Fact]
    public void ValidateDynamicJsonBody_SetupPasswordMetadata_ValidatesThatProperty()
    {
        var operation = CreateOperation("provision", "string");
        operation.CliMetadata.SetupPasswordProperty = "administratorPassword";

        Assert.Contains("password", Assert.Throws<CliException>(() => CliApplication.ValidateDynamicJsonBody("""{"password":"Str0ng-Password!"}""", operation)).Message);
        Assert.Throws<CliException>(() => CliApplication.ValidateDynamicJsonBody("""{"administratorPassword":"weak"}""", operation));
        CliApplication.ValidateDynamicJsonBody("""{"administratorPassword":"Str0ng-Password!"}""", operation);
    }

    [Theory]
    [InlineData("install")]
    [InlineData("setup")]
    public void ValidateDynamicJsonBody_ServerWithoutMetadata_ValidatesTenantSetupPassword(string verb)
    {
        var operation = new OpenApiOperationDefinition { Method = "POST", CliMetadata = new CliOperationMetadata(["tenants"], verb) };

        Assert.Throws<CliException>(() => CliApplication.ValidateDynamicJsonBody("""{"password":"weak"}""", operation));
        CliApplication.ValidateDynamicJsonBody("""{"password":"Str0ng-Password!"}""", operation);
    }

    [Fact]
    public void ValidateDynamicJsonBody_OtherOperationWithoutMetadata_LeavesPasswordToTheServer()
    {
        var operation = new OpenApiOperationDefinition { Method = "POST", CliMetadata = new CliOperationMetadata(["users"], "create") };

        CliApplication.ValidateDynamicJsonBody("""{"password":"weak"}""", operation);
    }

    private static OpenApiOperationDefinition CreateOperation(string verb, string valueType) =>
        new()
        {
            CliMetadata = new CliOperationMetadata(["widgets"], verb),
            RequestBodySchema = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["value"] = new JsonObject
                    {
                        ["type"] = valueType,
                    },
                },
            },
        };
}
