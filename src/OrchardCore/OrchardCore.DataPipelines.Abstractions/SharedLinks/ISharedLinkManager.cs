using System.Security.Claims;

namespace OrchardCore.DataPipelines.SharedLinks;

/// <summary>
/// Shares resources with users of the site through links: creates the links, decides who may use them, and revokes
/// and deletes them. A module shares a kind of resource by registering an <see cref="ISharedLinkHandler"/> for it.
/// </summary>
public interface ISharedLinkManager
{
    /// <summary>
    /// Creates a link to a resource for its recipients, and emails it to them when asked to.
    /// </summary>
    /// <param name="request">The resource, who to share it with, and for how long.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The link and its URL. The URL holds the token, which is never stored: it is only sent to the recipients.</returns>
    Task<SharedLinkResult> CreateAsync(SharedLinkRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a link.
    /// </summary>
    /// <param name="linkId">The identifier of the link.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The link, or <see langword="null"/>.</returns>
    Task<SharedLink> GetAsync(string linkId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks that a user may use a link and, when they may, counts the use.
    /// </summary>
    /// <param name="linkId">The identifier of the link.</param>
    /// <param name="token">The token of the link, from its URL.</param>
    /// <param name="user">The signed-in user.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>Whether the user may use the link, or why not, and the link when it exists.</returns>
    Task<(SharedLinkAccessResult Result, SharedLink Link)> UseAsync(string linkId, string token, ClaimsPrincipal user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops a link from working. The resource is kept until the link is deleted.
    /// </summary>
    /// <param name="link">The link.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the link is revoked.</returns>
    Task RevokeAsync(SharedLink link, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a link, and lets its handler release the resource.
    /// </summary>
    /// <param name="link">The link.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the link is deleted.</returns>
    Task DeleteAsync(SharedLink link, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the links whose title, or the name of what shared them, contains a text, the most recent first.
    /// </summary>
    /// <param name="search">The text, or <see langword="null"/> for every link.</param>
    /// <param name="skip">The number of links to skip.</param>
    /// <param name="take">The number of links to return.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The links, and the number of links that match.</returns>
    Task<(IReadOnlyList<SharedLink> Links, int Count)> ListAsync(string search, int skip, int take, CancellationToken cancellationToken = default);
}
