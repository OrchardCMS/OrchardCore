using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Navigation;
using OrchardCore.Recipes.Models;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.Models;
using OrchardCore.Tenants.Recipes;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Tenants;

public class FeatureProfilesExistingPathsTests
{
    [Fact]
    public async Task AdminEdit_InvalidRules_PreservesExistingDefinition()
    {
        var document = new FeatureProfilesDocument();
        var original = new FeatureProfile { Id = "profile", Name = "Original" };
        document.FeatureProfiles["profile"] = original;
        var manager = FeatureProfilesManagerTests.CreateManager(document, out _);
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        var controller = new FeatureProfilesController(authorization.Object, manager, null, Options.Create(new PagerOptions()), null,
            Mock.Of<IStringLocalizer<FeatureProfilesController>>(), Mock.Of<IHtmlLocalizer<FeatureProfilesController>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        var result = await controller.EditPost(new FeatureProfileViewModel
        {
            Id = "profile", Name = "Rejected", FeatureRules = "null",
        }, "Save");
        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Same(original, document.FeatureProfiles["profile"]);
        Assert.Contains("FeatureRules", controller.ModelState.Keys);
    }

    [Fact]
    public async Task Recipe_LegacyDefinitionAndInvalidReplacement_UseSharedValidation()
    {
        var document = new FeatureProfilesDocument();
        var manager = FeatureProfilesManagerTests.CreateManager(document, out _);
        var step = new FeatureProfilesStep(manager);
        var valid = JsonNode.Parse("""
            {"name":"FeatureProfiles","FeatureProfiles":{"legacy":{"FeatureRules":[{"Rule":"Exclude","Expression":"Custom.*"}]}}}
            """);
        await step.ExecuteAsync(new RecipeExecutionContext { Name = "FeatureProfiles", Step = (JsonObject)valid });
        var original = document.FeatureProfiles["legacy"];
        Assert.Null(original.Id);
        Assert.Null(original.Name);
        Assert.Equal("Custom.*", Assert.Single(original.FeatureRules).Expression);
        var invalid = JsonNode.Parse("""
            {"name":"FeatureProfiles","FeatureProfiles":{"legacy":{"Name":"Rejected","FeatureRules":null}}}
            """);
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            step.ExecuteAsync(new RecipeExecutionContext { Name = "FeatureProfiles", Step = (JsonObject)invalid }));
        Assert.Same(original, document.FeatureProfiles["legacy"]);
    }
}
