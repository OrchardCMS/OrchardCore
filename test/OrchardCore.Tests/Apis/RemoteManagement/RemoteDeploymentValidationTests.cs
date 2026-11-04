using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Deployment.Remote.Controllers;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Remote.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class RemoteDeploymentValidationTests
{
    [Theory]
    [InlineData("https://example.org/deploy", true)]
    [InlineData("http://localhost/deploy", true)]
    [InlineData("http://127.0.0.1/deploy", true)]
    [InlineData("http://example.org/deploy", false)]
    [InlineData("file:///tmp/deploy", false)]
    [InlineData("https://user:password@example.org/deploy", false)]
    [InlineData("https://example.org/deploy#fragment", false)]
    [InlineData("/relative", false)]
    public void Validate_TransportBoundary_ProtectsCredentials(string url, bool allowed)
    {
        Assert.Equal(allowed, RemoteDeploymentValidation.IsSafeUrl(url));
        Assert.Equal(allowed, RemoteDeploymentValidation.Instance("target", url, "client", "key").Count == 0);
    }

    [Fact]
    public void Validate_MissingOrOversizedValues_RejectsConfiguration()
    {
        Assert.Equal(4, RemoteDeploymentValidation.Instance("", "", "", "").Count);
        Assert.Equal(2, RemoteDeploymentValidation.Client(new string('n', 257), new string('k', 4097)).Count);
    }

    [Fact]
    public void ValidateViewModel_BothControllers_LocalizeClientMessagesInTheContextOfTheValidation()
    {
        var factory = new Mock<IStringLocalizerFactory>();
        var localizer = new Mock<IStringLocalizer>();
        localizer.Setup(value => value[It.IsAny<string>()]).Returns((string name) => new LocalizedString(name, "translated"));
        factory.Setup(value => value.Create(typeof(RemoteDeploymentValidation))).Returns(localizer.Object);

        var clients = new RemoteClientController(Mock.Of<IDataProtectionProvider>(value => value.CreateProtector(It.IsAny<string>()) == Mock.Of<IDataProtector>()), null, null,
            Options.Create(new PagerOptions()), Mock.Of<IShapeFactory>(), Mock.Of<IStringLocalizer<RemoteClientController>>(), Mock.Of<IHtmlLocalizer<RemoteClientController>>(), factory.Object, Mock.Of<INotifier>());
        var instances = new RemoteInstanceController(null, null, Options.Create(new PagerOptions()), Mock.Of<IShapeFactory>(),
            Mock.Of<IStringLocalizer<RemoteInstanceController>>(), Mock.Of<IHtmlLocalizer<RemoteInstanceController>>(), factory.Object, Mock.Of<INotifier>());

        InvokeValidation(clients, new EditRemoteClientViewModel());
        InvokeValidation(instances, new EditRemoteInstanceViewModel());

        Assert.Equal("translated", Assert.Single(clients.ModelState["ClientName"].Errors).ErrorMessage);
        Assert.Equal("translated", Assert.Single(instances.ModelState["ClientName"].Errors).ErrorMessage);
        Assert.Equal("translated", Assert.Single(instances.ModelState["Url"].Errors).ErrorMessage);
    }

    private static void InvokeValidation(object controller, object model)
        => controller.GetType().GetMethod("ValidateViewModel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(controller, [model]);

    [Fact]
    public void HttpClient_Redirects_DisabledForCredentialBearingRequests()
    {
        var services = new ServiceCollection();
        var startup = (global::OrchardCore.Modules.StartupBase)Activator.CreateInstance(typeof(RemoteDeploymentSender).Assembly.GetType("OrchardCore.Deployment.Startup"));
        startup.ConfigureServices(services);
        using var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler(RemoteDeploymentSender.HttpClientName);
        while (handler is DelegatingHandler delegating) { handler = delegating.InnerHandler; }
        Assert.False(Assert.IsType<HttpClientHandler>(handler).AllowAutoRedirect);
    }
}
