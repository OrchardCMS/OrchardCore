using Fluid;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.DisplayManagement.Liquid.TagHelpers;
using OrchardCore.DisplayManagement.Liquid.Tags;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Liquid;

namespace Microsoft.Extensions.DependencyInjection;

public static class LiquidCoreServices
{
    public static IServiceCollection AddLiquidCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<LiquidViewParser>();
        services.AddSingleton<IOptions<FluidParserOptions>>(serviceProvider =>
        {
            // The 'OrchardCore_Liquid' section is deprecated and will be removed in a future major version, use 'Liquid' instead.
            var section = serviceProvider.GetRequiredService<IShellConfiguration>().GetSectionCompat("Liquid", "OrchardCore_Liquid");

            return Options.Options.Create(new FluidParserOptions
            {
                AllowParentheses = section.GetValue<bool>(nameof(FluidParserOptions.AllowParentheses)),
                AllowLiquidTag = section.GetValue<bool>(nameof(FluidParserOptions.AllowLiquidTag)),
                AllowTrailingQuestionMark = section.GetValue<bool>(nameof(FluidParserOptions.AllowTrailingQuestionMark)),

                // Always enable the use of functions, do not expose it as a configuration option.
                AllowFunctions = true,

                // Existing templates can use escape sequences in their strings, do not expose it as a configuration option.
                AllowStringEscapeSequences = true,
            });
        });

        services.AddSingleton<IAnchorTag, AnchorTag>();

        services.AddLiquidTemplateOptions();
        services.AddTransient<IConfigureOptions<TemplateOptionsBuilder>, TemplateOptionsFileProviderSetup>();

        services.AddTransient<IConfigureOptions<LiquidViewOptions>, LiquidViewOptionsSetup>();

        services.AddTransient<IConfigureOptions<ShapeTemplateOptions>, LiquidShapeTemplateOptionsSetup>();

        services.AddSingleton<IApplicationFeatureProvider<ViewsFeature>, LiquidViewsFeatureProvider>();

        // Liquid shape templates are rendered directly, not through their Razor pages like Liquid views.
        services.AddScoped<LiquidTemplateContextAccessor>();
        services.AddScoped<LiquidShapeTemplateViewEngine>();
        services.AddScoped<IShapeTemplateViewEngine>(sp => sp.GetRequiredService<LiquidShapeTemplateViewEngine>());

        services.AddSingleton<LiquidTagHelperFactory>();

        services.AddSingleton<IConfigureOptions<TemplateOptionsBuilder>, TemplateOptionsConfigurations>();

        services.AddLiquidFilter<SanitizeHtmlFilter>("sanitize_html");

#pragma warning disable CS0618 // Type or member is obsolete
        services.AddLiquidFilter<SupportedCulturesFilter>("supported_cultures");
#pragma warning restore CS0618 // Type or member is obsolete

        return services;
    }
}
