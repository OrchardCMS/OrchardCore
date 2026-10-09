using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Modules;

namespace OrchardCore.Secrets.Filters;

public sealed class SecretExpirationWarningFilter : IAsyncResultFilter
{
    private const string SecretsArea = "OrchardCore.Secrets";

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
            !IsSecretsPage(context) &&
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

    // The Secrets pages show the warning themselves, where it is relevant.
    private static bool IsSecretsPage(ResultExecutingContext context)
        => context.RouteData.Values.TryGetValue("area", out var area) &&
            string.Equals(area as string, SecretsArea, StringComparison.OrdinalIgnoreCase);
}
