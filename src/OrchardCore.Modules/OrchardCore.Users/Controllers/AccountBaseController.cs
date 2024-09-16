using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Controllers;

public abstract class AccountBaseController : Controller
{
    protected async Task<IActionResult> LoggedInActionResultAsync(IUser user, string returnUrl = null, ExternalLoginInfo info = null)
    {
        var workflowManager = HttpContext.RequestServices.GetService<IWorkflowManager>();
        if (workflowManager != null && user is User u)
        {
            var input = new Dictionary<string, object>
            {
                ["UserName"] = user.UserName,
                ["ExternalClaims"] = info?.Principal?.GetSerializableClaims() ?? [],
                ["Roles"] = u.RoleNames,
                ["Provider"] = info?.LoginProvider
            };
            await workflowManager.TriggerEventAsync(nameof(Workflows.Activities.UserLoggedInEvent),
                input: input, correlationId: u.UserId);
        }

        return RedirectToLocal(returnUrl);
    }

    protected IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl.ToUriComponents());
        }

        return Redirect("~/");
    }

    protected void CopyTempDataErrorsToModelState()
    {
        foreach (var errorMessage in TempData.Where(x => x.Key.StartsWith("error")).Select(x => x.Value.ToString()))
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }
    }

    protected bool AddUserEnabledError(IUser user, IStringLocalizer S)
    {
        if (user is not User localUser || !localUser.IsEnabled)
        {
            ModelState.AddModelError(string.Empty, S["The specified user is not allowed to sign in."]);

            return true;
        }

        return false;
    }
}
