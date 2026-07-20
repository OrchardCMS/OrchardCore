using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OrchardCore.Media.Hubs;

[Authorize(AuthenticationSchemes = "Api")]
public class MediaHub : Hub
{
}
