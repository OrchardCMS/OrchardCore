using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Controllers;
using OrchardCore.Secrets.Providers;
using OrchardCore.Secrets.ViewModels;
using ISecret = OrchardCore.Secrets.ISecret;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SecretAdminControllerTests
{
    private readonly Mock<ISecretManager> _manager = new();
    private readonly AdminController _controller;
    private readonly Mock<IStringLocalizer<AdminController>> _localizer = new();

    public SecretAdminControllerTests()
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        var database = new Mock<ISecretStore>();
        database.SetupGet(s => s.Name).Returns("Database");
        var azure = new Mock<ISecretStore>();
        azure.SetupGet(s => s.Name).Returns("AzureKeyVault");
        _manager.Setup(m => m.GetStores()).Returns([database.Object, azure.Object]);
        _manager.Setup(m => m.GetSecretInfosAsync()).ReturnsAsync(
            [new SecretInfo { Name = "secret", Store = "AzureKeyVault", Type = nameof(TextSecret), Description = "description" }]);
        _manager.Setup(m => m.GetSecretAsync<ISecret>("secret", "AzureKeyVault")).ReturnsAsync(new TextSecret { Text = "old" });
        _controller = new AdminController(_manager.Object, [new TextSecretTypeProvider(Mock.Of<IStringLocalizer<TextSecretTypeProvider>>())],
            authorization.Object, Mock.Of<INotifier>(), Mock.Of<IShapeFactory>(), Options.Create(new PagerOptions()),
            _localizer.Object, Mock.Of<IHtmlLocalizer<AdminController>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    [Fact]
    public async Task Edit_ReadsSelectedStoreAndDescription()
    {
        var result = Assert.IsType<ViewResult>(await _controller.Edit("secret", "AzureKeyVault"));
        var model = Assert.IsType<SecretEditViewModel>(result.Model);
        Assert.Equal("AzureKeyVault", model.Store);
        Assert.Equal("description", model.Description);
        _manager.Verify(m => m.GetSecretAsync<ISecret>("secret"), Times.Never);
    }

    [Fact]
    public async Task Edit_CannotChangeStore()
    {
        var result = await _controller.EditPost(new SecretEditViewModel
        {
            Name = "secret",
            Store = "Database",
            SecretType = nameof(TextSecret),
            TextValue = "new",
        }, "secret", "AzureKeyVault");

        Assert.IsType<BadRequestResult>(result);
        _manager.Verify(m => m.SaveSecretAsync(It.IsAny<string>(), It.IsAny<ISecret>(), It.IsAny<string>(), It.IsAny<SecretSaveOptions>()), Times.Never);
    }

    [Fact]
    public async Task Edit_MissingSecretCannotBeRecreated()
    {
        Assert.IsType<BadRequestResult>(await _controller.EditPost(new SecretEditViewModel
        {
            Name = "missing",
            Store = "Database",
            SecretType = nameof(TextSecret),
        }, "missing", "Database"));
    }

    [Fact]
    public async Task Delete_TargetsOnlySelectedStore()
    {
        await _controller.Delete("secret", "AzureKeyVault");
        _manager.Verify(m => m.RemoveSecretAsync("secret", "AzureKeyVault"), Times.Once);
        _manager.Verify(m => m.RemoveSecretAsync("secret"), Times.Never);
    }

    [Fact]
    public async Task DuplicateCheck_UsesMetadataRatherThanDecryptingValues()
    {
        _localizer.Setup(s => s[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        var result = await _controller.CreatePost(new SecretEditViewModel
        {
            Name = "secret",
            Store = "Database",
            SecretType = nameof(TextSecret),
            TextValue = "new",
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        _manager.Verify(m => m.GetSecretAsync<ISecret>(It.IsAny<string>()), Times.Never);
    }
}
