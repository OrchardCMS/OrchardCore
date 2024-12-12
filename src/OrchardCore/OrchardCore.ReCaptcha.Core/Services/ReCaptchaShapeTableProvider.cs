using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.ReCaptcha.ActionFilters;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Services;

public abstract class ReCaptchaShapeTableProvider : ShapeTableProvider
{
    private readonly string _shapeType;
    private readonly string _tag;

    protected ReCaptchaShapeTableProvider(string shapeType, string tag)
    {
        _shapeType = shapeType;
        _tag = tag ?? string.Empty;
    }

    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {

        builder.Describe(_shapeType)
            .OnDisplaying(async builder =>
            {
                var siteService = builder.DisplayContext.ServiceProvider.GetService<ISiteService>();

                var reCaptchaSettings = await siteService.GetSettingsAsync<ReCaptchaSettings>();

                if (!reCaptchaSettings.IsValid())
                {
                    return;
                }

                var reCaptchaService = builder.ServiceProvider.GetService<ReCaptchaService>();

                if (!await reCaptchaService.IsThisARobotAsync(_tag))
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
                    tag = _tag,
                }));

                await contentShape.AddAsync(reCaptchaShape, "after");
            });

        return ValueTask.CompletedTask;
    }
}
