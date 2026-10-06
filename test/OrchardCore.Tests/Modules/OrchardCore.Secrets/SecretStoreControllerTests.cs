using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Controllers;
using OrchardCore.Secrets.ViewModels;
using ISecret = OrchardCore.Secrets.ISecret;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SecretStoreControllerTests
{
    private readonly Mock<ISecretManager> _manager = new();
    private readonly Mock<ISecretStoreOperations> _operations = new();
    private readonly Mock<IAuthorizationService> _authorization = new();
    private readonly Mock<ISecretStore> _source = new();
    private readonly StoreController _controller;

    public SecretStoreControllerTests()
    {
        _authorization.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        _source.SetupGet(s => s.Name).Returns("Database");
        _source.Setup(s => s.GetSecretInfosAsync()).ReturnsAsync([]);
        var destination = new Mock<ISecretStore>();
        destination.SetupGet(s => s.Name).Returns("AzureKeyVault");
        _manager.Setup(m => m.GetStores()).Returns([_source.Object, destination.Object]);
        var localizer = new Mock<IStringLocalizer<StoreController>>();
        localizer.Setup(s => s[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        _controller = new StoreController(_manager.Object, _operations.Object, _authorization.Object,
            NullLogger<StoreController>.Instance, localizer.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    [Fact]
    public async Task NoManagePermission_BlocksInspectionAndMutation()
    {
        _authorization.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed());

        Assert.IsType<ForbidResult>(await _controller.Index("Database", null));
        Assert.IsType<ForbidResult>(await _controller.IndexPost(ValidMove()));
        Assert.Empty(_operations.Invocations);
        _source.Verify(s => s.GetSecretInfosAsync(), Times.Never);
    }

    [Fact]
    public async Task MissingConfirmation_DoesNotExecuteAndKeepsValidationError()
    {
        var model = ValidMove();
        model.Confirm = false;

        Assert.IsType<ViewResult>(await _controller.IndexPost(model));

        Assert.False(_controller.ModelState.IsValid);
        Assert.Contains(nameof(model.Confirm), _controller.ModelState.Keys);
        Assert.Empty(_operations.Invocations);
    }

    [Theory]
    [InlineData("Missing", "AzureKeyVault")]
    [InlineData("Database", "Missing")]
    [InlineData("Database", "Database")]
    public async Task InvalidStoreSelection_DoesNotExecute(string source, string destination)
    {
        var model = ValidMove();
        model.SourceStore = source;
        model.DestinationStore = destination;

        await _controller.IndexPost(model);

        Assert.False(_controller.ModelState.IsValid);
        Assert.Empty(_operations.Invocations);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("secret")]
    public async Task Move_UsesSingleOrBulkServerSideOperation(string name)
    {
        _operations.Setup(o => o.MoveSecretAsync("secret", "Database", "AzureKeyVault"))
            .ReturnsAsync(new SecretStoreOperationResult { Name = "secret", Succeeded = true, DestinationSaved = true });
        _operations.Setup(o => o.MoveAllSecretsAsync("Database", "AzureKeyVault")).ReturnsAsync([]);
        var model = ValidMove();
        model.Name = name;

        var result = Assert.IsType<ViewResult>(await _controller.IndexPost(model));

        Assert.False(Assert.IsType<SecretStoreViewModel>(result.Model).Confirm);
        _operations.Verify(o => o.MoveSecretAsync("secret", "Database", "AzureKeyVault"), name == null ? Times.Never : Times.Once);
        _operations.Verify(o => o.MoveAllSecretsAsync("Database", "AzureKeyVault"), name == null ? Times.Once : Times.Never);
        _manager.Verify(m => m.GetSecretAsync<ISecret>(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PartialMoveFailure_IsVisibleAndCleanupIsNotAssumed()
    {
        _source.Setup(s => s.GetSecretInfosAsync()).ReturnsAsync([new SecretInfo { Name = "secret" }]);
        _operations.Setup(o => o.MoveAllSecretsAsync("Database", "AzureKeyVault")).ReturnsAsync(
            [new SecretStoreOperationResult { Name = "secret", DestinationSaved = true, Failure = SecretStoreOperationFailure.SourceRemovalFailed }]);

        var result = Assert.IsType<ViewResult>(await _controller.IndexPost(ValidMove()));
        var model = Assert.IsType<SecretStoreViewModel>(result.Model);

        Assert.False(_controller.ModelState.IsValid);
        Assert.Equal(1, model.ActiveCount);
        Assert.True(Assert.Single(model.Results).DestinationSaved);
    }

    [Fact]
    public async Task FailedInspection_ClearsAnyPostedCleanupStatus()
    {
        _source.Setup(s => s.GetSecretInfosAsync()).ThrowsAsync(new InvalidOperationException("unavailable"));
        var model = ValidMove();
        model.Confirm = false;
        model.ActiveCount = 0;

        var result = Assert.IsType<ViewResult>(await _controller.IndexPost(model));

        Assert.Null(Assert.IsType<SecretStoreViewModel>(result.Model).ActiveCount);
        Assert.False(_controller.ModelState.IsValid);
    }

    private static SecretStoreViewModel ValidMove() => new()
    {
        SourceStore = "Database",
        DestinationStore = "AzureKeyVault",
        Confirm = true,
    };
}
