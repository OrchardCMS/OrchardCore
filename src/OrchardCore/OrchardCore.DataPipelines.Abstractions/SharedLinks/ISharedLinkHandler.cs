using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace OrchardCore.DataPipelines.SharedLinks;

/// <summary>
/// Opens the links to a kind of resource, such as files or reports, once <see cref="ISharedLinkManager"/> checked that
/// the user may use them, and releases the resource when a link is deleted. Registered with
/// <see cref="SharedLinksServiceCollectionExtensions.AddSharedLinkHandler{THandler}"/>.
/// </summary>
public interface ISharedLinkHandler
{
    /// <summary>
    /// Gets the kind of resource the handler opens, such as <c>DataPipelines.File</c>. It is stored with each link.
    /// </summary>
    string Kind { get; }

    /// <summary>
    /// Gets the name of the kind of resource, shown in the admin.
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// Returns the resource of a link, such as the content of a file, or a redirect to a report.
    /// </summary>
    /// <param name="link">The link, which the user may use.</param>
    /// <param name="httpContext">The request.</param>
    /// <returns>The result to return to the user.</returns>
    Task<IActionResult> OpenAsync(SharedLink link, HttpContext httpContext);

    /// <summary>
    /// Returns the URL of an admin page about what shared the resource of a link, such as the run of a pipeline, shown in
    /// the list of the shared links.
    /// </summary>
    /// <param name="link">The link.</param>
    /// <param name="urlHelper">The URL helper of the admin page.</param>
    /// <returns>The URL, or <see langword="null"/>.</returns>
    string GetSourceUrl(SharedLink link, IUrlHelper urlHelper)
    {
        return null;
    }

    /// <summary>
    /// Releases the resource of a link that is deleted, such as the copy of a file.
    /// </summary>
    /// <param name="link">The link.</param>
    /// <returns>A task that completes when the resource is released.</returns>
    Task DeletedAsync(SharedLink link)
    {
        return Task.CompletedTask;
    }
}
