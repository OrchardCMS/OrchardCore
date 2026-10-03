using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.RemotePublishing;
using OrchardCore.Media;
using OrchardCore.MetaWeblog;
using OrchardCore.Security.Permissions;
using OrchardCore.Users;
using OrchardCore.Users.Services;
using OrchardCore.XmlRpc;
using OrchardCore.XmlRpc.Models;
using YesSqlSession = YesSql.ISession;

namespace OrchardCore.Tests.Modules.OrchardCore.Lists;

public class MetaWeblogAuthorizationTests
{
    [Fact]
    public async Task NewPost_ReauthorizesMutatedResourceWithXmlRpcPrincipal()
    {
        var xmlRpcPrincipal = new ClaimsPrincipal(new ClaimsIdentity("XmlRpc"));
        var contentManager = new Mock<IContentManager>();
        var list = new ContentItem
        {
            ContentItemId = "blog",
            ContentType = "Blog",
        };
        var post = new ContentItem
        {
            ContentItemId = "post",
            ContentType = "BlogPost",
        };
        post.Weld(new ContainedPart());
        contentManager
            .Setup(manager => manager.GetAsync("blog", null))
            .ReturnsAsync(list);
        contentManager
            .Setup(manager => manager.NewAsync("BlogPost"))
            .ReturnsAsync(post);

        var authorizationService = CreateMutationAuthorizationService(xmlRpcPrincipal, failOnResourceCall: 1);
        var session = new Mock<YesSqlSession>();
        var handler = CreateHandler(
            contentManager,
            authorizationService,
            session,
            CreateContentDefinitionManager(),
            xmlRpcPrincipal,
            CreateMutatingDriver());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.ProcessAsync(CreateNewPostContext()));

        authorizationService.Verify(service => service.AuthorizeAsync(
            xmlRpcPrincipal,
            post,
            It.IsAny<IEnumerable<IAuthorizationRequirement>>()));
        session.Verify(session => session.CancelAsync());
        contentManager.Verify(
            manager => manager.CreateAsync(It.IsAny<ContentItem>(), It.IsAny<VersionOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task EditPost_ReauthorizesAfterDriverMutationAndCancelsSession()
    {
        var xmlRpcPrincipal = new ClaimsPrincipal(new ClaimsIdentity("XmlRpc"));
        var contentManager = new Mock<IContentManager>();
        var post = new ContentItem
        {
            ContentItemId = "post",
            ContentType = "BlogPost",
        };
        contentManager
            .Setup(manager => manager.GetAsync("post", VersionOptions.DraftRequired))
            .ReturnsAsync(post);

        var authorizationService = CreateMutationAuthorizationService(xmlRpcPrincipal, failOnResourceCall: 2);
        var session = new Mock<YesSqlSession>();
        var handler = CreateHandler(
            contentManager,
            authorizationService,
            session,
            Mock.Of<IContentDefinitionManager>(),
            xmlRpcPrincipal,
            CreateMutatingDriver());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.ProcessAsync(CreateEditPostContext()));

        authorizationService.Verify(service => service.AuthorizeAsync(
            xmlRpcPrincipal,
            post,
            It.IsAny<IEnumerable<IAuthorizationRequirement>>()),
            Times.Exactly(2));
        session.Verify(session => session.CancelAsync());
        contentManager.Verify(
            manager => manager.SaveDraftAsync(It.IsAny<ContentItem>()),
            Times.Never);
        contentManager.Verify(
            manager => manager.PublishAsync(It.IsAny<ContentItem>()),
            Times.Never);
    }

    private static MetaWeblogHandler CreateHandler(
        Mock<IContentManager> contentManager,
        Mock<IAuthorizationService> authorizationService,
        Mock<YesSqlSession> session,
        IContentDefinitionManager contentDefinitionManager,
        ClaimsPrincipal xmlRpcPrincipal,
        IMetaWeblogDriver driver)
    {
        var membershipService = new Mock<IMembershipService>();
        membershipService
            .Setup(service => service.CheckPasswordAsync("user", "password"))
            .ReturnsAsync(true);
        membershipService
            .Setup(service => service.GetUserAsync("user"))
            .ReturnsAsync(Mock.Of<IUser>());
        membershipService
            .Setup(service => service.CreateClaimsPrincipal(It.IsAny<IUser>()))
            .ReturnsAsync(xmlRpcPrincipal);

        return new MetaWeblogHandler(
            contentManager.Object,
            authorizationService.Object,
            membershipService.Object,
            session.Object,
            contentDefinitionManager,
            Mock.Of<IMediaFileStore>(),
            null,
            Options.Create(new MediaOptions()),
            [driver],
            CreateLocalizer());
    }

    private static Mock<IAuthorizationService> CreateMutationAuthorizationService(
        ClaimsPrincipal xmlRpcPrincipal,
        int failOnResourceCall)
    {
        var resourceAuthorizationCount = 0;
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((
                ClaimsPrincipal principal,
                object resource,
                IEnumerable<IAuthorizationRequirement> _) =>
            {
                Assert.Same(xmlRpcPrincipal, principal);

                if (resource is null)
                {
                    return AuthorizationResult.Success();
                }

                resourceAuthorizationCount++;

                return resourceAuthorizationCount == failOnResourceCall
                    ? AuthorizationResult.Failed()
                    : AuthorizationResult.Success();
            });

        return authorizationService;
    }

    private static IContentDefinitionManager CreateContentDefinitionManager()
    {
        var listPart = new ContentPartDefinition("ListPart");
        var listTypePart = new ContentTypePartDefinition(
            "ListPart",
            listPart,
            new JsonObject
            {
                [nameof(ListPartSettings)] = new JsonObject
                {
                    [nameof(ListPartSettings.ContainedContentTypes)] = new JsonArray("BlogPost"),
                },
            });
        var listType = new ContentTypeDefinition(
            "Blog",
            "Blog",
            [listTypePart],
            new JsonObject());
        var postType = new ContentTypeDefinition("BlogPost", "Blog post");
        var contentDefinitionManager = new Mock<IContentDefinitionManager>();
        contentDefinitionManager
            .Setup(manager => manager.GetTypeDefinitionAsync("Blog"))
            .ReturnsAsync(listType);
        contentDefinitionManager
            .Setup(manager => manager.GetTypeDefinitionAsync("BlogPost"))
            .ReturnsAsync(postType);

        return contentDefinitionManager.Object;
    }

    private static IMetaWeblogDriver CreateMutatingDriver()
    {
        var driver = new Mock<IMetaWeblogDriver>();
        driver
            .Setup(item => item.EditPost(It.IsAny<XRpcStruct>(), It.IsAny<ContentItem>()))
            .Callback((XRpcStruct _, ContentItem contentItem) =>
            {
                ((JsonObject)contentItem.Content)["Embedded"] = new JsonObject
                {
                    [nameof(ContentItem.ContentType)] = "LiquidWidget",
                };
            });

        return driver.Object;
    }

    private static XmlRpcContext CreateNewPostContext() =>
        new()
        {
            RpcMethodCall = new XRpcMethodCall
            {
                MethodName = "metaWeblog.newPost",
                Params =
                [
                    XRpcData.For("blog"),
                    XRpcData.For("user"),
                    XRpcData.For("password"),
                    XRpcData.For(new XRpcStruct()),
                    XRpcData.For(false),
                ],
            },
        };

    private static XmlRpcContext CreateEditPostContext() =>
        new()
        {
            RpcMethodCall = new XRpcMethodCall
            {
                MethodName = "metaWeblog.editPost",
                Params =
                [
                    XRpcData.For("post"),
                    XRpcData.For("user"),
                    XRpcData.For("password"),
                    XRpcData.For(new XRpcStruct()),
                    XRpcData.For(false),
                ],
            },
        };

    private static IStringLocalizer<MetaWeblogHandler> CreateLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<MetaWeblogHandler>>();
        localizer
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        return localizer.Object;
    }
}
