using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Abstractions;
public interface IAccountControllerAbstraction : IControllerAbstraction<AccountController>
{
    IActionResult ChangePassword(string returnUrl = null);
    Task<IActionResult> ChangePassword(ChangePasswordViewModel model, string returnUrl = null);
    IActionResult ChangePasswordConfirmation();
    Task<IActionResult> DefaultExternalLogin(string protectedToken, string returnUrl = null);
    IActionResult ExternalLogin(string provider, string returnUrl = null);
    Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null);
    Task<IActionResult> ExternalLogins();
    Task<IActionResult> LinkExternalLogin(LinkExternalLoginViewModel model, string returnUrl = null);
    Task<IActionResult> LinkLogin(string provider);
    Task<IActionResult> LinkLoginCallback();
    Task<IActionResult> Login(string returnUrl = null);
    Task<IActionResult> Login(LoginViewModel model, string returnUrl = null);
    Task<IActionResult> LogOff();
    Task<IActionResult> RegisterExternalLogin(RegisterExternalLoginViewModel model, string returnUrl = null);
    Task<IActionResult> RemoveLogin(RemoveLoginViewModel model);

}
