using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Controllers;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.RemoteManagement.Tests;

public class ConfigurationAuthorizationTests
{
    [Fact]
    public async Task McpConfigure_WithoutConfigurationPermission_DoesNotChangeApplications()
    {
        var authorization = CreateDeniedAuthorization();
        var manager = new Mock<IOpenIdApplicationManager>(MockBehavior.Strict);
        var tenant = new Mock<IRemoteManagementTenantConfigurationService>(MockBehavior.Strict);
        var controller = new RemoteManagementMcpController(authorization.Object,
            new RemoteManagementMcpConfigurationService(manager.Object, tenant.Object),
            Mock.Of<IShellHost>(), new ShellSettings(), Mock.Of<INotifier>(), Mock.Of<IHtmlLocalizer<RemoteManagementMcpController>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };

        Assert.IsType<ForbidResult>(await controller.Configure(new RemoteManagementMcpViewModel { RedirectUris = "https://client.example/callback" }));
        Assert.IsType<ForbidResult>(await controller.ConfigureAutomatic(tenant.Object));
        manager.VerifyNoOtherCalls();
        tenant.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CliConfigure_WithoutConfigurationPermission_DoesNotChangeApplications()
    {
        var authorization = CreateDeniedAuthorization();
        var cli = new Mock<IRemoteManagementCliConfigurationService>(MockBehavior.Strict);
        var tenant = new Mock<IRemoteManagementTenantConfigurationService>(MockBehavior.Strict);
        var controller = new RemoteManagementCliController(authorization.Object, cli.Object, tenant.Object,
            Mock.Of<IShellHost>(), new ShellSettings(), Mock.Of<INotifier>(), Mock.Of<IHtmlLocalizer<RemoteManagementCliController>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };

        Assert.IsType<ForbidResult>(await controller.Configure());
        cli.VerifyNoOtherCalls();
        tenant.VerifyNoOtherCalls();
    }

    private static Mock<IAuthorizationService> CreateDeniedAuthorization()
    {
        var service = new Mock<IAuthorizationService>();
        service.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed());
        return service;
    }
}
