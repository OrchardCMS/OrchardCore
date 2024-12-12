using System.Globalization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.ReCaptcha.Drivers;

public sealed class ReCaptchaLoginFormDisplayDriver : DisplayDriver<LoginForm>
{
    private readonly ISiteService _siteService;
    private readonly IShapeFactory _shapeFactory;

    public ReCaptchaLoginFormDisplayDriver(
        ISiteService siteService,
        IShapeFactory shapeFactory)
    {
        _siteService = siteService;
        _shapeFactory = shapeFactory;
    }

    public override async Task<IDisplayResult> EditAsync(LoginForm model, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<ReCaptchaSettings>();

        if (!settings.ConfigurationExists())
        {
            return null;
        }

        var reCaptchaShape = await _shapeFactory.CreateAsync("ReCaptcha", Arguments.From(new
        {
            language = CultureInfo.CurrentUICulture.Name,
        }));

        return Shape("ReCaptcha", reCaptchaShape).Location("Content:after");
    }
}
