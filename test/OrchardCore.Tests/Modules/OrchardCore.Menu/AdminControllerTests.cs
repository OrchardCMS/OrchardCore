using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Menu.Controllers;
using OrchardCore.Menu.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Menu;

public class AdminControllerTests
{
    private const string MenuId = "menu-id";
    private const string MenuItemId = "menu-item-id";
    private const string TextFromHandler = "Text from handler";

    [Fact]
    public async Task CreatePost_ValidModel_RaisesCreateEventsAroundEditorUpdateAndSavesTheirChanges()
    {
        var menu = CreateMenu();
        var events = new List<string>();
        var controller = CreateController(menu, events);

        await controller.CreatePost("LinkMenuItem", MenuId, null);

        Assert.Equal(["Creating", "Editor", "Created"], events);
        Assert.Equal(TextFromHandler, Assert.Single(Reload(menu).GetOrCreate<MenuItemsListPart>().MenuItems).DisplayText);
    }

    [Fact]
    public async Task CreatePost_InvalidModel_DoesNotRaiseCreatedOrAddMenuItem()
    {
        var menu = CreateMenu();
        var events = new List<string>();
        var controller = CreateController(menu, events, isModelValid: false);

        await controller.CreatePost("LinkMenuItem", MenuId, null);

        Assert.Equal(["Creating", "Editor"], events);
        Assert.Empty(Reload(menu).GetOrCreate<MenuItemsListPart>().MenuItems);
    }

    [Fact]
    public async Task EditPost_ValidModel_RaisesUpdateEventsAroundEditorUpdateAndSavesTheirChanges()
    {
        var menu = CreateMenu(new ContentItem { ContentItemId = MenuItemId, ContentType = "LinkMenuItem", DisplayText = "Old" });
        var events = new List<string>();
        var controller = CreateController(menu, events);

        await controller.EditPost(MenuId, MenuItemId);

        Assert.Equal([$"Updating {MenuItemId}", "Editor", $"Updated {MenuItemId}"], events);
        Assert.Equal(TextFromHandler, Assert.Single(Reload(menu).GetOrCreate<MenuItemsListPart>().MenuItems).DisplayText);
    }

    [Fact]
    public async Task EditPost_InvalidModel_DoesNotRaiseUpdatedOrChangeMenuItem()
    {
        var menu = CreateMenu(new ContentItem { ContentItemId = MenuItemId, ContentType = "LinkMenuItem", DisplayText = "Old" });
        var events = new List<string>();
        var controller = CreateController(menu, events, isModelValid: false);

        await controller.EditPost(MenuId, MenuItemId);

        Assert.Equal([$"Updating {MenuItemId}", "Editor"], events);
        Assert.Equal("Old", Assert.Single(Reload(menu).GetOrCreate<MenuItemsListPart>().MenuItems).DisplayText);
    }

    private static ContentItem Reload(ContentItem contentItem)
        => JObject.FromObject(contentItem).ToObject<ContentItem>();

    private static ContentItem CreateMenu(params ContentItem[] menuItems)
    {
        var menu = new ContentItem { ContentItemId = MenuId, ContentType = "Menu" };
        menu.Alter<MenuItemsListPart>(part => part.MenuItems.AddRange(menuItems));

        return menu;
    }

    private static AdminController CreateController(ContentItem menu, List<string> events, bool isModelValid = true)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var definitionManager = new Mock<IContentDefinitionManager>();
        definitionManager
            .Setup(manager => manager.GetTypeDefinitionAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => new ContentTypeDefinitionBuilder().WithName(name).Build());

        var contentManager = new Mock<IContentManager>();
        contentManager
            .Setup(manager => manager.GetAsync(MenuId, It.IsAny<VersionOptions>()))
            .ReturnsAsync(menu);
        contentManager
            .Setup(manager => manager.NewAsync(It.IsAny<string>()))
            .ReturnsAsync((string contentType) => new ContentItem { ContentItemId = "new-id", ContentType = contentType });

        var displayManager = new Mock<IContentItemDisplayManager>();
        AdminController controller = null;
        displayManager
            .Setup(manager => manager.UpdateEditorAsync(
                It.IsAny<ContentItem>(),
                It.IsAny<IUpdateModel>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback(() =>
            {
                events.Add("Editor");

                if (!isModelValid)
                {
                    controller.ModelState.AddModelError("Field", "Invalid");
                }
            })
            .ReturnsAsync(new Shape());

        controller = new AdminController(
            contentManager.Object,
            authorizationService.Object,
            displayManager.Object,
            definitionManager.Object,
            Mock.Of<INotifier>(),
            Mock.Of<IHtmlLocalizer<AdminController>>(),
            Mock.Of<IUpdateModelAccessor>(),
            [new RecordingContentHandler(events)],
            NullLogger<AdminController>.Instance)
        {
            ControllerContext = new()
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity("Test")) },
            },
        };

        return controller;
    }

    private sealed class RecordingContentHandler : ContentHandlerBase
    {
        private readonly List<string> _events;

        public RecordingContentHandler(List<string> events)
        {
            _events = events;
        }

        public override Task CreatingAsync(CreateContentContext context)
        {
            _events.Add("Creating");

            return Task.CompletedTask;
        }

        public override Task CreatedAsync(CreateContentContext context)
        {
            _events.Add("Created");
            context.ContentItem.DisplayText = TextFromHandler;

            return Task.CompletedTask;
        }

        public override Task UpdatingAsync(UpdateContentContext context)
        {
            _events.Add($"Updating {context.ContentItem.ContentItemId}");

            return Task.CompletedTask;
        }

        public override Task UpdatedAsync(UpdateContentContext context)
        {
            _events.Add($"Updated {context.ContentItem.ContentItemId}");
            context.ContentItem.DisplayText = TextFromHandler;

            return Task.CompletedTask;
        }
    }
}
