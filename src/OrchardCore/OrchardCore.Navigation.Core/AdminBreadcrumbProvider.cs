using Microsoft.AspNetCore.Http;
using OrchardCore.Admin;

namespace OrchardCore.Navigation;

/// <summary>
/// A base <see cref="IBreadcrumbProvider"/> that only contributes to the trails rendered on the admin. A trail
/// rendered by a front end theme never reaches <see cref="BuildAsync(BreadcrumbBuilder)"/>.
/// </summary>
public abstract class AdminBreadcrumbProvider : IBreadcrumbProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected AdminBreadcrumbProvider(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the context of the admin request the trail is being built for. It is never <see langword="null"/> inside
    /// <see cref="BuildAsync(BreadcrumbBuilder)"/>.
    /// </summary>
    protected HttpContext HttpContext
        => _httpContextAccessor.HttpContext;

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null || !AdminAttribute.IsApplied(httpContext))
        {
            return ValueTask.CompletedTask;
        }

        return BuildAsync(builder);
    }

    /// <summary>
    /// Adds the nodes of the breadcrumb to the builder. It is only called while an admin request is being handled.
    /// </summary>
    /// <param name="builder">The builder collecting the nodes of the breadcrumb being rendered.</param>
    protected abstract ValueTask BuildAsync(BreadcrumbBuilder builder);
}
