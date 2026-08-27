using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;
using YesSql.Filters.Nodes;
using YesSql.Filters.Query;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public class DefaultContentsAdminListQueryServiceTests
{
    [Fact]
    public async Task QueryAsync_NameOnlyDefaultTermRewrite_RestoresOriginalNode()
    {
       // Arrange
        var query = Mock.Of<IQuery<ContentItem>>();
        var rootQuery = new Mock<IQuery>();
        rootQuery
            .Setup(value => value.For<ContentItem>(It.IsAny<bool>()))
            .Returns(query);

        var session = new Mock<YesSql.ISession>();
        session
            .Setup(value => value.Query(It.IsAny<string>()))
            .Returns(rootQuery.Object);

        using var services = new ServiceCollection().BuildServiceProvider();

        var options = new ContentsAdminListFilterOptions();
        options.DefaultTermNames["Article"] = "customText";

        var service = new DefaultContentsAdminListQueryService(
            session.Object,
            services,
            [],
            Options.Create(options),
            NullLogger<DefaultContentsAdminListQueryService>.Instance);

        var filterResult = CreateFilterResult();
        var originalNode = Assert.Single(filterResult.OfType<DefaultTermNode>());
        var model = new ContentOptionsViewModel
        {
            SelectedContentType = "Article",
            FilterResult = filterResult,
        };

        await service.QueryAsync(model, Mock.Of<IUpdateModel>());

        var restoredNode = Assert.Single(model.FilterResult.OfType<DefaultTermNode>());
        Assert.Equal(ContentsAdminListFilterOptions.DefaultTermName, restoredNode.TermName);
        Assert.Same(originalNode, restoredNode);
    }

    private static QueryFilterResult<ContentItem> CreateFilterResult()
    {
        static IQuery<ContentItem> Apply(string _, IQuery<ContentItem> query) => query;

        var builder = new QueryEngineBuilder<ContentItem>();
        builder
            .WithDefaultTerm(ContentsAdminListFilterOptions.DefaultTermName, term => term
                .ManyCondition(Apply, Apply))
            .WithDefaultTerm("customText", term => term
                .ManyCondition(Apply, Apply));

        var result = builder.Build().Parse("does-not-exist");
        var parsedNode = Assert.Single(result.OfType<DefaultTermNode>());

        result.TryRemove(parsedNode.TermName);
        result.TryAddOrReplace(new DefaultTermNode(
            ContentsAdminListFilterOptions.DefaultTermName,
            parsedNode.Operation));

        return result;
    }
}
