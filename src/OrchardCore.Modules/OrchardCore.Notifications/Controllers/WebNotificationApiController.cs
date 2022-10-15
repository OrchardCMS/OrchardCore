using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Models;
using YesSql;

namespace OrchardCore.Notifications.Controllers;

[Feature("OrchardCore.Notifications.Web")]
[Route("api/web-notifications")]
[ApiController]
public class WebNotificationApiController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly YesSql.ISession _session;
    private readonly IClock _clock;

    public WebNotificationApiController(
        IAuthorizationService authorizationService,
        YesSql.ISession session,
        IClock clock)
    {
        _authorizationService = authorizationService;
        _session = session;
        _clock = clock;
    }

    [HttpPost("read")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Read([FromBody] string messageId)
    {
        if (String.IsNullOrEmpty(messageId))
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, WebNotificationPermission.ManageOwnNotifications))
        {
            return Forbid();
        }

        var notificationContentItem = await _session.Query<ContentItem, WebNotificationMessageIndex>(x => x.ContentItemId == messageId && x.UserId == CurrentUserId()).FirstOrDefaultAsync();

        if (notificationContentItem == null)
        {
            return NotFound();
        }

        var updated = false;

        notificationContentItem.Alter<WebNotificationMessagePart>(part =>
        {
            if (!part.IsRead)
            {
                updated = true;

                part.IsRead = true;
                part.ReadAtUtc = _clock.UtcNow;
            }
        });

        if (updated)
        {
            _session.Save(notificationContentItem);
        }

        return Ok(new
        {
            messageId,
            updated,
        });
    }
    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
