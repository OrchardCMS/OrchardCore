using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Indexing.Controllers;
using OrchardCore.Indexing.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexProfileManagementTests
{
    [Theory]
    [InlineData(true, true, IndexProfileManagementResult.Success)]
    [InlineData(false, true, IndexProfileManagementResult.ProviderRejected)]
    [InlineData(false, false, IndexProfileManagementResult.LocalDeleteFailed)]
    public async Task Create_OrdersLocalProviderCompensationAndScheduling(bool accepted, bool removed, IndexProfileManagementResult expected)
    {
        var calls = new List<string>();
        var profile = Profile();
        var profiles = Profiles(profile);
        profiles.Setup(value => value.CreateAsync(profile)).Callback(() => calls.Add("local-create")).Returns(ValueTask.CompletedTask);
        profiles.Setup(value => value.DeleteAsync(profile)).Callback(() => calls.Add("local-delete")).ReturnsAsync(removed);
        profiles.Setup(value => value.SynchronizeAsync(profile)).Callback(() => calls.Add("schedule")).Returns(ValueTask.CompletedTask);
        var provider = new Mock<IIndexManager>();
        provider.Setup(value => value.CreateAsync(profile)).Callback(() => calls.Add("provider-create")).ReturnsAsync(accepted);
        using var services = Services(provider.Object);

        var result = await new IndexProfileManagementService(profiles.Object, services).CreateAsync(profile);

        Assert.Equal(expected, result);
        Assert.Equal(accepted ? ["local-create", "provider-create", "schedule"] : new[] { "local-create", "provider-create", "local-delete" }, calls);
    }

    [Fact]
    public async Task Create_ProviderException_PreservesLocalProfileForUncertainOutcome()
    {
        var profile = Profile();
        var profiles = Profiles(profile);
        var provider = new Mock<IIndexManager>();
        provider.Setup(value => value.CreateAsync(profile)).ThrowsAsync(new InvalidOperationException("Provider outcome unknown."));
        using var services = Services(provider.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => new IndexProfileManagementService(profiles.Object, services).CreateAsync(profile));

        profiles.Verify(value => value.CreateAsync(profile), Times.Once);
        profiles.Verify(value => value.DeleteAsync(profile), Times.Never);
        profiles.Verify(value => value.SynchronizeAsync(profile), Times.Never);
    }

    [Fact]
    public async Task Create_InvalidProfile_DoesNotMutateEitherStore()
    {
        var profile = Profile();
        var profiles = Profiles(profile);
        var validation = new ValidationResultDetails();
        validation.Fail(new ValidationResult("Invalid definition."));
        profiles.Setup(value => value.ValidateAsync(profile)).ReturnsAsync(validation);
        var provider = new Mock<IIndexManager>(MockBehavior.Strict);
        using var services = Services(provider.Object);

        await Assert.ThrowsAsync<IndexProfileValidationException>(() => new IndexProfileManagementService(profiles.Object, services).CreateAsync(profile));

        profiles.Verify(value => value.CreateAsync(profile), Times.Never);
        provider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MissingProvider_DoesNotCreateOrDeleteLocally_UnlessForced()
    {
        var profile = Profile();
        var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        using var services = new ServiceCollection().BuildServiceProvider();
        var management = new IndexProfileManagementService(profiles.Object, services);

        Assert.Equal(IndexProfileManagementResult.ProviderUnavailable, await management.CreateAsync(profile));
        Assert.Equal(IndexProfileManagementResult.ProviderUnavailable, await management.DeleteAsync(profile));
        profiles.VerifyNoOtherCalls();

        profiles.Setup(value => value.DeleteAsync(profile)).ReturnsAsync(true);
        Assert.Equal(IndexProfileManagementResult.Success, await management.DeleteAsync(profile, force: true));
    }

    [Theory]
    [InlineData(true, true, true, IndexProfileManagementResult.Success)]
    [InlineData(true, false, true, IndexProfileManagementResult.ProviderRejected)]
    [InlineData(false, false, true, IndexProfileManagementResult.Success)]
    [InlineData(true, true, false, IndexProfileManagementResult.LocalDeleteFailed)]
    public async Task Delete_RemovesLocalOnlyAfterProviderIsAbsentOrDeleted(bool exists, bool accepted, bool removed, IndexProfileManagementResult expected)
    {
        var calls = new List<string>();
        var profile = Profile();
        var profiles = new Mock<IIndexProfileManager>();
        profiles.Setup(value => value.DeleteAsync(profile)).Callback(() => calls.Add("local-delete")).ReturnsAsync(removed);
        var provider = new Mock<IIndexManager>();
        provider.Setup(value => value.ExistsAsync(profile.IndexFullName)).ReturnsAsync(exists);
        provider.Setup(value => value.DeleteAsync(profile)).Callback(() => calls.Add("provider-delete")).ReturnsAsync(accepted);
        using var services = Services(provider.Object);

        Assert.Equal(expected, await new IndexProfileManagementService(profiles.Object, services).DeleteAsync(profile));

        Assert.Equal(exists ? (accepted ? new[] { "provider-delete", "local-delete" } : ["provider-delete"]) : ["local-delete"], calls);
    }

    [Fact]
    public async Task Recipe_Create_UsesSharedProviderCoordination()
    {
        var profile = Profile();
        var profiles = Profiles(profile);
        profiles.Setup(value => value.NewAsync("Lucene", "Content", It.IsAny<JsonNode>())).ReturnsAsync(profile);
        profiles.Setup(value => value.DeleteAsync(profile)).ReturnsAsync(true);
        var provider = new Mock<IIndexManager>();
        provider.Setup(value => value.CreateAsync(profile)).ReturnsAsync(false);
        using var services = Services(provider.Object);
        var options = new IndexingOptions();
        options.AddIndexingSource("Lucene", "Content");
        var localizer = new Mock<IStringLocalizer<CreateOrUpdateIndexProfileStep>>();
        localizer.Setup(value => value[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string message, object[] arguments) => new LocalizedString(message, string.Format(message, arguments)));
        var recipe = new CreateOrUpdateIndexProfileStep(profiles.Object, Options.Create(options),
            new IndexProfileManagementService(profiles.Object, services), localizer.Object);
        var context = new RecipeExecutionContext
        {
            Name = CreateOrUpdateIndexProfileStep.StepKey,
            Step = JsonNode.Parse("""{"Indexes":[{"ProviderName":"Lucene","Type":"Content","IndexName":"articles"}]}""").AsObject(),
        };

        await recipe.ExecuteAsync(context);

        Assert.Single(context.Errors);
        profiles.Verify(value => value.CreateAsync(profile), Times.Once);
        profiles.Verify(value => value.DeleteAsync(profile), Times.Once);
        profiles.Verify(value => value.SynchronizeAsync(profile), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Admin_Delete_UsesCoordinatorAndPreservesForceChoice(bool force)
    {
        var profile = Profile();
        var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        profiles.Setup(value => value.FindByIdAsync("id")).ReturnsAsync(profile);
        var management = new Mock<IIndexProfileManagementService>(MockBehavior.Strict);
        management.Setup(value => value.DeleteAsync(profile, force)).ReturnsAsync(IndexProfileManagementResult.Success);
        var controller = Controller(profiles.Object, management.Object);

        Assert.IsType<RedirectToActionResult>(await controller.Delete("id", force));

        management.Verify(value => value.DeleteAsync(profile, force), Times.Once);
        profiles.Verify(value => value.FindByIdAsync("id"), Times.Once);
        profiles.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Admin_BulkDelete_UsesCoordinatorAndSkipsMissingProfiles()
    {
        var profile = Profile();
        var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        profiles.Setup(value => value.FindByIdAsync("id")).ReturnsAsync(profile);
        profiles.Setup(value => value.FindByIdAsync("missing")).ReturnsAsync((IndexProfile)null);
        var management = new Mock<IIndexProfileManagementService>(MockBehavior.Strict);
        management.Setup(value => value.DeleteAsync(profile, false)).ReturnsAsync(IndexProfileManagementResult.Success);

        await Controller(profiles.Object, management.Object).IndexPost(new IndexingEntityOptions { BulkAction = IndexingEntityAction.Remove }, ["id", "missing"]);

        management.Verify(value => value.DeleteAsync(profile, false), Times.Once);
        management.VerifyNoOtherCalls();
    }

    private static AdminController Controller(IIndexProfileManager profiles, IIndexProfileManagementService management)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        return new AdminController(authorization.Object, null, profiles, management, null, null, Options.Create(new IndexingOptions()),
            Mock.Of<INotifier>(), Mock.Of<IHtmlLocalizer<AdminController>>(), Mock.Of<IStringLocalizer<AdminController>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    private static IndexProfile Profile() => new() { Id = "id", ProviderName = "Lucene", Type = "Content", IndexFullName = "tenant_articles" };

    private static Mock<IIndexProfileManager> Profiles(IndexProfile profile)
    {
        var profiles = new Mock<IIndexProfileManager>();
        profiles.Setup(value => value.ValidateAsync(profile)).ReturnsAsync(new ValidationResultDetails());
        return profiles;
    }

    private static ServiceProvider Services(IIndexManager provider) => new ServiceCollection()
        .AddKeyedSingleton("Lucene", provider).BuildServiceProvider();
}
