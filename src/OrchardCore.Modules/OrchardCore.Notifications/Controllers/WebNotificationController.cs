using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;
using YesSql;

namespace OrchardCore.Notifications.Controllers;

[Feature("OrchardCore.Notifications.Web")]
[Route("api/web-notifications")]
[ApiController]
public class WebNotificationController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly YesSql.ISession _session;
    private readonly IClock _clock;

    public WebNotificationController(
        IAuthorizationService authorizationService,
        YesSql.ISession session,
        IClock clock)
    {
        _authorizationService = authorizationService;
        _session = session;
        _clock = clock;
    }

    [HttpPost("read"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> Read(ReadWebNotificationViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, WebNotificationPermissions.ManageWebNotifications))
        {
            return Forbid();
        }

        var notificationContentItem = await _session.Query<ContentItem, WebNotificationIndex>(x => x.ContentItemId == viewModel.MessageId && x.UserId == CurrentUserId()).FirstOrDefaultAsync();

        if (notificationContentItem == null)
        {
            return NotFound();
        }

        var updated = false;

        notificationContentItem.Alter<WebNotificationPart>(part =>
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
            messageId = viewModel.MessageId,
            updated,
        });
    }
    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
