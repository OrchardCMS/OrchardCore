using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Services;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Admin.Controllers;

/// <summary>
/// Describes the cron expression typed in a <c>CronExpressionEditor</c> shape.
/// </summary>
[Admin("CronExpression/{action}", "AdminCronExpression{action}")]
public sealed class CronExpressionController : Controller
{
    private const int Occurrences = 3;

    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteService _siteService;
    private readonly IClock _clock;

    internal readonly IStringLocalizer S;

    public CronExpressionController(
        IAuthorizationService authorizationService,
        ISiteService siteService,
        IClock clock,
        IStringLocalizer<CronExpressionController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _siteService = siteService;
        _clock = clock;
        S = stringLocalizer;
    }

    /// <summary>
    /// Describes a cron expression and gives its next occurrences, with the parser that evaluates the schedules.
    /// </summary>
    /// <param name="expression">The cron expression.</param>
    /// <param name="utc">Whether the expression applies to UTC, and to the time zone of the site otherwise.</param>
    [HttpGet]
    public async Task<IActionResult> Describe(string expression, bool utc = false)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AdminPermissions.AccessAdminPanel))
        {
            return Forbid();
        }

        Response.Headers.CacheControl = "no-store";

        if (!CronExpressionDescriber.IsValid(expression))
        {
            return Json(new
            {
                IsValid = false,
                Error = S["This is not a valid cron expression with five fields: minute, hour, day of the month, month, day of the week."].Value,
            });
        }

        var timeZone = utc ? null : _clock.GetTimeZone((await _siteService.GetSiteSettingsAsync()).TimeZoneId);
        var now = timeZone is null ? _clock.UtcNow : _clock.ConvertToTimeZone(_clock.UtcNow, timeZone).DateTime;

        return Json(new
        {
            IsValid = true,
            Description = CronExpressionDescriber.Describe(expression, CultureInfo.CurrentUICulture),
            Occurrences = CronExpressionDescriber.GetNextOccurrences(expression, now, Occurrences)
                .Select(occurrence => occurrence.ToString("f", CultureInfo.CurrentCulture)),
            TimeZone = timeZone?.TimeZoneId ?? "UTC",
        });
    }
}
