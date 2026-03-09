using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OrchardCore.Media.Hubs;

[Authorize]
public class MediaHub : Hub
{
}
