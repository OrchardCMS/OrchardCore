using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;

namespace OrchardCore.Users.Handlers;

/// <summary>
/// Redirects a user whose email address is not confirmed to the confirmation notice.
/// Login flows that have verified the user's password use this result type to know that they can
/// let the user request a new confirmation email.
/// </summary>
internal sealed class EmailConfirmationRequiredResult : RedirectToActionResult
{
    public EmailConfirmationRequiredResult(RouteValueDictionary routeValues)
        : base(
            nameof(EmailConfirmationController.ConfirmEmailSent),
            typeof(EmailConfirmationController).ControllerName(),
            routeValues)
    {
    }
}
