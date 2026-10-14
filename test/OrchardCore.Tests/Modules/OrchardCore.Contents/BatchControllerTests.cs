using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents.Controllers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Shapes;
using YesSql;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public class BatchControllerTests
{
    [Fact]
    public async Task GetCreatesPrefixedEditorsWithoutSaving()
    {
        var test = CreateHarness();
        var result = await test.Controller.BatchCreate("Article");

        var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(view.Model);
        Assert.Equal(5, test.Prefixes.Count);
        Assert.Equal(["BatchItems[0]", "BatchItems[1]", "BatchItems[2]", "BatchItems[3]", "BatchItems[4]"], test.Prefixes);
        test.Content.Verify(x => x.CreateAsync(It.IsAny<ContentItem>(), It.IsAny<VersionOptions>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task PostRejectsOutOfRangeCountBeforeCreatingAnything(int count)
    {
        var test = CreateHarness();

        Assert.IsType<BadRequestResult>(await test.Controller.BatchCreatePost("Article", count, "Publish"));
        test.Content.Verify(x => x.NewAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PostUpdatesEveryActiveEditorUsingIndependentPrefixes()
    {
        var test = CreateHarness();

        Assert.IsType<RedirectToActionResult>(await test.Controller.BatchCreatePost("Article", 3, "Save"));

        Assert.Equal(["BatchItems[0]", "BatchItems[1]", "BatchItems[2]"], test.UpdatedPrefixes);
        test.Content.Verify(x => x.CreateAsync(It.IsAny<ContentItem>(), VersionOptions.Draft), Times.Exactly(3));
        test.Content.Verify(x => x.SaveDraftAsync(It.IsAny<ContentItem>()), Times.Exactly(3));
        test.Content.Verify(x => x.PublishAsync(It.IsAny<ContentItem>()), Times.Never);
    }

    [Fact]
    public async Task InvalidEditorDoesNotPersistAnyItem()
    {
        var test = CreateHarness();
        test.Controller.ModelState.AddModelError("BatchItems[1].Title", "Required");

        Assert.IsType<ViewResult>(await test.Controller.BatchCreatePost("Article", 2, "Publish"));

        test.Content.Verify(x => x.CreateAsync(It.IsAny<ContentItem>(), It.IsAny<VersionOptions>()), Times.Never);
        test.Session.Verify(x => x.CancelAsync(), Times.Once);
    }

    [Fact]
    public async Task FailedPublishCancelsWholeSession()
    {
        var test = CreateHarness();
        test.Content.SetupSequence(x => x.PublishAsync(It.IsAny<ContentItem>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        Assert.IsType<ViewResult>(await test.Controller.BatchCreatePost("Article", 2, "Publish"));

        test.Content.Verify(x => x.CreateAsync(It.IsAny<ContentItem>(), VersionOptions.Draft), Times.Exactly(2));
        test.Session.Verify(x => x.CancelAsync(), Times.Once);
    }

    [Fact]
    public async Task FailedCreateCancelsWholeSession()
    {
        var test = CreateHarness();
        test.Content.SetupSequence(x => x.CreateAsync(It.IsAny<ContentItem>(), VersionOptions.Draft))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        Assert.IsType<ViewResult>(await test.Controller.BatchCreatePost("Article", 2, "Save"));

        test.Session.Verify(x => x.CancelAsync(), Times.Once);
    }

    [Fact]
    public async Task UnknownContentTypeReturnsNotFound()
    {
        var test = CreateHarness();
        Assert.IsType<NotFoundResult>(await test.Controller.BatchCreate("Unknown"));
    }

    [Fact]
    public async Task PostRejectsUnknownSubmitAction()
    {
        var test = CreateHarness();

        Assert.IsType<BadRequestResult>(await test.Controller.BatchCreatePost("Article", 2, "Delete"));
        test.Content.Verify(x => x.CreateAsync(It.IsAny<ContentItem>(), It.IsAny<VersionOptions>()), Times.Never);
    }

    private static Harness CreateHarness()
    {
        var content = new Mock<IContentManager>();
        content.Setup(x => x.NewAsync("Article")).ReturnsAsync(() => new ContentItem { ContentType = "Article" });
        content.Setup(x => x.CreateAsync(It.IsAny<ContentItem>(), VersionOptions.Draft)).ReturnsAsync(true);
        content.Setup(x => x.PublishAsync(It.IsAny<ContentItem>())).ReturnsAsync(true);

        var definition = new ContentTypeDefinitionBuilder()
            .WithName("Article")
            .WithDisplayName("Article")
            .Creatable()
            .Build();

        var definitions = new Mock<IContentDefinitionManager>();
        definitions.Setup(x => x.GetTypeDefinitionAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => name == "Article" ? definition : null);

        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(x => x.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var prefixes = new List<string>();
        var updatedPrefixes = new List<string>();
        var editors = new Mock<IContentItemDisplayManager>();
        editors.Setup(x => x.BuildEditorAsync(
            It.IsAny<ContentItem>(), It.IsAny<IUpdateModel>(), true, "", It.IsAny<string>()))
            .Callback<ContentItem, IUpdateModel, bool, string, string>((_, _, _, _, prefix) => prefixes.Add(prefix))
            .ReturnsAsync(() => new Shape());

        editors.Setup(x => x.UpdateEditorAsync(
            It.IsAny<ContentItem>(), It.IsAny<IUpdateModel>(), true, "", It.IsAny<string>()))
            .Callback<ContentItem, IUpdateModel, bool, string, string>((_, _, _, _, prefix) => updatedPrefixes.Add(prefix))
            .ReturnsAsync(() => new Shape());

        var accessor = new Mock<IUpdateModelAccessor>();
        accessor.SetupGet(x => x.ModelUpdater).Returns(Mock.Of<IUpdateModel>());

        var session = new Mock<ISession>();
        var localizer = new Mock<IHtmlLocalizer<BatchController>>();
        var controller = new BatchController(
            content.Object,
            editors.Object,
            definitions.Object,
            authorization.Object,
            accessor.Object,
            session.Object,
            Mock.Of<INotifier>(),
            localizer.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, "user-id")], "Test")),
                },
            },
        };

        return new Harness(controller, content, session, prefixes, updatedPrefixes);
    }

    private sealed record Harness(
        BatchController Controller,
        Mock<IContentManager> Content,
        Mock<ISession> Session,
        List<string> Prefixes,
        List<string> UpdatedPrefixes);
}
