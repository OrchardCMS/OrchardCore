using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Layers.Endpoints.Management;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Layers.ViewModels;
using LayerAdminController = OrchardCore.Layers.Controllers.AdminController;
using OrchardCore.Rules.Services;
using OrchardCore.Tests.Apis.Context;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class LayerManagementApiTests
{
    [Fact]
    public async Task Definitions_RetryValidationAndDeletion_PersistAcrossTenantScopes()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var name = "RemoteLayer" + Guid.NewGuid().ToString("N");
        var definition = new LayerDefinitionDto
        {
            Name = name,
            Conditions = [new RuleConditionDefinition
            {
                Name = "UrlCondition", Properties = new JsonObject { ["value"] = "/news", ["operation"] = "StringStartsWithOperator" },
            }],
        };
        string conditionId = null;
        for (var retry = 0; retry < 2; retry++)
        {
            await context.UsingTenantScopeAsync(async scope =>
            {
                var result = await LayerManagementEndpoints.CreateAsync(new DefaultHttpContext(), Authorize(true),
                    scope.ServiceProvider.GetRequiredService<ILayerService>(), scope.ServiceProvider.GetRequiredService<IRuleManagementService>(), definition);
                var created = Assert.IsType<Created<LayerDefinitionDto>>(result).Value;
                if (conditionId is not null)
                {
                    Assert.Equal(conditionId, created.Conditions[0].ConditionId);
                }
                conditionId = created.Conditions[0].ConditionId;
            });
        }
        await context.UsingTenantScopeAsync(async scope =>
        {
            var layers = scope.ServiceProvider.GetRequiredService<ILayerService>();
            var rules = scope.ServiceProvider.GetRequiredService<IRuleManagementService>();
            var conflict = await LayerManagementEndpoints.CreateAsync(new DefaultHttpContext(), Authorize(true), layers, rules,
                new LayerDefinitionDto { Name = name, Description = "conflict" });
            Assert.Equal(409, Assert.IsAssignableFrom<IStatusCodeHttpResult>(conflict).StatusCode);
            var invalid = new LayerDefinitionDto { Name = name, Conditions = [new RuleConditionDefinition { Name = "NoSuchCondition" }] };
            var validation = await LayerManagementEndpoints.ValidateAsync(new DefaultHttpContext(), Authorize(true), rules, layers, invalid);
            Assert.False(Assert.IsType<Ok<RuleValidationResponse>>(validation).Value.IsValid);
            var update = await LayerManagementEndpoints.UpdateAsync(new DefaultHttpContext(), Authorize(true), layers, rules, name, invalid);
            Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(update).StatusCode);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var layers = scope.ServiceProvider.GetRequiredService<ILayerService>();
            var rules = scope.ServiceProvider.GetRequiredService<IRuleManagementService>();
            var existing = Assert.IsType<Ok<LayerDefinitionDto>>(await LayerManagementEndpoints.GetAsync(new DefaultHttpContext(), Authorize(true), layers, rules, name)).Value;
            Assert.Equal(conditionId, existing.Conditions[0].ConditionId);
            var updated = Assert.IsType<Ok<LayerDefinitionDto>>(await LayerManagementEndpoints.UpdateAsync(new DefaultHttpContext(), Authorize(true), layers, rules,
                name, new LayerDefinitionDto { Name = name, Description = "Updated", Conditions = existing.Conditions })).Value;
            Assert.Equal("Updated", updated.Description);
            Assert.Equal(conditionId, updated.Conditions[0].ConditionId);
        });
        for (var retry = 0; retry < 2; retry++)
        {
            await context.UsingTenantScopeAsync(async scope =>
                Assert.IsType<NoContent>(await LayerManagementEndpoints.DeleteAsync(new DefaultHttpContext(), Authorize(true),
                    scope.ServiceProvider.GetRequiredService<ILayerService>(), name)));
        }
    }

    [Fact]
    public async Task DeniedOperations_DoNotReadOrMutateDocuments()
    {
        var layers = new Mock<ILayerService>(MockBehavior.Strict).Object;
        var rules = new Mock<IRuleManagementService>(MockBehavior.Strict).Object;
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var auth = Authorize(false);
        IResult[] results =
        [
            await LayerManagementEndpoints.ListAsync(http, auth, layers, rules, new()),
            await LayerManagementEndpoints.GetAsync(http, auth, layers, rules, "name"),
            await LayerManagementEndpoints.ConditionsAsync(http, auth, rules),
            await LayerManagementEndpoints.ValidateAsync(http, auth, rules, layers, new()),
            await LayerManagementEndpoints.CreateAsync(http, auth, layers, rules, new()),
            await LayerManagementEndpoints.UpdateAsync(http, auth, layers, rules, "name", new()),
            await LayerManagementEndpoints.DeleteAsync(http, auth, layers, "name"),
        ];
        Assert.All(results, result => Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode));
    }

    [Fact]
    public async Task ReferencedLayer_CannotBeDeleted()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            await scope.ServiceProvider.GetRequiredService<ILayerService>().CreateAsync("Referenced", "Test layer");
            // Load definitions before the session flush invokes content index providers.
            await scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>().GetTypeDefinitionAsync("LayerReferenceProbe");
            var item = new ContentItem { ContentItemId = Guid.NewGuid().ToString("N"), ContentType = "LayerReferenceProbe", Latest = true, Published = true };
            item.Weld(new LayerMetadata { Layer = "referenced", Zone = "Content" });
            await scope.ServiceProvider.GetRequiredService<ISession>().SaveAsync(item);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var layers = scope.ServiceProvider.GetRequiredService<ILayerService>();
            var result = await LayerManagementEndpoints.DeleteAsync(new DefaultHttpContext(), Authorize(true), layers, "Referenced");
            Assert.Equal(409, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
            Assert.NotNull(await layers.GetLayerAsync("Referenced"));
        });
    }

    [Fact]
    public async Task MetadataEdits_UseTheSharedServiceWithoutReplacingTheRule()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        string ruleId = null;
        string conditionId = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var rules = scope.ServiceProvider.GetRequiredService<IRuleManagementService>();
            var rule = rules.CreateRule([new RuleConditionDefinition { Name = "HomepageCondition" }]).Rule;
            var result = await scope.ServiceProvider.GetRequiredService<ILayerService>().CreateAsync("SharedEdit", "Original", rule);
            Assert.Equal(LayerMutationStatus.Success, result.Status);
            ruleId = result.Layer.LayerRule.ConditionId;
            conditionId = result.Layer.LayerRule.Conditions[0].ConditionId;
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<ILayerService>();
            var controller = ActivatorUtilities.CreateInstance<LayerAdminController>(scope.ServiceProvider, Authorize(true));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider },
            };
            controller.Url = Mock.Of<IUrlHelper>();
            Assert.IsType<RedirectToActionResult>(await controller.EditPost(new LayerEditViewModel { Name = "SharedEdit", Description = "Edited by admin" }));
            Assert.IsType<ViewResult>(await controller.CreatePost(new LayerEditViewModel { Name = "sharededit", Description = "Duplicate" }));
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal(LayerMutationStatus.Conflict, (await service.CreateAsync("sharededit", "Duplicate")).Status);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var layer = await scope.ServiceProvider.GetRequiredService<ILayerService>().GetLayerAsync("SharedEdit");
            Assert.Equal("Edited by admin", layer.Description);
            Assert.Equal(ruleId, layer.LayerRule.ConditionId);
            Assert.Equal(conditionId, Assert.Single(layer.LayerRule.Conditions).ConditionId);
        });
    }

    private static IAuthorizationService Authorize(bool success)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
            It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(success ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }
}
