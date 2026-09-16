using System.Linq.Expressions;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Localization;
using OrchardCore.Queries;
using OrchardCore.Queries.Deployment;
using OrchardCore.Queries.ViewModels;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class QueryDeploymentConfigurationTests
{
    [Theory]
    [InlineData("content", null, true)]
    [InlineData("content", "{}", true)]
    [InlineData("content", "{\"limit\":5}", true)]
    [InlineData("content", "null", false)]
    [InlineData("content", "[]", false)]
    [InlineData("content", "{", false)]
    [InlineData("rows", "{}", false)]
    [InlineData("missing", "{}", false)]
    [InlineData("", "{}", false)]
    public async Task AdminAndApi_ShareQueryAndParameterValidation(string name, string parameters, bool valid)
    {
        var queries = Queries();
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(value => value.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<QueryBasedContentDeploymentStepViewModel>(), It.IsAny<string>(), It.IsAny<Expression<Func<QueryBasedContentDeploymentStepViewModel, object>>[]>()))
            .Callback((QueryBasedContentDeploymentStepViewModel model, string _, Expression<Func<QueryBasedContentDeploymentStepViewModel, object>>[] _) =>
            {
                model.QueryName = name;
                model.QueryParameters = parameters;
                model.ExportAsSetupRecipe = true;
            }).ReturnsAsync(true);
        var admin = new QueryBasedContentDeploymentStep();
        var driver = new QueryBasedContentDeploymentStepDriver(queries.Object,
            new StringLocalizer<QueryBasedContentDeploymentStepDriver>(new NullStringLocalizerFactory()));
        await driver.UpdateAsync(admin, new UpdateEditorContext(new Shape(), "", false, "", Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object));
        Assert.Equal(valid, updater.Object.ModelState.IsValid);

        var step = new QueryBasedContentDeploymentStep { QueryName = "content", QueryParameters = "{}" };
        var definition = new QueryBasedContentStepDefinition(queries.Object);
        var before = definition.Describe(step);
        var errors = await definition.UpdateAsync(step, new JsonObject
        {
            ["queryName"] = name,
            ["queryParameters"] = parameters,
            ["exportAsSetupRecipe"] = true,
        });
        Assert.Equal(valid, errors.Count == 0);
        if (valid)
        {
            Assert.True(JsonNode.DeepEquals(definition.Describe(admin), definition.Describe(step)));
        }
        else
        {
            Assert.True(JsonNode.DeepEquals(before, definition.Describe(step)));
        }
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("null", true)]
    [InlineData("{}", true)]
    [InlineData("[]", false)]
    [InlineData("{", false)]
    public async Task ExistingSource_PreservesNullObjectAsEmptyAndSkipsMalformedParameters(string parameters, bool executes)
    {
        var queries = Queries();
        var results = new Mock<IQueryResults>();
        results.SetupGet(value => value.Items).Returns([]);
        queries.Setup(value => value.ExecuteQueryAsync(It.IsAny<Query>(), It.IsAny<IDictionary<string, object>>()))
            .ReturnsAsync(results.Object);
        var step = new QueryBasedContentDeploymentStep { QueryName = "content", QueryParameters = parameters };
        await new QueryBasedContentDeploymentSource(queries.Object).ProcessDeploymentStepAsync(step,
            new DeploymentPlanResult(Mock.Of<IFileBuilder>(), new RecipeDescriptor()));
        queries.Verify(value => value.ExecuteQueryAsync(It.IsAny<Query>(), It.Is<IDictionary<string, object>>(values => values.Count == 0)),
            executes ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task RegisteredContract_PreservesOmissionsAndRejectsInvalidShapes()
    {
        var services = new ServiceCollection();
        new global::OrchardCore.Queries.DeploymentStartup().ConfigureServices(services);
        services.AddSingleton(Queries().Object);
        using var provider = services.BuildServiceProvider();
        var definition = Assert.Single(provider.GetServices<IDeploymentStepDefinition>(), value => value.Type == nameof(QueryBasedContentDeploymentStep));
        var factory = Assert.Single(provider.GetServices<IDeploymentStepFactory>(), value => value.Name == definition.Type);
        var step = factory.Create();
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject { ["queryName"] = "content", ["queryParameters"] = "{}" }));
        var before = definition.Describe(step);
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject()));
        Assert.True(JsonNode.DeepEquals(before, definition.Describe(step)));
        foreach (var patch in new JsonObject[]
        {
            new() { ["queryName"] = null },
            new() { ["queryParameters"] = new JsonObject() },
            new() { ["exportAsSetupRecipe"] = "true" },
            new() { ["exportAsSetupRecipe"] = true, ["unknown"] = false },
        })
        {
            Assert.NotEmpty(await definition.UpdateAsync(step, patch));
            Assert.True(JsonNode.DeepEquals(before, definition.Describe(step)));
        }
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject { ["queryParameters"] = null }));
        Assert.Null(definition.Describe(step)["queryParameters"]);
    }

    private static Mock<IQueryManager> Queries()
    {
        var queries = new Mock<IQueryManager>();
        queries.Setup(value => value.GetQueryAsync("content")).ReturnsAsync(new Query { Name = "content", ReturnContentItems = true });
        queries.Setup(value => value.GetQueryAsync("rows")).ReturnsAsync(new Query { Name = "rows", ReturnContentItems = false });
        return queries;
    }
}
