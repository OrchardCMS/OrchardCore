using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Controllers;
using OrchardCore.Recipes.Endpoints.Management;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Recipes;

public class RecipeManagementEndpointsTests
{
    [Fact]
    public void GetParameters_MergesRecipeVariablesAndEnvironmentUsingStableRecipeId()
    {
        var recipe = new RecipeDescriptor
        {
            BasePath = "Recipes/Module",
            RecipeFileInfo = new TestFileInfo("sample.recipe.json", """
                {
                  "variables": {
                    "Color": "Blue",
                    "Region": "US"
                  }
                }
                """),
        };

        var recipeId = RecipeManagementEndpoints.GetRecipeId(recipe);
        var parameters = RecipeManagementEndpoints.GetParameters(recipe, new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["Region"] = "EU",
            ["Tenant"] = "Default",
        });

        Assert.Equal("UmVjaXBlcy9Nb2R1bGV8c2FtcGxlLnJlY2lwZS5qc29u", recipeId);

        Assert.Collection(parameters,
            parameter =>
            {
                Assert.Equal("Color", parameter.Name);
                Assert.Equal("recipe", parameter.Source);
                Assert.Equal("Blue", parameter.DefaultValue);
                Assert.Null(parameter.CurrentValue);
            },
            parameter =>
            {
                Assert.Equal("Region", parameter.Name);
                Assert.Equal("recipe+environment", parameter.Source);
                Assert.Equal("US", parameter.DefaultValue);
                Assert.Equal("EU", parameter.CurrentValue);
            },
            parameter =>
            {
                Assert.Equal("Tenant", parameter.Name);
                Assert.Equal("environment", parameter.Source);
                Assert.Null(parameter.DefaultValue);
                Assert.Equal("Default", parameter.CurrentValue);
            });
    }

    [Fact]
    public async Task List_CalledTwice_HarvestsRecipesOnce()
    {
        var scenario = new Scenario();

        for (var i = 0; i < 2; i++)
        {
            var result = await RecipeManagementEndpoints.ListAsync(scenario.Context, scenario.Authorization, scenario.Features.Object, [scenario.Harvester.Object],
                scenario.Cache, [], NullLogger<AdminController>.Instance, new RecipeManagementEndpoints.RecipeListRequest());

            var response = Assert.IsType<Ok<RecipeManagementEndpoints.RecipeListResponse>>(result).Value;
            Assert.Equal(1, response.TotalCount);
        }

        scenario.Harvester.Verify(value => value.HarvestRecipesAsync(), Times.Once);
    }

    [Fact]
    public async Task Validate_RecipeFound_ReturnsTheResultWithoutExecuting()
    {
        var scenario = new Scenario();
        var expected = new RecipeValidationResult { Valid = true };
        var validator = new Mock<IRecipeValidator>();
        validator.Setup(value => value.ValidateAsync(scenario.Recipe, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await RecipeManagementEndpoints.ValidateAsync(scenario.Context, scenario.RecipeId, scenario.Authorization, scenario.Features.Object,
            [scenario.Harvester.Object], scenario.Cache, validator.Object, scenario.Localizer);

        Assert.Same(expected, Assert.IsType<Ok<RecipeValidationResult>>(result).Value);
        scenario.Executor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Validate_WithoutManageRecipes_IsForbiddenBeforeLookingAtTheRecipe()
    {
        var scenario = new Scenario(authorized: false);
        var validator = new Mock<IRecipeValidator>(MockBehavior.Strict);

        var result = await RecipeManagementEndpoints.ValidateAsync(scenario.Context, "any", scenario.Authorization, scenario.Features.Object,
            [scenario.Harvester.Object], scenario.Cache, validator.Object, scenario.Localizer);

        Assert.Equal(StatusCodes.Status403Forbidden, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        scenario.Harvester.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Validate_RecipeThatCantBeExecuted_IsNotFound()
    {
        var scenario = new Scenario();
        scenario.Recipe.IsSetupRecipe = true;
        var validator = new Mock<IRecipeValidator>(MockBehavior.Strict);

        var result = await RecipeManagementEndpoints.ValidateAsync(scenario.Context, scenario.RecipeId, scenario.Authorization, scenario.Features.Object,
            [scenario.Harvester.Object], scenario.Cache, validator.Object, scenario.Localizer);

        Assert.Equal(StatusCodes.Status404NotFound, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }

    [Fact]
    public async Task Execute_WithRequestId_UsesItAsTheExecutionIdAndRecordsTheOutcome()
    {
        var scenario = new Scenario();

        var result = await scenario.ExecuteAsync("request-1");

        Assert.Equal("request-1", Assert.IsType<Ok<RecipeManagementEndpoints.RecipeExecutionResponse>>(result).Value.ExecutionId);
        Assert.Equal(RecipeExecutionState.Succeeded, scenario.Executions.Executions["request-1"].State);
        scenario.Executor.Verify(value => value.ExecuteAsync("request-1", It.IsAny<RecipeDescriptor>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_WithoutRequestId_GeneratesAnExecutionId()
    {
        var scenario = new Scenario();

        var result = await scenario.ExecuteAsync(null);

        var executionId = Assert.IsType<Ok<RecipeManagementEndpoints.RecipeExecutionResponse>>(result).Value.ExecutionId;
        Assert.Equal(32, executionId.Length);
        Assert.Equal(RecipeExecutionState.Succeeded, scenario.Executions.Executions[executionId].State);
    }

    [Fact]
    public async Task Execute_RequestRepeatedAfterSuccess_ReportsItWithoutExecutingAgain()
    {
        var scenario = new Scenario();

        await scenario.ExecuteAsync("request-1");
        var result = await scenario.ExecuteAsync("request-1");

        Assert.Equal("request-1", Assert.IsType<Ok<RecipeManagementEndpoints.RecipeExecutionResponse>>(result).Value.ExecutionId);
        scenario.Executor.Verify(value => value.ExecuteAsync(It.IsAny<string>(), It.IsAny<RecipeDescriptor>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_StepFails_RecordsTheErrorsAndRepeatingReportsThem()
    {
        var scenario = new Scenario();
        scenario.Executor.Setup(value => value.ExecuteAsync(It.IsAny<string>(), It.IsAny<RecipeDescriptor>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RecipeExecutionException(new RecipeStepResult { StepName = "step", Errors = ["Step failed."], IsCompleted = true }));

        foreach (var attempt in new[] { 1, 2 })
        {
            var problem = Assert.IsType<ProblemHttpResult>(await scenario.ExecuteAsync("request-1")).ProblemDetails;

            Assert.Equal(400, problem.Status);
            Assert.Equal("Step failed.", problem.Detail);
            Assert.Equal("request-1", problem.Extensions["executionId"]);
        }

        Assert.Equal(RecipeExecutionState.Failed, scenario.Executions.Executions["request-1"].State);
        Assert.Equal(["Step failed."], scenario.Executions.Executions["request-1"].Errors);
        scenario.Executor.Verify(value => value.ExecuteAsync(It.IsAny<string>(), It.IsAny<RecipeDescriptor>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
        scenario.Shells.Verify(value => value.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Execute_RequestIdHasNoOutcomeOrOtherRecipe_ReturnsConflictWithoutExecuting(bool sameRecipe)
    {
        var scenario = new Scenario();
        await scenario.Executions.TryStartAsync("request-1", sameRecipe ? scenario.RecipeId : "other", "Recipe");

        var problem = Assert.IsType<ProblemHttpResult>(await scenario.ExecuteAsync("request-1")).ProblemDetails;

        Assert.Equal(409, problem.Status);
        scenario.Executor.Verify(value => value.ExecuteAsync(It.IsAny<string>(), It.IsAny<RecipeDescriptor>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a/b")]
    [InlineData("a b")]
    public async Task Execute_InvalidRequestId_ReturnsBadRequest(string requestId)
    {
        var scenario = new Scenario();

        var result = await scenario.ExecuteAsync(requestId);

        Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Empty(scenario.Executions.Executions);
    }

    [Fact]
    public async Task GetExecution_KnownExecution_ReturnsItsState()
    {
        var scenario = new Scenario();
        await scenario.ExecuteAsync("request-1");

        var result = await RecipeManagementEndpoints.GetExecutionAsync(scenario.Context, "request-1", scenario.Authorization, scenario.Executions, scenario.Localizer);

        var status = Assert.IsType<Ok<RecipeManagementEndpoints.RecipeExecutionStatusResponse>>(result).Value;
        Assert.Equal("succeeded", status.State);
        Assert.Equal(scenario.RecipeId, status.RecipeId);
    }

    [Fact]
    public async Task GetExecution_UnknownExecution_ReturnsNotFound()
    {
        var scenario = new Scenario();

        var result = await RecipeManagementEndpoints.GetExecutionAsync(scenario.Context, "unknown", scenario.Authorization, scenario.Executions, scenario.Localizer);

        Assert.Equal(404, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }

    [Fact]
    public async Task GetExecution_NoManageRecipesPermission_ReturnsForbidden()
    {
        var scenario = new Scenario(authorized: false);

        var result = await RecipeManagementEndpoints.GetExecutionAsync(scenario.Context, "request-1", scenario.Authorization, scenario.Executions, scenario.Localizer);

        Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }

    private sealed class Scenario
    {
        public Mock<IRecipeHarvester> Harvester { get; } = new();
        public Mock<IShellFeaturesManager> Features { get; } = new();
        public Mock<IRecipeExecutor> Executor { get; } = new();
        public Mock<IShellHost> Shells { get; } = new();
        public FakeExecutionStore Executions { get; } = new();
        public RecipeDescriptorCache Cache { get; } = new();
        public IAuthorizationService Authorization { get; }
        public IStringLocalizer<AdminController> Localizer { get; }
        public HttpContext Context { get; }
        public RecipeDescriptor Recipe { get; } = new()
        {
            Name = "Recipe",
            DisplayName = "Recipe",
            BasePath = "Modules/Test/Recipes",
            RecipeFileInfo = new TestFileInfo("sample.recipe.json", "{}"),
        };

        public string RecipeId => RecipeManagementEndpoints.GetRecipeId(Recipe);

        public Scenario(bool authorized = true)
        {
            Harvester.Setup(value => value.HarvestRecipesAsync()).ReturnsAsync([Recipe]);

            var extension = new Mock<IExtensionInfo>();
            extension.SetupGet(value => value.SubPath).Returns("Modules/Test");
            var feature = new Mock<IFeatureInfo>();
            feature.SetupGet(value => value.Id).Returns("Test");
            feature.SetupGet(value => value.Extension).Returns(extension.Object);
            Features.Setup(value => value.GetAvailableFeaturesAsync()).ReturnsAsync([feature.Object]);

            var authorization = new Mock<IAuthorizationService>();
            authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());
            Authorization = authorization.Object;

            var localizer = new Mock<IStringLocalizer<AdminController>>();
            localizer.Setup(value => value[It.IsAny<string>()]).Returns((string name) => new LocalizedString(name, name));
            localizer.Setup(value => value[It.IsAny<string>(), It.IsAny<object[]>()]).Returns((string name, object[] _) => new LocalizedString(name, name));
            Localizer = localizer.Object;

            Context = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider(),
            };
        }

        public Task<IResult> ExecuteAsync(string requestId)
            => RecipeManagementEndpoints.ExecuteAsync(Context, RecipeId, new RecipeManagementEndpoints.RecipeExecuteRequest { RequestId = requestId },
                Authorization, Shells.Object, new ShellSettings { Name = "Default" }, Features.Object, [Harvester.Object], Cache, Executions, Executor.Object, [],
                NullLogger<AdminController>.Instance, Localizer);
    }

    private sealed class FakeExecutionStore : IRecipeExecutionStore
    {
        public Dictionary<string, RecipeExecution> Executions { get; } = [];

        public Task<(bool Started, RecipeExecution Execution)> TryStartAsync(string executionId, string recipeId, string recipeName)
        {
            if (Executions.TryGetValue(executionId, out var existing))
            {
                return Task.FromResult((false, existing));
            }

            var started = new RecipeExecution { ExecutionId = executionId, RecipeId = recipeId, RecipeName = recipeName, State = RecipeExecutionState.Running };
            Executions[executionId] = started;
            return Task.FromResult((true, started));
        }

        public Task<RecipeExecution> FindAsync(string executionId) => Task.FromResult(Executions.GetValueOrDefault(executionId));

        public Task CompleteAsync(string executionId, RecipeExecutionState state, string[] errors = null)
        {
            Executions[executionId] = Executions[executionId] with { State = state, Errors = errors ?? [] };
            return Task.CompletedTask;
        }
    }

    private sealed class TestFileInfo(string name, string content) : IFileInfo
    {
        private readonly byte[] _content = Encoding.UTF8.GetBytes(content);

        public bool Exists => true;
        public long Length => _content.Length;
        public string PhysicalPath => null;
        public string Name { get; } = name;
        public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
        public bool IsDirectory => false;

        public Stream CreateReadStream() => new MemoryStream(_content, writable: false);
    }
}
