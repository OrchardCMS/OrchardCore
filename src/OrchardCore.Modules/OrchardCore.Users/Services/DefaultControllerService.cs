using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;
using OrchardCore.Email;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Services
{
    public class DefaultControllerService
    {
        private readonly ISmtpService _smtpService;
        private readonly IDisplayHelper _displayHelper;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IEnumerable<IRegistrationFormEvents> _registrationFormEvents;
        private readonly IUserService _userService;
        private readonly ISiteService _siteService;
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;
        private readonly ILogger<DefaultControllerService> _logger;

        public DefaultControllerService(
            ISmtpService smtpService,
            IDisplayHelper displayHelper,
            HtmlEncoder htmlEncoder,
            IEnumerable<IRegistrationFormEvents> registrationFormEvents,
            IUserService userService,
            ISiteService siteService,
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            ILogger<DefaultControllerService> logger)
        {
            _smtpService = smtpService;
            _displayHelper = displayHelper;
            _htmlEncoder = htmlEncoder;
            _registrationFormEvents = registrationFormEvents;
            _userService = userService;
            _siteService = siteService;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Returns the created user, otherwise returns null
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="model"></param>
        /// <param name="confirmationEmailSubject"></param>
        /// <returns></returns>
        public virtual async Task<IUser> RegisterUser(Controller controller, RegisterViewModel model, string confirmationEmailSubject)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();

            if (settings.UsersCanRegister != UserRegistrationType.NoRegistration)
            {
                await _registrationFormEvents.InvokeAsync((e, modelState) => e.RegistrationValidationAsync((key, message) => modelState.AddModelError(key, message)), controller.ModelState, _logger);

                if (controller.ModelState.IsValid)
                {
                    var user = await _userService.CreateUserAsync(new User { UserName = model.UserName, Email = model.Email, EmailConfirmed = !settings.UsersMustValidateEmail, IsEnabled = !settings.UsersAreModerated }, model.Password, (key, message) => controller.ModelState.AddModelError(key, message)) as User;

                    if (user != null && controller.ModelState.IsValid)
                    {
                        if (settings.UsersMustValidateEmail && !user.EmailConfirmed)
                        {
                            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                            // Send an email with this link
                            await SendEmailConfirmationTokenAsync(controller, user, confirmationEmailSubject);
                        }
                        else if (!(settings.UsersAreModerated && !user.IsEnabled))
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                        }

                        _logger.LogInformation(3, "User created a new account with password.");

                        await _registrationFormEvents.InvokeAsync((e, user) => e.RegisteredAsync(user), user, _logger);

                        return user;
                    }
                }
            }

            return null;
        }

        public virtual async Task<bool> SendEmailAsync(string email, string subject, IShape model)
        {
            var body = String.Empty;

            using (var sw = new StringWriter())
            {
                var htmlContent = await _displayHelper.ShapeExecuteAsync(model);
                htmlContent.WriteTo(sw, _htmlEncoder);
                body = sw.ToString();
            }

            var message = new MailMessage()
            {
                To = email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            var result = await _smtpService.SendAsync(message);

            return result.Succeeded;
        }

        public async Task<string> SendEmailConfirmationTokenAsync(Controller controller, User user, string subject)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = controller.Url.Action("ConfirmEmail", "Registration", new { userId = user.UserId, code }, protocol: controller.HttpContext.Request.Scheme);

            await SendEmailAsync(user.Email, subject, new ConfirmEmailViewModel() { User = user, ConfirmEmailUrl = callbackUrl });

            return callbackUrl;
        }
    }
}
