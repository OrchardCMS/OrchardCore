using System.Linq.Expressions;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrchardCore.Alias.Handlers;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Models;
using OrchardCore.Autoroute.Core.Indexes;
using OrchardCore.Autoroute.Handlers;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid;
using OrchardCore.Localization;
using OrchardCore.Settings;
using YesSql.Indexes;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.ContentManagement;

public class GeneratedTextBoundaryTests
{
    [Theory]
    [InlineData("😀")]
    [InlineData("e\u0301")]
    [InlineData("🇺🇸")]
    [InlineData("👩‍💻")]
    public async Task CreatedAsync_AliasPatternCutsTextElement_PreservesWholeElements(string element)
    {
        var prefix = new string('a', AliasPart.MaxAliasLength - 1);
        var item = new ContentItem { ContentType = "Type", ContentItemId = "item" };
        var part = new AliasPart { ContentItem = item };
        var handler = CreateAliasHandler(prefix + element + "more");

        await handler.CreatedAsync(new CreateContentContext(item), part);

        Assert.Equal(prefix, part.Alias);
    }

    [Theory]
    [InlineData("😀")]
    [InlineData("e\u0301")]
    [InlineData("🇺🇸")]
    [InlineData("👩‍💻")]
    public async Task CloningAsync_AliasVersionCutsTextElement_PreservesWholeElements(string element)
    {
        var prefix = new string('a', AliasPart.MaxAliasLength - 3);
        var item = new ContentItem { ContentType = "Type", ContentItemId = "item" };
        var clone = new ContentItem { ContentType = "Type", ContentItemId = "clone" };
        clone.Apply(new AliasPart());
        var part = new AliasPart { ContentItem = item, Alias = prefix + element + "more" };

        await CreateAliasHandler("").CloningAsync(new CloneContentContext(item, clone), part);

        Assert.Equal(prefix + "-1", clone.As<AliasPart>().Alias);
    }

    [Theory]
    [InlineData("😀")]
    [InlineData("e\u0301")]
    [InlineData("🇺🇸")]
    [InlineData("👩‍💻")]
    public async Task CreatedAsync_AutoroutePatternCutsTextElement_PreservesWholeElements(string element)
    {
        var prefix = new string('a', AutoroutePart.MaxPathLength - 1);
        var item = new ContentItem { ContentType = "Type", ContentItemId = "item" };
        var part = new AutoroutePart { ContentItem = item };
        var contentManager = new Mock<IContentManager>();
        contentManager.Setup(m => m.PopulateAspectAsync(It.IsAny<IContent>(), It.IsAny<CultureAspect>()))
            .ReturnsAsync((IContent content, CultureAspect aspect) => aspect);
        using var services = new ServiceCollection()
            .AddSingleton(contentManager.Object)
            .AddSingleton(Options.Create(new CultureOptions()))
            .BuildServiceProvider();
        var handler = new AutoroutePartHandler(
            Mock.Of<IAutorouteEntries>(), Options.Create(new AutorouteOptions()),
            Templates(prefix + element + "more"), Definitions(nameof(AutoroutePart)),
            Mock.Of<ISiteService>(), Mock.Of<ITagCache>(), Session<AutoroutePartIndex>(), services,
            Mock.Of<IStringLocalizer<AutoroutePartHandler>>());

        await handler.CreatedAsync(new CreateContentContext(item), part);

        Assert.Equal(prefix, part.Path);
    }

    private static AliasPartHandler CreateAliasHandler(string rendered) => new(
        Definitions(nameof(AliasPart)), Mock.Of<ITagCache>(), Templates(rendered),
        Session<AliasPartIndex>(), Mock.Of<IStringLocalizer<AliasPartHandler>>());

    private static IContentDefinitionManager Definitions(string partName)
    {
        var settings = new JsonObject { [partName + "Settings"] = new JsonObject { ["Pattern"] = "pattern" } };
        var part = new ContentTypePartDefinition(partName, new ContentPartDefinition(partName), settings);
        var definition = new ContentTypeDefinition("Type", "Type", [part], []);
        var manager = new Mock<IContentDefinitionManager>();
        manager.Setup(m => m.GetTypeDefinitionAsync("Type")).ReturnsAsync(definition);
        return manager.Object;
    }

    private static ILiquidTemplateManager Templates(string rendered)
    {
        var manager = new Mock<ILiquidTemplateManager>();
        manager.Setup(m => m.RenderStringAsync("pattern", It.IsAny<TextEncoder>(), It.IsAny<object>(), It.IsAny<IEnumerable<KeyValuePair<string, FluidValue>>>()))
            .ReturnsAsync(rendered);
        return manager.Object;
    }

    private static ISession Session<TIndex>() where TIndex : class, IIndex
    {
        var index = new Mock<IQueryIndex<TIndex>>();
        index.Setup(q => q.Where(It.IsAny<Expression<Func<TIndex, bool>>>())).Returns(index.Object);
        index.Setup(q => q.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        index.Setup(q => q.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<TIndex>());
        var query = new Mock<IQuery>();
        query.Setup(q => q.ForIndex<TIndex>()).Returns(index.Object);
        var session = new Mock<ISession>();
        session.Setup(s => s.Query(It.IsAny<string>())).Returns(query.Object);
        return session.Object;
    }
}
