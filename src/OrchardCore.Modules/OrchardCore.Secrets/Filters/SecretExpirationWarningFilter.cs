using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Modules;

namespace OrchardCore.Secrets.Filters;

/// <summary>
/// Warns the users who manage secrets about expired and expiring secrets on the admin dashboard. The Secrets list
/// renders the same warning itself.
/// </summary>
public sealed class SecretExpirationWarningFilter : IAsyncResultFilter
{

    private readonly ISecretManager _secretManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IClock _clock;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IShapeFactory _shapeFactory;

    public SecretExpirationWarningFilter(
        ISecretManager secretManager,
        IAuthorizationService authorizationService,
        IClock clock,
        ILayoutAccessor layoutAccessor,
        IShapeFactory shapeFactory)
    {
        _secretManager = secretManager;
        _authorizationService = authorizationService;
        _clock = clock;
        _layoutAccessor = layoutAccessor;
        _shapeFactory = shapeFactory;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.IsViewOrPageResult() &&
            AdminAttribute.IsApplied(context.HttpContext) &&
            IsDashboard(context) &&
            context.HttpContext.User.Identity?.IsAuthenticated == true &&
            await _authorizationService.AuthorizeAsync(context.HttpContext.User, SecretsPermissions.ManageSecrets))
        {
            var now = _clock.UtcNow;
            var warningThreshold = now.AddDays(30);
            var expiredCount = 0;
            var expiringCount = 0;

            foreach (var secret in await _secretManager.GetSecretInfosAsync())
            {
                if (secret.ExpiresUtc is not DateTime expiration)
                {
                    continue;
                }

                if (expiration < now)
                {
                    expiredCount++;
                }
                else if (expiration < warningThreshold)
                {
                    expiringCount++;
                }
            }

            if (expiredCount > 0 || expiringCount > 0)
            {
                var warning = await _shapeFactory.CreateAsync("SecretsExpirationWarning", () =>
                {
                    var shape = new Shape();
                    shape.Properties["ExpiredCount"] = expiredCount;
                    shape.Properties["ExpiringCount"] = expiringCount;
                    shape.Properties["ShowReviewLink"] = true;
                    return ValueTask.FromResult<IShape>(shape);
                });
                var layout = await _layoutAccessor.GetLayoutAsync();
                await layout.Zones["Messages"].AddAsync(warning);
            }
        }

        await next();
    }

    // The admin root is served by the Admin module, or by the Admin Dashboard module when it is enabled.
    private static bool IsDashboard(ResultExecutingContext context)
    {
        var values = context.RouteData.Values;

        return IsRoute(values, "OrchardCore.Admin", "Admin", "Index") ||
            IsRoute(values, "OrchardCore.AdminDashboard", "Dashboard", "Index");
    }

    private static bool IsRoute(RouteValueDictionary values, string area, string controller, string action)
        => string.Equals(values["area"] as string, area, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(values["controller"] as string, controller, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(values["action"] as string, action, StringComparison.OrdinalIgnoreCase);
}
