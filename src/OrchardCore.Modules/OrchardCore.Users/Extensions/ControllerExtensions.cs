using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Controllers;

internal static class ControllerExtensions
{
    /// <summary>
    /// Returns the created user, otherwise returns null.
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="model"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    internal static async Task<IUser> RegisterUser(this Controller controller, RegisterUserForm model, ILogger logger)
    {
        var registrationEvents = controller.ControllerContext.HttpContext.RequestServices.GetServices<IRegistrationFormEvents>();

        await registrationEvents.InvokeAsync((e, modelState) => e.RegistrationValidationAsync((key, message) => modelState.AddModelError(key, message)), controller.ModelState, logger);

        if (controller.ModelState.IsValid)
        {
            var siteService = controller.ControllerContext.HttpContext.RequestServices.GetRequiredService<ISiteService>();

            var loginSettings = await siteService.GetSettingsAsync<LoginSettings>();

            var registrationOptions = controller.ControllerContext.HttpContext.RequestServices.GetRequiredService<IOptions<RegistrationOptions>>().Value;

            var userService = controller.ControllerContext.HttpContext.RequestServices.GetRequiredService<IUserService>();

            var user = await userService.CreateUserAsync(new User
            {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = !loginSettings.UsersMustValidateEmail,
                IsEnabled = !registrationOptions.UsersAreModerated,
            }, model.Password, controller.ModelState.AddModelError) as User;

            if (user != null && controller.ModelState.IsValid)
            {
                var context = new UserRegisteringContext(user);

                await registrationEvents.InvokeAsync((e, ctx) => e.RegisteringAsync(ctx), context, logger);

                if (!context.Cancel)
                {
                    var signInManager = controller.ControllerContext.HttpContext.RequestServices.GetRequiredService<SignInManager<IUser>>();

                    await signInManager.SignInAsync(user, isPersistent: false);
                }

                logger.LogInformation(3, "User created a new account with password.");

                await registrationEvents.InvokeAsync((e, user) => e.RegisteredAsync(user), user, logger);

                return user;
            }
        }

        return null;
    }
}
