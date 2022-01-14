using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using OrchardCore.Email;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Services
{
    public class DefaultUserControllerService
    {
        private readonly ISmtpService _smtpService;
        private readonly IEnumerable<IRegistrationFormEvents> _registrationFormEvents;
        private readonly IUserService _userService;
        private readonly ISiteService _siteService;
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;
        private readonly ILogger<DefaultUserControllerService> _logger;

        public DefaultUserControllerService(
            ISmtpService smtpService,
            IEnumerable<IRegistrationFormEvents> registrationFormEvents,
            IUserService userService,
            ISiteService siteService,
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            ILogger<DefaultUserControllerService> logger)
        {
            _smtpService = smtpService;
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
        /// <param name="urlHelper"></param>
        /// <param name="model"></param>
        /// <param name="confirmationEmailSubject"></param>
        /// <returns></returns>
        public virtual async Task<IUser> RegisterUser(IUrlHelper urlHelper, RegisterViewModel model, string confirmationEmailSubject)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();

            if (settings.UsersCanRegister != UserRegistrationType.NoRegistration)
            {
                await _registrationFormEvents.InvokeAsync((e, modelState) => e.RegistrationValidationAsync((key, message) => modelState.AddModelError(key, message)), urlHelper.ActionContext.ModelState, _logger);

                if (urlHelper.ActionContext.ModelState.IsValid)
                {
                    var user = await _userService.CreateUserAsync(new User { UserName = model.UserName, Email = model.Email, EmailConfirmed = !settings.UsersMustValidateEmail, IsEnabled = !settings.UsersAreModerated }, model.Password, (key, message) => urlHelper.ActionContext.ModelState.AddModelError(key, message)) as User;

                    if (user != null && urlHelper.ActionContext.ModelState.IsValid)
                    {
                        if (settings.UsersMustValidateEmail && !user.EmailConfirmed)
                        {
                            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                            // Send an email with this link
                            await SendEmailConfirmationTokenAsync(urlHelper, user, confirmationEmailSubject);
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

        public virtual async Task<string> SendEmailConfirmationTokenAsync(IUrlHelper urlHelper, User user, string subject)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = urlHelper.Action("ConfirmEmail", "Registration", new { userId = user.UserId, code }, protocol: urlHelper.ActionContext.HttpContext.Request.Scheme);

            await _smtpService.SendAsync(user.Email, subject, new ConfirmEmailViewModel() { User = user, ConfirmEmailUrl = callbackUrl });

            return callbackUrl;
        }
    }
}
