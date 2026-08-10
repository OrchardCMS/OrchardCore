using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OrchardCore.SignalR;

namespace OrchardCore.Media.Hubs;

[AuthorizeSignalR(AuthenticationSchemes = MediaApiConstants.ApiScheme)]
public sealed class MediaHub : Hub
{
    private readonly IAuthorizationService _authorizationService;

    public MediaHub(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    // The hub attribute only guarantees an authenticated user. Require the same ManageMedia
    // permission the media API endpoints enforce, otherwise any authenticated user could subscribe
    // to the MediaChanged broadcasts (which reveal media paths across all folders).
    public override async Task OnConnectedAsync()
    {
        if (!await _authorizationService.AuthorizeAsync(Context.User, MediaPermissions.ManageMedia))
        {
            Context.Abort();

            return;
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called by the client when it starts viewing a folder. Checks that the caller is authorized
    /// to manage the given path before adding the connection to the corresponding SignalR group.
    /// Using groups (instead of a per-node in-memory tracker) makes authorization work correctly
    /// when a SignalR backplane is configured, because group membership is propagated across nodes.
    /// </summary>
    public async Task SubscribePath(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(Context.User, MediaPermissions.ManageMediaFolder, (object)path))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetFolderGroupName(path));
    }

    /// <summary>
    /// Called by the client when it stops viewing a folder.
    /// </summary>
    public Task UnsubscribePath(string path)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, GetFolderGroupName(path));

    /// <summary>
    /// Returns the SignalR group name used to broadcast changes for <paramref name="folderPath"/>.
    /// </summary>
    internal static string GetFolderGroupName(string folderPath)
        => $"media-path:{folderPath}";
}
