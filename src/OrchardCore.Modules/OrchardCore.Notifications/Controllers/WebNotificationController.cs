using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Modules;
using OrchardCore.Notifications.Indexes;
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

        var notification = await _session.Query<WebNotification, WebNotificationIndex>(x => x.ContentItemId == viewModel.MessageId && x.UserId == CurrentUserId(), collection: NotificationConstants.NotificationCollection).FirstOrDefaultAsync();

        if (notification == null)
        {
            return NotFound();
        }

        var updated = false;

        if (!notification.IsRead)
        {
            notification.ReadAtUtc = _clock.UtcNow;
            notification.IsRead = true;

            updated = true;
            _session.Save(notification, collection: NotificationConstants.NotificationCollection);
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
