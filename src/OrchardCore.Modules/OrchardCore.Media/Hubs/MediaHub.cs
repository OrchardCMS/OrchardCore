using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OrchardCore.Media.Hubs;

[Authorize(Policy = MediaApiConstants.AuthorizationPolicyName)]
public class MediaHub : Hub
{
    private readonly IAuthorizationService _authorizationService;

    public MediaHub(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    // The [Authorize] policy only guarantees an authenticated user. Require the same ManageMedia
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
}
