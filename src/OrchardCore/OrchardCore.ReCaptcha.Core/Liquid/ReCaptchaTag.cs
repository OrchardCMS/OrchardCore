using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid.Ast;
using OrchardCore.Localization;
using OrchardCore.ReCaptcha.ActionFilters;
using OrchardCore.ReCaptcha.ActionFilters.Detection;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ReCaptcha.TagHelpers;
using OrchardCore.ResourceManagement;

namespace OrchardCore.ReCaptcha.Liquid
{
    public class ReCaptchaTag : ArgumentsTag
    {
        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] args)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var servicesValue))
            {
                throw new ArgumentException("Services missing while invoking 'helper'");
            }

            var services = servicesValue as IServiceProvider;

            var arguments = (FilterArguments)(await new ArgumentsExpression(args).EvaluateAsync(context)).ToObjectValue();
            var mode = arguments.HasNamed("mode") ? Enum.Parse<ReCaptchaMode>(arguments["mode"].ToString()) : ReCaptchaMode.PreventRobots;
            var language = arguments.HasNamed("language") ? arguments["language"].ToStringValue() : null;

            var settings = services.GetRequiredService<IOptions<ReCaptchaSettings>>().Value;
            var robotDetectors = services.GetRequiredService<IEnumerable<IDetectRobots>>();

            void RenderDivToTagHelper(TagBuilder builder)
            {
                builder.WriteTo(writer, (HtmlEncoder)encoder);
            }

            await ReCaptchaRenderer.ShowCaptchaOrCallback(
                services.GetRequiredService<IOptions<ReCaptchaSettings>>().Value,
                mode,
                language,
                services.GetRequiredService<IEnumerable<IDetectRobots>>(),
                services.GetRequiredService<ILocalizationService>(),
                services.GetRequiredService<IResourceManager>(),
                services.GetRequiredService<IStringLocalizer<ReCaptchaTagHelper>>(),
                services.GetRequiredService<ILogger>(),
                RenderDivToTagHelper,
                () => { });

            return Completion.Normal;
        }
    }
}
