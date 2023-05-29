using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;
using YesSql;

namespace OrchardCore.Notifications.Controllers;

[Feature("OrchardCore.Notifications")]
[Route("api/notifications")]
[ApiController]
public class ApiController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly YesSql.ISession _session;
    private readonly IClock _clock;

    public ApiController(
        IAuthorizationService authorizationService,
        YesSql.ISession session,
        IClock clock)
    {
        _authorizationService = authorizationService;
        _session = session;
        _clock = clock;
    }

    [HttpPost("read"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> Read(ReadNotificationViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, NotificationPermissions.ManageNotifications))
        {
            return Forbid();
        }

        var notification = await _session.Query<Notification, NotificationIndex>(x => x.NotificationId == viewModel.MessageId && x.UserId == CurrentUserId(), collection: NotificationConstants.NotificationCollection).FirstOrDefaultAsync();

        if (notification == null)
        {
            return NotFound();
        }

        var updated = false;
        var readInfo = notification.As<NotificationReadInfo>();
        if (!readInfo.IsRead)
        {
            readInfo.ReadAtUtc = _clock.UtcNow;
            readInfo.IsRead = true;
            notification.Put(readInfo);

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
