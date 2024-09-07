using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class ForgotPasswordLoginFormDisplayDriver : DisplayDriver<LoginForm>
{
    private readonly ISiteService _siteService;

    public ForgotPasswordLoginFormDisplayDriver(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override async Task<IDisplayResult> EditAsync(LoginForm model, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<ResetPasswordSettings>();

        if (!settings.AllowResetPassword)
        {
            return null;
        }

        return View("LoginFormForgotPassword", model).Location("Links:5");
    }
}
