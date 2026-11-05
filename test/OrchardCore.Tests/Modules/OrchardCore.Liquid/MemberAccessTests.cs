using Fluid;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Liquid;

public class MemberAccessTests
{
    [Fact]
    public async Task RenderString_ContentItem_ReadsMembers()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentItem = new ContentItem { ContentItemId = "id", DisplayText = "Text" };
            contentItem.Content.TitlePart = new System.Text.Json.Nodes.JsonObject { ["Title"] = "Title" };

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(
                "{{ Model.ContentItemId }} {{ Model.DisplayText }} {{ Model.Content.TitlePart.Title }}",
                NullEncoder.Default,
                contentItem);

            Assert.Equal("id Text Title", result);
        });
    }

    [Theory]
    [InlineData("{{ Model.UserName }}", "admin")]
    [InlineData("{{ Model.PasswordHash }}", "")]
    [InlineData("{{ Model.SecurityStamp }}", "")]
    [InlineData("{{ Model.ResetToken }}", "")]
    public async Task RenderString_User_OnlyReadsExposedMembers(string template, string expected)
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var user = new User { UserName = "admin", PasswordHash = "hash", SecurityStamp = "stamp", ResetToken = "token" };

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();

            Assert.Equal(expected, await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default, user));
        });
    }

    [Fact]
    public async Task RenderString_SiteSalt_IsRedacted()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();

            Assert.Equal("[REDACTED]", await liquidTemplateManager.RenderStringAsync("{{ Site.SiteSalt }}", NullEncoder.Default));
        });
    }
}
