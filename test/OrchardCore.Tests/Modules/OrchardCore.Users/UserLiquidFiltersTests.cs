using Fluid;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Liquid;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

public class UserLiquidFiltersTests
{
    [Fact]
    public async Task UsersByName_SingleUsername_ReturnsUser()
    {
        // Arrange
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            // Create a test user
            var testUser = new User
            {
                UserName = "testuser",
                Email = "testuser@test.com",
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(testUser, "Test@Pass123");

            var template = """
                {% assign user = "testuser" | users_by_name %}
                {{ user.UserName }}
                """;

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default);

            // Assert
            Assert.Contains("testuser", result);
        });
    }

    [Fact]
    public async Task UsersByName_CaseInsensitive_ReturnsUser()
    {
        // Arrange
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            // Create a test user
            var testUser = new User
            {
                UserName = "TestUser",
                Email = "testuser2@test.com",
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(testUser, "Test@Pass123");

            // Use lowercase username in filter - should still find user due to normalization
            var template = """
                {% assign user = "testuser" | users_by_name %}
                {{ user.UserName }}
                """;

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default);

            // Assert
            Assert.Contains("TestUser", result);
        });
    }

    [Fact]
    public async Task UsersByName_MultipleUsernames_ReturnsUsers()
    {
        // Arrange
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            // Create test users
            var user1 = new User
            {
                UserName = "user1",
                Email = "user1@test.com",
                EmailConfirmed = true,
            };

            var user2 = new User
            {
                UserName = "user2",
                Email = "user2@test.com",
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(user1, "Test@Pass123");
            await userManager.CreateAsync(user2, "Test@Pass123");

            var template = """
                {% assign usernames = "user1,user2" | split: "," %}
                {% assign users = usernames | users_by_name %}
                {% for user in users %}{{ user.UserName }},{% endfor %}
                """;

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default);

            // Assert
            Assert.Contains("user1", result);
            Assert.Contains("user2", result);
        });
    }

    [Fact]
    public async Task UsersByName_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = """
                {% assign user = "nonexistentuser" | users_by_name %}
                {% if user == nil %}null{% else %}{{ user.UserName }}{% endif %}
                """;

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default);

            // Assert
            Assert.Contains("null", result);
        });
    }

    [Fact]
    public async Task UsersByName_AccessUserProperties_ReturnsCorrectValues()
    {
        // Arrange
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            // Create a test user with specific properties
            var testUser = new User
            {
                UserName = "propuser",
                Email = "propuser@test.com",
                EmailConfirmed = true,
                IsEnabled = true,
            };

            await userManager.CreateAsync(testUser, "Test@Pass123");

            var template = """
                {% assign user = "propuser" | users_by_name %}
                Username: {{ user.UserName }}
                Email: {{ user.Email }}
                IsEnabled: {{ user.IsEnabled }}
                """;

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default);

            // Assert
            Assert.Contains("Username: propuser", result);
            Assert.Contains("Email: propuser@test.com", result);
            Assert.Contains("IsEnabled: true", result);
        });
    }
}
