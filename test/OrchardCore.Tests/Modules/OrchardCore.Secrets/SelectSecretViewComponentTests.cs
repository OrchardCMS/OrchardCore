using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Providers;
using OrchardCore.Secrets.ViewComponents;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SelectSecretViewComponentTests
{
    [Fact]
    public async Task TypeFilter_MatchesDatabaseAndAzureTypeNames()
    {
        var manager = new Mock<ISecretManager>();
        manager.Setup(m => m.GetSecretInfosAsync()).ReturnsAsync(
            [
                new SecretInfo { Name = "database", Store = "Database", Type = typeof(TextSecret).FullName },
                new SecretInfo { Name = "azure", Store = "AzureKeyVault", Type = nameof(TextSecret) },
                new SecretInfo { Name = "rsa", Store = "Database", Type = typeof(RsaKeySecret).FullName },
            ]);
        var component = new SelectSecretViewComponent(manager.Object,
            [new TextSecretTypeProvider(Mock.Of<IStringLocalizer<TextSecretTypeProvider>>())],
            Mock.Of<IStringLocalizer<SelectSecretViewComponent>>())
        {
            ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                },
            },
        };

        var result = Assert.IsType<ViewViewComponentResult>(await component.InvokeAsync("database", "id", "name", [nameof(TextSecret)], true));
        var model = Assert.IsType<SelectSecretViewModel>(result.ViewData.Model);

        Assert.Equal(["database", "azure"], model.Secrets.Select(s => s.Value));
        Assert.True(model.Secrets[0].Selected);
    }
}
