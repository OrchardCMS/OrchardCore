using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.Remote.Controllers;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Remote.ViewModels;
using OrchardCore.Deployment.Services;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.FileStorage;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class RemoteDeploymentImportTests
{
    [Fact]
    public async Task Import_ExecutionThrows_ReportsFailureAndCleansStaging()
    {
        using var site = new SiteContext();
        await site.InitializeAsync();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var clients = new RemoteClientService(scope.ServiceProvider.GetRequiredService<global::YesSql.ISession>(),
                scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>());
            await clients.CreateRemoteClientAsync("probe", "private-probe-key");
            var manager = new Mock<IDeploymentManager>();
            manager.Setup(value => value.ImportDeploymentPackageAsync(It.IsAny<IFileProvider>())).ThrowsAsync(new InvalidOperationException("probe"));
            var root = Directory.CreateTempSubdirectory("deployment-import-test-").FullName;
            try
            {
                var temporary = new Mock<ITempDirectoryProvider>();
                temporary.Setup(value => value.GetRootDirectory()).Returns(root);
                temporary.Setup(value => value.CreateTempSubdirectory(It.IsAny<string>())).Returns(() => Directory.CreateDirectory(Path.Combine(root, Guid.NewGuid().ToString("n"))).FullName);
                var packages = new DeploymentPackageService(temporary.Object, Options.Create(new DeploymentPackageOptions()));
                var controller = new ImportRemoteInstanceController(clients, manager.Object, new FileCreationService([]), packages,
                    Mock.Of<INotifier>(), Mock.Of<IHtmlLocalizer<ImportRemoteInstanceController>>(),
                    Mock.Of<IStringLocalizer<ImportRemoteInstanceController>>(), NullLogger<ImportRemoteInstanceController>.Instance)
                {
                    ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
                };
                var bytes = Encoding.UTF8.GetBytes("{\"steps\":[]}");
                using var input = new MemoryStream(bytes);
                var result = await controller.Import(new ImportViewModel
                {
                    ClientName = "probe", ApiKey = "private-probe-key",
                    Content = new FormFile(input, 0, bytes.Length, "Content", "Recipe.json") { Headers = new HeaderDictionary(), ContentType = "application/json" },
                });
                Assert.Equal(500, Assert.IsType<StatusCodeResult>(result).StatusCode);
                manager.Verify(value => value.ImportDeploymentPackageAsync(It.IsAny<IFileProvider>()), Times.Once);
                Assert.Empty(Directory.EnumerateFileSystemEntries(root));
            }
            finally { Directory.Delete(root, true); }
        });
    }
}
