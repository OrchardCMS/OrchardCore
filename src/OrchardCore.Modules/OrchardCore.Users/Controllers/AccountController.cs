using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Users.Abstractions;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        private readonly IAccountControllerAbstraction _accountControllerAbstraction;

        public AccountController(
            IAccountControllerAbstraction accountControllerAbstraction)
        {
            _accountControllerAbstraction = accountControllerAbstraction;
            accountControllerAbstraction.BindController = this;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            return await _accountControllerAbstraction.Login(returnUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DefaultExternalLogin(string protectedToken, string returnUrl = null)
        {
            return await _accountControllerAbstraction.DefaultExternalLogin(protectedToken, returnUrl);
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            return await _accountControllerAbstraction.Login(model, returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            return await _accountControllerAbstraction.LogOff();
        }

        [HttpGet]
        public IActionResult ChangePassword(string returnUrl = null)
        {
            return _accountControllerAbstraction.ChangePassword(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, string returnUrl = null)
        {
            return await _accountControllerAbstraction.ChangePassword(model, returnUrl);
        }

        [HttpGet]
        public IActionResult ChangePasswordConfirmation()
        {
            return _accountControllerAbstraction.ChangePasswordConfirmation();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            return _accountControllerAbstraction.ExternalLogin(provider, returnUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            return await _accountControllerAbstraction.ExternalLoginCallback(returnUrl, remoteError);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterExternalLogin(RegisterExternalLoginViewModel model, string returnUrl = null)
        {
            return await _accountControllerAbstraction.RegisterExternalLogin(model, returnUrl);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkExternalLogin(LinkExternalLoginViewModel model, string returnUrl = null)
        {
            return await _accountControllerAbstraction.LinkExternalLogin(model, returnUrl);

        }

        [HttpGet]
        public async Task<IActionResult> ExternalLogins()
        {
            return await _accountControllerAbstraction.ExternalLogins();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkLogin(string provider)
        {
            return await _accountControllerAbstraction.LinkLogin(provider);
        }

        [HttpGet]
        public async Task<IActionResult> LinkLoginCallback()
        {
            return await _accountControllerAbstraction.LinkLoginCallback();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel model)
        {
            return await _accountControllerAbstraction.RemoveLogin(model);
        }
    }
}
