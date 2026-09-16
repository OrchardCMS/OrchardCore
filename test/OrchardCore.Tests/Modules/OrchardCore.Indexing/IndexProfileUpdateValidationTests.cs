using Microsoft.Extensions.Localization;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Recipes.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexProfileUpdateValidationTests
{
    [Fact]
    public async Task Update_IncomingDataInvalidatesProfile_DoesNotSaveAndRestoresTrackedValues()
    {
        var profile = new IndexProfile { Id = "id", Name = "Original", ProviderName = "Lucene", Type = "Content" };
        profile.Properties["Private"] = new JsonObject { ["Value"] = "Original" };
        var store = new Mock<IIndexProfileStore>();
        store.Setup(value => value.UpdateAsync(profile)).Returns(ValueTask.CompletedTask);
        var manager = new DefaultIndexProfileManager(store.Object, [new TestHandler()], NullLogger<DefaultIndexProfileManager>.Instance);
        Assert.True((await manager.ValidateAsync(profile)).Succeeded);

        await Assert.ThrowsAnyAsync<ValidationException>(() => manager.UpdateAsync(profile,
            new JsonObject { ["Name"] = "Rejected" }).AsTask());

        store.Verify(value => value.UpdateAsync(It.IsAny<IndexProfile>()), Times.Never);
        Assert.Equal("Original", profile.Name);
        Assert.Equal("Original", profile.Properties["Private"]["Value"].GetValue<string>());
    }

    [Fact]
    public async Task Update_ValidIncomingValues_UsesExistingHandlerAndStore()
    {
        var profile = new IndexProfile { Id = "id", Name = "Original" };
        profile.Properties["Private"] = new JsonObject { ["Value"] = "Original" };
        var store = new Mock<IIndexProfileStore>();
        store.Setup(value => value.UpdateAsync(profile)).Returns(ValueTask.CompletedTask);
        var manager = new DefaultIndexProfileManager(store.Object, [new TestHandler()], NullLogger<DefaultIndexProfileManager>.Instance);

        await manager.UpdateAsync(profile, new JsonObject { ["Name"] = "Accepted" });

        Assert.Equal("Accepted", profile.Name);
        store.Verify(value => value.UpdateAsync(profile), Times.Once);
    }

    [Fact]
    public async Task Recipe_InvalidIncomingValues_ReportsErrorAndKeepsStoredProfile()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var profile = new IndexProfile { Id = "id", Name = "Original", ProviderName = "Lucene", Type = "Content" };
        profile.Properties["Private"] = new JsonObject { ["Value"] = "Original" };
        var store = new Mock<IIndexProfileStore>();
        store.Setup(value => value.FindByIdAsync("id")).ReturnsAsync(profile);
        store.Setup(value => value.UpdateAsync(profile)).Returns(ValueTask.CompletedTask);
        var manager = new DefaultIndexProfileManager(store.Object, [new TestHandler()], NullLogger<DefaultIndexProfileManager>.Instance);
        var step = new CreateOrUpdateIndexProfileStep(manager, Options.Create(new IndexingOptions()), new IndexProfileManagementService(manager, services),
            Mock.Of<IStringLocalizer<CreateOrUpdateIndexProfileStep>>());
        var context = new RecipeExecutionContext
        {
            Name = CreateOrUpdateIndexProfileStep.StepKey,
            Step = JsonNode.Parse("""{"Indexes":[{"Id":"id","Name":"Rejected"}]}""").AsObject(),
        };

        await step.ExecuteAsync(context);

        Assert.Equal("Rejected incoming name.", Assert.Single(context.Errors));
        Assert.Equal("Original", profile.Name);
        Assert.Equal("Original", profile.Properties["Private"]["Value"].GetValue<string>());
        store.Verify(value => value.UpdateAsync(It.IsAny<IndexProfile>()), Times.Never);
    }

    private sealed class TestHandler : IndexProfileHandlerBase
    {
        public override Task UpdatingAsync(UpdatingContext<IndexProfile> context)
        {
            context.Model.Name = context.Data["Name"].GetValue<string>();
            context.Model.Properties["Private"]["Value"] = "Changed";
            return Task.CompletedTask;
        }

        public override Task ValidatingAsync(ValidatingContext<IndexProfile> context)
        {
            if (context.Model.Name == "Rejected")
            {
                context.Result.Fail(new ValidationResult("Rejected incoming name.", [nameof(IndexProfile.Name)]));
            }
            return Task.CompletedTask;
        }
    }
}
