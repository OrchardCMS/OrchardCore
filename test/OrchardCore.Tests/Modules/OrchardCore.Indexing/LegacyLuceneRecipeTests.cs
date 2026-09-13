using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Lucene.Recipes;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class LegacyLuceneRecipeTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NewProfile_UsesSharedCreationAndCompensation(bool accepted)
    {
        var profile = Profile();
        var profiles = new Mock<IIndexProfileManager>();
        profiles.Setup(value => value.NewAsync("Lucene", "Content", It.IsAny<JsonNode>())).ReturnsAsync(profile);
        profiles.Setup(value => value.ValidateAsync(profile)).ReturnsAsync(new ValidationResultDetails());
        profiles.Setup(value => value.DeleteAsync(profile)).ReturnsAsync(true);
        var provider = new Mock<IIndexManager>();
        provider.Setup(value => value.CreateAsync(profile)).ReturnsAsync(accepted);
        using var services = new ServiceCollection().AddKeyedSingleton("Lucene", provider.Object).BuildServiceProvider();
        var context = Context();
        var recipe = Recipe(profiles.Object, new IndexProfileManagementService(profiles.Object, services), provider.Object);

        await recipe.ExecuteAsync(context);

        Assert.Equal(accepted ? 0 : 1, context.Errors.Count);
        profiles.Verify(value => value.CreateAsync(profile), Times.Once);
        profiles.Verify(value => value.DeleteAsync(profile), accepted ? Times.Never() : Times.Once());
        profiles.Verify(value => value.SynchronizeAsync(profile), accepted ? Times.Once() : Times.Never());
        provider.Verify(value => value.CreateAsync(profile), Times.Once);
        provider.Verify(value => value.ExistsAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistingProfile_PreservesLegacyEnsureBehaviorWithoutRecreatingTheProfile(bool exists)
    {
        var profile = Profile();
        var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        profiles.Setup(value => value.FindByNameAndProviderAsync("articles", "Lucene")).ReturnsAsync(profile);
        profiles.Setup(value => value.SynchronizeAsync(profile)).Returns(ValueTask.CompletedTask);
        var management = new Mock<IIndexProfileManagementService>(MockBehavior.Strict);
        var provider = new Mock<IIndexManager>();
        provider.Setup(value => value.ExistsAsync("articles")).ReturnsAsync(exists);
        provider.Setup(value => value.CreateAsync(profile)).ReturnsAsync(true);
        var context = Context();

        await Recipe(profiles.Object, management.Object, provider.Object).ExecuteAsync(context);

        Assert.Empty(context.Errors);
        management.VerifyNoOtherCalls();
        provider.Verify(value => value.CreateAsync(profile), exists ? Times.Never() : Times.Once());
        profiles.Verify(value => value.FindByNameAndProviderAsync("articles", "Lucene"), Times.Once);
        profiles.Verify(value => value.SynchronizeAsync(profile), Times.Once);
        profiles.VerifyNoOtherCalls();
    }

    private static IndexProfile Profile() => new() { Id = "id", Name = "Articles", IndexName = "articles", IndexFullName = "articles", ProviderName = "Lucene", Type = "Content" };

    private static RecipeExecutionContext Context() => new()
    {
        Name = "lucene-index", Step = JsonNode.Parse("""{"Indices":[{"articles":{"IndexedContentTypes":["Article"]}}]}""").AsObject(),
    };

    private static LuceneIndexStep Recipe(IIndexProfileManager profiles, IIndexProfileManagementService management, IIndexManager provider)
    {
        var localizer = new Mock<IStringLocalizer<LuceneIndexStep>>();
        localizer.Setup(value => value[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string message, object[] arguments) => new LocalizedString(message, string.Format(message, arguments)));
        return new LuceneIndexStep(profiles, management, provider, NullLogger<LuceneIndexStep>.Instance, localizer.Object);
    }
}
