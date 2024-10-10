using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;

namespace OrchardCore.Users.Services;

/// <summary>
/// In version 2.1, the "External Authentication" feature was introduced, necessitating the relocation of all external authentication actions to a new controller.
/// To maintain backward compatibility, this filter was added to automatically redirect requests from the old path to the new path, ensuring existing users were not impacted.
/// 
/// In version 3.0, this filter should be removed. The following note should be included in the 3.0.0 release notes:
/// 
/// ## Breaking Changes
/// ### Login View Update
/// The `ExternalLogin` action has been removed from the `Account` controller.
/// If you are using a custom `Login.cshtml` view or `Login` template, please update the external login form action.
/// As of this update, the `ExternalLogin` action has been relocated to the `ExternalAuthentications` controller.
/// </summary>

internal sealed class ExternalAuthenticationsStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            builder.Use(async (context, next) =>
            {
                if (context.Request.Method == HttpMethods.Post &&
                context.Request.Path.StartsWithSegments("/OrchardCore.Users/Account/ExternalLogin", StringComparison.OrdinalIgnoreCase))
                {
                    context.Request.Path = "/OrchardCore.Users/ExternalAuthentications/ExternalLogin";
                    context.Request.RouteValues["controller"] = typeof(ExternalAuthenticationsController).ControllerName();
                    context.Request.RouteValues["action"] = nameof(ExternalAuthenticationsController.ExternalLogin);
                    context.Request.RouteValues["area"] = "OrchardCore.Users";
                }

                await next();
            });

            next(builder);
        };
    }
}
