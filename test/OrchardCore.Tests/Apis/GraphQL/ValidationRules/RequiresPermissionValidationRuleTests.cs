using System.Text.Json.Nodes;
using GraphQL;
using GraphQL.Conversion;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using GraphQL.Validation;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.ValidationRules;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.GraphQL.ValidationRules;

public class RequiresPermissionValidationRuleTests
{
    internal static readonly Dictionary<string, Permission> _permissions = new()
    {
        { "permissionOne",  new Permission("TestPermissionOne", "TestPermissionOne") },
        { "permissionTwo",  new Permission("TestPermissionTwo", "TestPermissionTwo") }
    };

    [Fact]
    public async Task FieldsWithNoRequirePermissionsShouldResolve()
    {
        var options = BuildExecutionOptions("query { test { noPermissions } }",
                        new PermissionsContext
                        {
                            UsePermissionsContext = true
                        });

        var executer = new DocumentExecuter();

        var executionResult = await executer.ExecuteAsync(options);

        Assert.Null(executionResult.Errors);

        var writer = new GraphQLSerializer();
        var result = JsonObject.Parse(writer.Serialize(executionResult));

        Assert.Equal("Fantastic Fox Hates Permissions", result["data"]["test"]["noPermissions"].ToString());
    }

    [Theory]
    [InlineData("permissionOne", "Fantastic Fox Loves Permission One")]
    [InlineData("permissionTwo", "Fantastic Fox Loves Permission Two")]
    public async Task FieldsWithRequirePermissionsShouldResolveWhenUserHasPermissions(string fieldName, string expectedFieldValue)
    {
        var options = BuildExecutionOptions($"query {{ test {{{fieldName}}} }}",
                        new PermissionsContext
                        {
                            UsePermissionsContext = true,
                            AuthorizedPermissions = new[] { _permissions[fieldName] }
                        });

        var executer = new DocumentExecuter();

        var executionResult = await executer.ExecuteAsync(options);

        Assert.Null(executionResult.Errors);

        var writer = new GraphQLSerializer();
        var result = JObject.Parse(writer.Serialize(executionResult));

        Assert.Equal(expectedFieldValue, result["data"]["test"][fieldName].ToString());
    }

    [Fact]
    public async Task FieldsWithRequirePermissionsShouldNotResolveWhenUserDoesntHavePermissions()
    {
        var options = BuildExecutionOptions("query { test { permissionOne } }",
            new PermissionsContext
            {
                UsePermissionsContext = true
            });

        var executer = new DocumentExecuter();

        var executionResult = await executer.ExecuteAsync(options);

        Assert.NotEmpty(executionResult.Errors);
    }

    [Fact]
    public async Task FieldsWithMultipleRequirePermissionsShouldResolveWhenUserHasAllPermissions()
    {
        var options = BuildExecutionOptions("query { test { permissionMultiple  } }",
                        new PermissionsContext
                        {
                            UsePermissionsContext = true,
                            AuthorizedPermissions = _permissions.Values
                        });

        var executer = new DocumentExecuter();

        var executionResult = await executer.ExecuteAsync(options);

        Assert.Null(executionResult.Errors);

        var writer = new GraphQLSerializer();
        var result = JObject.Parse(writer.Serialize(executionResult));

        Assert.Equal("Fantastic Fox Loves Multiple Permissions", result["data"]["test"]["permissionMultiple"].ToString());
    }

    private static ExecutionOptions BuildExecutionOptions(string query, PermissionsContext permissionsContext)
    {
        var services = new ServiceCollection();
        services.AddAuthorization();
        services.AddLogging();
        services.AddOptions();
        services.AddLocalization();

        services.AddScoped<IAuthorizationHandler, PermissionContextAuthorizationHandler>(x =>
        {
            return new PermissionContextAuthorizationHandler(permissionsContext);
        });

        services.AddScoped<IValidationRule, RequiresPermissionValidationRule>();
        services.AddLocalization();
        var serviceProvider = services.BuildServiceProvider();

        return new ExecutionOptions
        {
            Query = query,
            Schema = new ValidationSchema(),
            UserContext = new GraphQLUserContext
            {
                User = new ClaimsPrincipal(new StubIdentity())
            },
            ValidationRules = DocumentValidator.CoreRules.Concat(serviceProvider.GetServices<IValidationRule>())
        };
    }

    private sealed class ValidationSchema : Schema
    {
        public ValidationSchema()
        {
            RegisterType(typeof(TestField));
            Query = new ValidationQueryRoot { Name = "Query" };
            NameConverter = new CamelCaseNameConverter();
        }
    }

    private sealed class ValidationQueryRoot : ObjectGraphType
    {
        public ValidationQueryRoot()
        {
            Field<TestField>("test")
                .Returns<object>()
                .Resolve(_ => new object());
        }
    }

    private sealed class TestField : ObjectGraphType
    {
        public TestField()
        {
            Field<StringGraphType>("NoPermissions")
                 .Returns<string>()
                 .Resolve(_ => "Fantastic Fox Hates Permissions");

            Field<StringGraphType>("PermissionOne")
                .Returns<string>()
                .RequirePermission(_permissions["permissionOne"])
                .Resolve(_ => "Fantastic Fox Loves Permission One");

            Field<StringGraphType>("PermissionTwo")
                 .Returns<string>()
                 .RequirePermission(_permissions["permissionTwo"])
                 .Resolve(_ => "Fantastic Fox Loves Permission Two");

            Field<StringGraphType>("PermissionMultiple")
                 .Returns<string>()
                 .RequirePermission(_permissions["permissionOne"])
                 .RequirePermission(_permissions["permissionTwo"])
                 .Resolve(_ => "Fantastic Fox Loves Multiple Permissions");
        }
    }
}
