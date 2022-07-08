using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;
using OrchardCore.Email;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers
{
    internal static class ControllerExtensions
    {
        internal static async Task<bool> SendEmailAsync(this Controller controller, string email, string subject, IShape model)
        {
            var smtpService = controller.HttpContext.RequestServices.GetRequiredService<ISmtpService>();
            var displayHelper = controller.HttpContext.RequestServices.GetRequiredService<IDisplayHelper>();
            var htmlEncoder = controller.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>();
            var body = string.Empty;

            using (var sw = new StringWriter())
            {
                var htmlContent = await displayHelper.ShapeExecuteAsync(model);
                htmlContent.WriteTo(sw, htmlEncoder);
                body = sw.ToString();
            }

            var message = new MailMessage()
            {
                To = email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            var result = await smtpService.SendAsync(message);

            return result.Succeeded;
        }

        /// <summary>
        /// Returns the created user, otherwise returns null
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="model"></param>
        /// <param name="confirmationEmailSubject"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        internal static async Task<IUser> RegisterUser(this Controller controller, RegisterViewModel model, string confirmationEmailSubject, ILogger logger)
        {
            var registrationEvents = controller.ControllerContext.HttpContext.RequestServices.GetRequiredService<IEnumerable<IRegistrationFormEvents>>();
            var userService = controller.ControllerContext.HttpContext.RequestServices.GetRequiredService<IUserService>();
            var settings = (await controller.ControllerContext.HttpContext.RequestServices.GetRequiredService<ISiteService>().GetSiteSettingsAsync()).As<RegistrationSettings>();
            var signInManager = controller.ControllerContext.HttpContext.RequestServices.GetRequiredService<SignInManager<IUser>>();

            if (settings.UsersCanRegister != UserRegistrationType.NoRegistration)
            {
                await registrationEvents.InvokeAsync((e, modelState) => e.RegistrationValidationAsync((key, message) => modelState.AddModelError(key, message)), controller.ModelState, logger);

                if (controller.ModelState.IsValid)
                {
                    var user = await userService.CreateUserAsync(new User { UserName = model.UserName, Email = model.Email, EmailConfirmed = !settings.UsersMustValidateEmail, IsEnabled = !settings.UsersAreModerated }, model.Password, (key, message) => controller.ModelState.AddModelError(key, message)) as User;

                    if (user != null && controller.ModelState.IsValid)
                    {
                        if (settings.UsersMustValidateEmail && !user.EmailConfirmed)
                        {
                            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                            // Send an email with this link
                            await controller.SendEmailConfirmationTokenAsync(user, confirmationEmailSubject);
                        }
                        else if (!(settings.UsersAreModerated && !user.IsEnabled))
                        {
                            await signInManager.SignInAsync(user, isPersistent: false);
                        }
                        logger.LogInformation(3, "User created a new account with password.");
                        await registrationEvents.InvokeAsync((e, user) => e.RegisteredAsync(user), user, logger);

                        return user;
                    }
                }
            }
            return null;
        }

        internal static async Task<string> SendEmailConfirmationTokenAsync(this Controller controller, User user, string subject)
        {
            var userManager = controller.ControllerContext.HttpContext.RequestServices.GetRequiredService<UserManager<IUser>>();
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = controller.Url.Action("ConfirmEmail", "Registration", new { userId = user.UserId, code }, protocol: controller.HttpContext.Request.Scheme);
            await SendEmailAsync(controller, user.Email, subject, new ConfirmEmailViewModel() { User = user, ConfirmEmailUrl = callbackUrl });

            return callbackUrl;
        }
    }
}
