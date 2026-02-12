using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Lists.Handlers;
using OrchardCore.Lists.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Lists.Handlers;

public class ContainedPartHandlerTests
{
    private readonly Mock<IContentDefinitionManager> _contentDefinitionManager;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly ContainedPartHandler _handler;

    public ContainedPartHandlerTests()
    {
        _contentDefinitionManager = new Mock<IContentDefinitionManager>();
        _serviceProvider = new Mock<IServiceProvider>();
        _serviceProvider
            .Setup(sp => sp.GetService(typeof(IContentDefinitionManager)))
            .Returns(_contentDefinitionManager.Object);

        var localizer = new Mock<IStringLocalizer<ContainedPartHandler>>();
        localizer
            .Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns<string, object[]>((s, args) => new LocalizedString(s, string.Format(s, args)));

        _handler = new ContainedPartHandler(_serviceProvider.Object, localizer.Object);
    }

    [Fact]
    public async Task ValidatingAsync_ShouldNotFail_WhenContentTypeIsNotContained()
    {
        _contentDefinitionManager
            .Setup(m => m.ListTypeDefinitionsAsync())
            .ReturnsAsync([]);

        var contentItem = new ContentItem { ContentType = "Page" };
        var context = new ValidateContentContext(contentItem);

        await _handler.ValidatingAsync(context);

        Assert.Empty(context.ContentValidateResult.Errors);
    }

    [Fact]
    public async Task ValidatingAsync_ShouldFail_WhenContainedTypeHasNoContainedPart()
    {
        SetupBlogWithBlogPostContainedType();

        var contentItem = new ContentItem { ContentType = "BlogPost" };
        var context = new ValidateContentContext(contentItem);

        await _handler.ValidatingAsync(context);

        Assert.NotEmpty(context.ContentValidateResult.Errors);
    }

    [Fact]
    public async Task ValidatingAsync_ShouldFail_WhenListContentItemIdIsEmpty()
    {
        SetupBlogWithBlogPostContainedType();

        var contentItem = new ContentItem { ContentType = "BlogPost" };
        contentItem.Weld<ContainedPart>();
        contentItem.Alter<ContainedPart>(p =>
        {
            p.ListContentItemId = string.Empty;
            p.ListContentType = "Blog";
        });

        var context = new ValidateContentContext(contentItem);

        await _handler.ValidatingAsync(context);

        Assert.Contains(context.ContentValidateResult.Errors,
            e => e.MemberNames.Contains(nameof(ContainedPart.ListContentItemId)));
    }

    [Fact]
    public async Task ValidatingAsync_ShouldFail_WhenListContentTypeIsEmpty()
    {
        SetupBlogWithBlogPostContainedType();

        var contentItem = new ContentItem { ContentType = "BlogPost" };
        contentItem.Weld<ContainedPart>();
        contentItem.Alter<ContainedPart>(p =>
        {
            p.ListContentItemId = "some-id";
            p.ListContentType = string.Empty;
        });

        var context = new ValidateContentContext(contentItem);

        await _handler.ValidatingAsync(context);

        Assert.Contains(context.ContentValidateResult.Errors,
            e => e.MemberNames.Contains(nameof(ContainedPart.ListContentType)));
    }

    [Fact]
    public async Task ValidatingAsync_ShouldNotFail_WhenContainedPartIsValid()
    {
        SetupBlogWithBlogPostContainedType();

        var contentItem = new ContentItem { ContentType = "BlogPost" };
        contentItem.Weld<ContainedPart>();
        contentItem.Alter<ContainedPart>(p =>
        {
            p.ListContentItemId = "blog-content-item-id";
            p.ListContentType = "Blog";
        });

        var context = new ValidateContentContext(contentItem);

        await _handler.ValidatingAsync(context);

        Assert.Empty(context.ContentValidateResult.Errors);
    }

    [Fact]
    public async Task ValidatingAsync_ShouldNotFail_WhenContentTypeIsNotInContainedTypes()
    {
        SetupBlogWithBlogPostContainedType();

        var contentItem = new ContentItem { ContentType = "Article" };
        var context = new ValidateContentContext(contentItem);

        await _handler.ValidatingAsync(context);

        Assert.Empty(context.ContentValidateResult.Errors);
    }

    [Fact]
    public async Task ValidatingAsync_ShouldNotFail_WhenContentTypeIsCreatableAndListable()
    {
        SetupBlogWithBlogPostContainedType(creatableAndListable: true);

        var contentItem = new ContentItem { ContentType = "BlogPost" };
        var context = new ValidateContentContext(contentItem);

        await _handler.ValidatingAsync(context);

        Assert.Empty(context.ContentValidateResult.Errors);
    }

    private void SetupBlogWithBlogPostContainedType(bool creatableAndListable = false)
    {
        var listPartSettings = new JsonObject
        {
            [nameof(ListPartSettings)] = JsonSerializer.SerializeToNode(new ListPartSettings
            {
                ContainedContentTypes = ["BlogPost"],
            }),
        };

        var blogTypeDef = new ContentTypeDefinition(
            "Blog",
            "Blog",
            [
                new ContentTypePartDefinition(
                    nameof(ListPart),
                    new ContentPartDefinition(nameof(ListPart)),
                    listPartSettings),
            ],
            new JsonObject());

        _contentDefinitionManager
            .Setup(m => m.ListTypeDefinitionsAsync())
            .ReturnsAsync([blogTypeDef]);

        var blogPostTypeSettings = new JsonObject();

        if (creatableAndListable)
        {
            blogPostTypeSettings[nameof(ContentTypeSettings)] = JsonSerializer.SerializeToNode(new ContentTypeSettings
            {
                Creatable = true,
                Listable = true,
            });
        }

        var blogPostTypeDef = new ContentTypeDefinition(
            "BlogPost",
            "Blog Post",
            [],
            blogPostTypeSettings);

        _contentDefinitionManager
            .Setup(m => m.GetTypeDefinitionAsync("BlogPost"))
            .ReturnsAsync(blogPostTypeDef);
    }
}
