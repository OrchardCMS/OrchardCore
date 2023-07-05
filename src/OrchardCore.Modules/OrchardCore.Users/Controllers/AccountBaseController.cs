using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Controllers;

public class AccountBaseController : Controller
{
    protected async Task<IActionResult> LoggedInActionResultAsync(IUser user, string returnUrl = null, ExternalLoginInfo info = null)
    {
        var workflowManager = HttpContext.RequestServices.GetService<IWorkflowManager>();
        if (workflowManager != null && user is User u)
        {
            var input = new Dictionary<string, object>
            {
                ["UserName"] = user.UserName,
                ["ExternalClaims"] = info?.Principal?.GetSerializableClaims() ?? Enumerable.Empty<SerializableClaim>(),
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
}
