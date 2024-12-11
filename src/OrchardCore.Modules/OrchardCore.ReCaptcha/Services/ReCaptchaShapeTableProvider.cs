using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.ReCaptcha.ActionFilters;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Services;

internal sealed class ReCaptchaShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("LoginForm_Edit")
            .OnDisplaying(async builder =>
            {
                var siteService = builder.DisplayContext.ServiceProvider.GetService<ISiteService>();

                var reCaptchaSettings = await siteService.GetSettingsAsync<ReCaptchaSettings>();

                if (!reCaptchaSettings.IsValid())
                {
                    return;
                }

                var reCaptchaService = builder.ServiceProvider.GetService<ReCaptchaService>();

                if (!reCaptchaService.IsThisARobot())
                {
                    return;
                }

                if (!builder.Shape.Properties.TryGetValue("Content", out var content))
                {
                    return;
                }

                var contentShape = content as IShape;

                if (contentShape == null)
                {
                    return;
                }

                var shapeFactory = builder.DisplayContext.ServiceProvider.GetService<IShapeFactory>();

                var reCaptchaShape = await shapeFactory.CreateAsync("ReCaptcha", Arguments.From(new
                {
                    mode = ReCaptchaMode.AlwaysShow,
                    language = CultureInfo.CurrentUICulture.Name,
                }));

                await contentShape.AddAsync(reCaptchaShape, "after");
            });

        return ValueTask.CompletedTask;
    }
}
