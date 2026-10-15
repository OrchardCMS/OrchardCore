using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Modules;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Filters;
using OrchardCore.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SecretExpirationWarningFilterTests
{
    private static readonly DateTime s_now = new(2026, 10, 3, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("OrchardCore.Admin", "Admin")]
    [InlineData("OrchardCore.AdminDashboard", "Dashboard")]
    public async Task Dashboard_AddsWarningWithExpiredAndExpiringCounts(string area, string controller)
    {
        var manager = new Mock<ISecretManager>();
        manager.Setup(service => service.GetSecretInfosAsync()).ReturnsAsync(
        [
            new SecretInfo { ExpiresUtc = s_now.AddTicks(-1) },
            new SecretInfo { ExpiresUtc = s_now },
            new SecretInfo { ExpiresUtc = s_now.AddDays(30).AddTicks(-1) },
            new SecretInfo { ExpiresUtc = s_now.AddDays(30) },
            new SecretInfo { ExpiresUtc = s_now.AddDays(31) },
            new SecretInfo(),
        ]);
        var layout = CreateLayout();
        var context = CreateContext(area, controller);

        await ExecuteAsync(manager, layout, context);

        var warning = Assert.IsType<Shape>(Assert.Single(Assert.IsType<Shape>(layout.Zones["Messages"]).Items));
        Assert.Equal("SecretsExpirationWarning", warning.Metadata.Type);
        Assert.Equal(1, warning.Properties["ExpiredCount"]);
        Assert.Equal(2, warning.Properties["ExpiringCount"]);
        Assert.Equal(true, warning.Properties["ShowReviewLink"]);
        manager.Verify(service => service.GetSecretInfosAsync(), Times.Once);
        manager.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NoUpcomingExpiration_DoesNotAddWarning(bool hasExpiration)
    {
        var manager = new Mock<ISecretManager>();
        manager.Setup(service => service.GetSecretInfosAsync()).ReturnsAsync(
        [
            new SecretInfo { ExpiresUtc = hasExpiration ? s_now.AddDays(30) : null },
        ]);
        var layout = CreateLayout();

        await ExecuteAsync(manager, layout, CreateContext());

        Assert.Empty(Assert.IsType<Shape>(layout.Zones["Messages"]).Items);
    }

    [Theory]
    [InlineData("frontend")]
    [InlineData("anonymous")]
    [InlineData("unauthorized")]
    [InlineData("json")]
    [InlineData("redirect")]
    [InlineData("secrets")]
    [InlineData("otherAdminPage")]
    public async Task IneligibleRequest_DoesNotReadSecretMetadata(string request)
    {
        var manager = new Mock<ISecretManager>(MockBehavior.Strict);
        var layout = CreateLayout();
        var context = CreateContext();
        if (request == "frontend")
        {
            context.HttpContext.Items.Remove(typeof(AdminAttribute));
        }
        else if (request == "anonymous")
        {
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        }
        else if (request == "json")
        {
            context.Result = new JsonResult(new { });
        }
        else if (request == "redirect")
        {
            context.Result = new RedirectResult("/Admin");
        }
        else if (request == "secrets")
        {
            // The Secrets list renders the warning itself.
            context = CreateContext("OrchardCore.Secrets", "Admin");
        }
        else if (request == "otherAdminPage")
        {
            context = CreateContext("OrchardCore.Features", "Admin", "Features");
        }

        await ExecuteAsync(manager, layout, context, authorized: request != "unauthorized");

        Assert.Empty(Assert.IsType<Shape>(layout.Zones["Messages"]).Items);
        manager.VerifyNoOtherCalls();
    }

    private static ZoneHolding CreateLayout()
    {
        var layout = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));
        layout.Zones["Messages"] = new Shape();
        return layout;
    }

    private static ResultExecutingContext CreateContext(string area = "OrchardCore.Admin", string controller = "Admin", string action = "Index")
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([], "test")),
        };
        AdminAttribute.Apply(httpContext);
        return new ResultExecutingContext(
            new ActionContext(httpContext, new RouteData(new RouteValueDictionary
            {
                ["area"] = area,
                ["controller"] = controller,
                ["action"] = action,
            }), new ActionDescriptor()),
            [],
            new ViewResult(),
            new object());
    }

    private static async Task ExecuteAsync(
        Mock<ISecretManager> manager,
        ZoneHolding layout,
        ResultExecutingContext context,
        bool authorized = true)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.Is<IEnumerable<IAuthorizationRequirement>>(requirements =>
                requirements.OfType<PermissionRequirement>().Any(requirement =>
                    requirement.Permission == SecretsPermissions.ViewSecrets))))
            .ReturnsAsync(authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        var clock = new Mock<IClock>();
        clock.SetupGet(service => service.UtcNow).Returns(s_now);
        var accessor = new Mock<ILayoutAccessor>();
        accessor.Setup(service => service.GetLayoutAsync()).ReturnsAsync(layout);
        var factory = new Mock<IShapeFactory>();
        factory.Setup(service => service.CreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<ValueTask<IShape>>>(),
            It.IsAny<Action<ShapeCreatingContext>>(),
            It.IsAny<Action<ShapeCreatedContext>>()))
            .Returns<string, Func<ValueTask<IShape>>, Action<ShapeCreatingContext>, Action<ShapeCreatedContext>>(
                async (type, create, creating, created) =>
                {
                    var shape = await create();
                    shape.Metadata.Type = type;
                    return shape;
                });
        var filter = new SecretExpirationWarningFilter(
            manager.Object,
            authorization.Object,
            clock.Object,
            accessor.Object,
            factory.Object);
        var nextCalls = 0;

        await filter.OnResultExecutionAsync(context, () =>
        {
            nextCalls++;
            return Task.FromResult(new ResultExecutedContext(context, [], context.Result, context.Controller));
        });

        Assert.Equal(1, nextCalls);
    }
}
