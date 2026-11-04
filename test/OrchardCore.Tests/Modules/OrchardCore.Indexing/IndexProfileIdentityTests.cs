using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Drivers;
using OrchardCore.Indexing.Models;
using OrchardCore.Indexing.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexProfileIdentityTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task MissingNames_FailWithoutStoreLookups(string name)
    {
        var store = new Mock<IIndexProfileStore>(MockBehavior.Strict);
        using var services = Services(store.Object);
        var errors = await services.GetRequiredService<IndexProfileIdentityValidator>().ValidateAsync(new IndexProfile { Name = name, IndexName = name });

        Assert.Equal(2, errors.Count);
        store.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("another", 2)]
    [InlineData("id", 0)]
    public async Task BothNames_AllowTheSameProfileAndRejectAnotherProfile(string existingId, int expectedErrors)
    {
        var store = new Mock<IIndexProfileStore>();
        store.Setup(value => value.FindByNameAsync("Taken")).ReturnsAsync(new IndexProfile { Id = existingId });
        store.Setup(value => value.FindByIndexNameAndProviderAsync("taken", "Custom")).ReturnsAsync(new IndexProfile { Id = existingId });
        using var services = Services(store.Object);

        var errors = await services.GetRequiredService<IndexProfileIdentityValidator>().ValidateAsync(Profile());

        Assert.Equal(expectedErrors, errors.Count);
    }

    [Fact]
    public async Task EditorAndDomainHandler_UseSameValidatorWithFieldSpecificErrors()
    {
        var store = new Mock<IIndexProfileStore>();
        store.Setup(value => value.FindByNameAsync("Taken")).ReturnsAsync(new IndexProfile { Id = "other" });
        store.Setup(value => value.FindByIndexNameAndProviderAsync("taken", "Custom")).ReturnsAsync(new IndexProfile { Id = "other" });
        using var services = Services(store.Object);
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(value => value.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<EditIndexProfileViewModel>(), It.IsAny<string>()))
            .Callback((EditIndexProfileViewModel model, string _) => { model.Name = "Taken"; model.IndexName = "taken"; }).ReturnsAsync(true);
        var driver = new IndexProfileDisplayDriver(services.GetRequiredService<IndexProfileIdentityValidator>(), services);
        var profile = Profile();

        await driver.UpdateAsync(profile, new UpdateEditorContext(new Shape(), "", true, "",
            Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object));
        var validation = await services.GetRequiredService<IIndexProfileManager>().ValidateAsync(profile);

        Assert.False(validation.Succeeded);
        Assert.Equal(2, validation.Errors.Count);
        Assert.Equal(validation.Errors.Select(error => error.ErrorMessage).Order(), updater.Object.ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage).Order());
        Assert.Contains(updater.Object.ModelState.Keys, key => key.Split('.').Last() == "Name");
        Assert.Contains(updater.Object.ModelState.Keys, key => key.Split('.').Last() == "IndexName");
    }

    private static IndexProfile Profile() => new()
    {
        Id = "id", Name = "Taken", IndexName = "taken", IndexFullName = "taken", ProviderName = "Custom", Type = "Content",
    };

    private static ServiceProvider Services(IIndexProfileStore store)
    {
        var services = new ServiceCollection().AddLogging().AddLocalization();
        services.AddSingleton(store);
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton(Mock.Of<IClock>());
        services.Configure<IndexingOptions>(options => options.AddIndexingSource("Custom", "Content"));
        services.AddIndexingCore();
        return services.BuildServiceProvider();
    }
}
