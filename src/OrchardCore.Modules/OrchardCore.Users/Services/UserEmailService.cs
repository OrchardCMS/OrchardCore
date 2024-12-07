using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.Email;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
namespace OrchardCore.Users.Services;

public sealed class UserEmailService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;
    private readonly UserManager<IUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IDisplayHelper _displayHelper;
    private readonly HtmlEncoder _htmlEncoder;

    internal readonly IStringLocalizer S;

    public UserEmailService(
        IHttpContextAccessor httpContextAccessor,
        LinkGenerator linkGenerator,
        UserManager<IUser> userManager,
        IEmailService emailService,
        IDisplayHelper displayHelper,
        HtmlEncoder htmlEncoder,
        IStringLocalizer<UserEmailService> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
        _userManager = userManager;
        _emailService = emailService;
        _displayHelper = displayHelper;
        _htmlEncoder = htmlEncoder;
        S = stringLocalizer;
    }

    public async Task<bool> SendEmailConfirmationAsync(IUser user)
    {
        var confirmEmailUrl = _linkGenerator.GetUriByAction(
            httpContext: _httpContextAccessor.HttpContext,
            action: nameof(EmailConfirmationController.ConfirmEmail),
            controller: typeof(EmailConfirmationController).ControllerName(),
            values: new
            {
                userId = await _userManager.GetUserIdAsync(user),
                code = await _userManager.GenerateEmailConfirmationTokenAsync(user),
            });

        var email = await _userManager.GetEmailAsync(user);

        if (string.IsNullOrEmpty(email))
        {
            return false;
        }

        return await SendEmailAsync(email, S["Confirm your account"], new ConfirmEmailViewModel
        {
            User = user,
            ConfirmEmailUrl = confirmEmailUrl,
        });
    }

    public async Task<bool> SendPasswordResetAsync(User user)
    {
        user.ResetToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.ResetToken));

        var lostPasswordUrl = _linkGenerator.GetUriByAction(
            httpContext: _httpContextAccessor.HttpContext,
            action: nameof(ResetPasswordController.ResetPassword),
            controller: typeof(ResetPasswordController).ControllerName(),
            values: new
            {
                userId = await _userManager.GetUserIdAsync(user),
                code = user.ResetToken,
            });

        // Send email with callback link.
        return await SendEmailAsync(user.Email, S["Reset your password"], new LostPasswordViewModel()
        {
            User = user,
            LostPasswordUrl = lostPasswordUrl,
        });
    }

    private async Task<bool> SendEmailAsync(string email, string subject, IShape model)
    {
        var body = string.Empty;

        using (var sw = new StringWriter())
        {
            var htmlContent = await _displayHelper.ShapeExecuteAsync(model);
            htmlContent.WriteTo(sw, _htmlEncoder);
            body = sw.ToString();
        }

        var result = await _emailService.SendAsync(email, subject, body);

        return result.Succeeded;
    }
}
