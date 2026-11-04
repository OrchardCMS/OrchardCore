using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using OrchardCore.Email;
using OrchardCore.Email.Endpoints;
using OrchardCore.Email.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class EmailDeliveryManagementTests
{
    [Fact]
    public async Task UnauthorizedCallerNeverInvokesDelivery()
    {
        var authorization = Authorization(false);
        var email = new Mock<IEmailService>(MockBehavior.Strict);
        using var services = new ServiceCollection().AddSingleton<IStringLocalizerFactory, NullStringLocalizerFactory>()
            .AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>)).BuildServiceProvider();
        var result = await EmailManagementEndpoints.TestAsync(new DefaultHttpContext { RequestServices = services }, authorization,
            Mock.Of<IEmailAddressValidator>(), email.Object, Message());
        Assert.Equal(403, Assert.IsType<ProblemHttpResult>(result).StatusCode);
    }

    [Theory]
    [InlineData("one@example.com,two@example.com", "Test")]
    [InlineData("one@example.com", "Test\r\nBcc: two@example.com")]
    public async Task InvalidMessageNeverInvokesDelivery(string to, string subject)
    {
        var message = Message();
        message.To = to;
        message.Subject = subject;
        var result = await EmailManagementEndpoints.TestAsync(new DefaultHttpContext(), Authorization(true),
            Validator(), new Mock<IEmailService>(MockBehavior.Strict).Object, message);
        Assert.IsType<ValidationProblem>(result);
    }

    [Fact]
    public async Task DeliveryUsesExistingServiceAndDoesNotEchoProviderExceptions()
    {
        var email = new Mock<IEmailService>();
        email.Setup(service => service.SendAsync(It.IsAny<MailMessage>(), "SMTP", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("sensitive connection and password"));
        var result = await EmailManagementEndpoints.TestAsync(new DefaultHttpContext(), Authorization(true),
            Validator(), email.Object, Message());
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(502, problem.StatusCode);
        Assert.DoesNotContain("sensitive", problem.ProblemDetails.Detail);
        email.Verify(service => service.SendAsync(It.Is<MailMessage>(message => message.To == "one@example.com"
            && message.Subject == "Test" && message.TextBody == "Delivery test"), "SMTP", It.IsAny<CancellationToken>()), Times.Once);
    }

    private static EmailDeliveryTestRequest Message() => new()
    {
        Provider = "SMTP", To = "one@example.com", Subject = "Test", Body = "Delivery test",
    };

    private static IEmailAddressValidator Validator()
    {
        var validator = new Mock<IEmailAddressValidator>();
        validator.Setup(value => value.Validate(It.IsAny<string>())).Returns(true);
        return validator.Object;
    }

    private static IAuthorizationService Authorization(bool allowed)
    {
        var service = new Mock<IAuthorizationService>();
        service.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(allowed ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return service.Object;
    }
}
