using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.Email;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

internal static class ControllerExtensions
{
    internal static async Task<bool> SendEmailAsync(this Controller controller, string email, string subject, IShape model)
    {
        var emailService = controller.HttpContext.RequestServices.GetService<IEmailService>();

        if (emailService == null)
        {
            return false;
        }

        var displayHelper = controller.HttpContext.RequestServices.GetRequiredService<IDisplayHelper>();
        var htmlEncoder = controller.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>();
        var body = string.Empty;

        using (var sw = new StringWriter())
        {
            var htmlContent = await displayHelper.ShapeExecuteAsync(model);
            htmlContent.WriteTo(sw, htmlEncoder);
            body = sw.ToString();
        }

        var result = await emailService.SendAsync(email, subject, body);

        return result.Succeeded;
    }

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

    internal static async Task<string> SendEmailConfirmationTokenAsync(this Controller controller, User user, string subject)
    {
        var userManager = controller.ControllerContext.HttpContext.RequestServices.GetRequiredService<UserManager<IUser>>();

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var callbackUrl = controller.Url.Action(nameof(EmailConfirmationController.ConfirmEmail), typeof(EmailConfirmationController).ControllerName(),
            new
            {
                userId = user.UserId,
                code,
            },
            protocol: controller.HttpContext.Request.Scheme);

        await SendEmailAsync(controller, user.Email, subject, new ConfirmEmailViewModel
        {
            User = user,
            ConfirmEmailUrl = callbackUrl,
        });

        return callbackUrl;
    }
}
