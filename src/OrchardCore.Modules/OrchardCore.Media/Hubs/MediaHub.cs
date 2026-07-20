using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OrchardCore.Media.Hubs;

[Authorize(Policy = MediaApiConstants.AuthorizationPolicyName)]
public class MediaHub : Hub
{
}
