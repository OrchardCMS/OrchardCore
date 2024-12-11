using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.ReCaptcha.ActionFilters;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ReCaptcha.TagHelpers;
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

                var reCaptchaTagHelper = builder.ServiceProvider.GetService<ReCaptchaTagHelper>();

                var context = new TagHelperContext(
                    tagName: "captcha",
                    allAttributes: new TagHelperAttributeList(),
                    items: new Dictionary<object, object>(),
                    uniqueId: IdGenerator.GenerateId()
                );

                // Create an empty TagHelperOutput to hold the final output HTML
                var output = new TagHelperOutput(
                    tagName: "captcha",
                    attributes: new TagHelperAttributeList(),
                    getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
                );

                reCaptchaTagHelper.Mode = ReCaptchaMode.AlwaysShow;
                reCaptchaTagHelper.Language = CultureInfo.CurrentUICulture.Name;

                await reCaptchaTagHelper.ProcessAsync(context, output);

                if (!builder.Shape.Properties.TryGetValue("Content", out var content))
                {
                    return;
                }

                var contentShape = content as IShape;

                if (contentShape == null)
                {
                    return;
                }

                await contentShape.AddAsync(output, "after");
            });

        return ValueTask.CompletedTask;
    }
}
