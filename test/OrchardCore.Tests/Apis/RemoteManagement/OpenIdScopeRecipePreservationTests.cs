using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;
using OrchardCore.OpenId;
using OrchardCore.OpenId.Controllers;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Recipes;
using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class OpenIdScopeRecipePreservationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ScopeEditors_PreserveExtensionPropertiesAndRespectResourceOmission(bool useRecipe)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await features.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.OpenId.Management");
            await features.EnableFeaturesAsync([feature], force: true);
        });
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIdScopeManager>();
            var descriptor = new OpenIdScopeDescriptor { Name = "preserved-scope", DisplayName = "Original" };
            descriptor.Resources.Add("existing-api");
            descriptor.Properties.Add("extension", JsonSerializer.SerializeToElement("preserve-me"));
            await manager.CreateAsync(descriptor);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIdScopeManager>();
            if (useRecipe)
            {
                var step = new OpenIdScopeStep(manager);
                await step.ExecuteAsync(new RecipeExecutionContext
                {
                    Name = "OpenIdScope",
                    Step = new JsonObject { ["ScopeName"] = "preserved-scope", ["DisplayName"] = "Updated" },
                });
            }
            else
            {
                var authorization = new Mock<IAuthorizationService>();
                authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success());
                var services = scope.ServiceProvider;
                var shellSettings = services.GetRequiredService<ShellSettings>();
                var controller = new ScopeController(manager, services.GetRequiredService<IShapeFactory>(),
                    services.GetRequiredService<IOptions<PagerOptions>>(), services.GetRequiredService<IStringLocalizer<ScopeController>>(),
                    authorization.Object, shellSettings)
                {
                    Url = Mock.Of<IUrlHelper>(),
                    ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services } },
                };
                var item = await manager.FindByNameAsync("preserved-scope");
                var model = new EditOpenIdScopeViewModel
                {
                    Id = await manager.GetPhysicalIdAsync(item), Name = "preserved-scope", DisplayName = "Updated", Resources = "",
                };
                Assert.IsType<RedirectToActionResult>(await controller.Edit(model));
                model.DisplayName = "Rejected";
                model.Resources = OpenIdConstants.Prefixes.Tenant + shellSettings.Name;
                Assert.IsType<ViewResult>(await controller.Edit(model));
                Assert.False(controller.ModelState.IsValid);
                controller.ModelState.Clear();
                model.Name = "invalid scope";
                model.Resources = "rejected-resource";
                await Assert.ThrowsAsync<OpenIddictExceptions.ValidationException>(() => controller.Edit(model));
            }
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIdScopeManager>();
            var item = await manager.FindByNameAsync("preserved-scope");
            Assert.NotNull(item);
            Assert.Equal("Updated", await manager.GetDisplayNameAsync(item));
            Assert.Equal(useRecipe ? ["existing-api"] : [], await manager.GetResourcesAsync(item));
            Assert.Equal("preserve-me", (await manager.GetPropertiesAsync(item))["extension"].GetString());
        });
    }
}
