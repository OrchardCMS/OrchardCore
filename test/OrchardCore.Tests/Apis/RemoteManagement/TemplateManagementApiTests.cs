using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Templates.Endpoints.Management;
using OrchardCore.Templates.Services;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class TemplateManagementApiTests
{
    [Fact]
    public async Task Mutations_RetriesAndConflict_PersistAcrossTenantScopes()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var name = $"Content__RemoteManagement_{Guid.NewGuid():N}";
        var definition = new TemplateDefinitionDto
        {
            Name = name,
            Content = "<article>Remote management</article>",
            Description = "Remote management integration test.",
        };

        await AssertStatusAsync(context, manager =>
            TemplateManagementEndpoints.CreateAsync(
                new DefaultHttpContext(),
                CreateAuthorizationService(),
                manager,
                definition),
            StatusCodes.Status201Created);

        await AssertStatusAsync(context, manager =>
            TemplateManagementEndpoints.CreateAsync(
                new DefaultHttpContext(),
                CreateAuthorizationService(),
                manager,
                definition),
            StatusCodes.Status201Created);

        await AssertStatusAsync(context, manager =>
            TemplateManagementEndpoints.CreateAsync(
                new DefaultHttpContext(),
                CreateAuthorizationService(),
                manager,
                new TemplateDefinitionDto
                {
                    Name = name,
                    Content = "<article>Conflicting content</article>",
                    Description = definition.Description,
                }),
            StatusCodes.Status409Conflict);

        var updated = new TemplateDefinitionDto
        {
            Name = name,
            Content = "<article>Updated remote management</article>",
            Description = "Updated integration test.",
        };
        await AssertStatusAsync(context, manager =>
            TemplateManagementEndpoints.UpdateAsync(
                new DefaultHttpContext(),
                CreateAuthorizationService(),
                manager,
                name,
                updated),
            StatusCodes.Status200OK);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<TemplatesManager>();
            var result = await TemplateManagementEndpoints.GetAsync(
                new DefaultHttpContext(),
                CreateAuthorizationService(),
                manager,
                name);
            var response = Assert.IsType<global::Microsoft.AspNetCore.Http.HttpResults.Ok<TemplateDefinitionDto>>(result).Value;

            Assert.Equal(updated.Content, response.Content);
            Assert.Equal(updated.Description, response.Description);
        });

        await AssertStatusAsync(context, manager =>
            TemplateManagementEndpoints.DeleteAsync(
                new DefaultHttpContext(),
                CreateAuthorizationService(),
                manager,
                name),
            StatusCodes.Status200OK);

        await AssertStatusAsync(context, manager =>
            TemplateManagementEndpoints.DeleteAsync(
                new DefaultHttpContext(),
                CreateAuthorizationService(),
                manager,
                name),
            StatusCodes.Status204NoContent);
    }

    private static async Task AssertStatusAsync(
        SiteContext context,
        Func<TemplatesManager, Task<IResult>> action,
        int expectedStatus)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var result = await action(scope.ServiceProvider.GetRequiredService<TemplatesManager>());
            Assert.Equal(expectedStatus, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        });
    }

    private static IAuthorizationService CreateAuthorizationService()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        return authorizationService.Object;
    }
}
