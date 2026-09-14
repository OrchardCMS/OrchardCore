using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Navigation;

/// <summary>
/// Builds the nodes of a breadcrumb trail out of every <see cref="IBreadcrumbProvider"/> registered on the tenant.
/// </summary>
public interface IBreadcrumbManager
{
    /// <summary>
    /// Builds the ordered nodes of the breadcrumb of the given name.
    /// </summary>
    /// <param name="name">The name of the breadcrumb. e.g., <c>Contents.Edit</c>.</param>
    /// <param name="actionContext">The context of the request, used to generate the url of each node.</param>
    /// <param name="data">The optional contextual data of the page rendering the breadcrumb.</param>
    /// <returns>
    /// The nodes of the trail, ordered by their position. The last node is the current one: it is never a link.
    /// </returns>
    Task<IList<BreadcrumbItem>> BuildBreadcrumbAsync(string name, ActionContext actionContext, IReadOnlyDictionary<string, object> data = null);
}
