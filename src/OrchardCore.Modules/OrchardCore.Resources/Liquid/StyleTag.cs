using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources.Liquid;

public class StyleTag
{
    private static readonly char[] _separators = [',', ' '];

    public static async ValueTask<Completion> WriteToAsync(IReadOnlyList<FilterArgument> argumentsList, TextWriter writer, TextEncoder _, TemplateContext context)
    {
        var services = ((LiquidTemplateContext)context).Services;
        var resourceManager = services.GetRequiredService<IResourceManager>();

        string name = null;
        string src = null;
        bool? appendversion = null;
        string cdnSrc = null;
        string debugSrc = null;
        string debugCdnSrc = null;
        bool? useCdn = null;
        string condition = null;
        string culture = null;
        bool? debug = null;
        string dependsOn = null;
        string version = null;
        var at = ResourceLocation.Unspecified;

        Dictionary<string, string> customAttributes = null;

        foreach (var argument in argumentsList)
        {
            switch (argument.Name)
            {
                case "name": name = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                case "src": src = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                case "append_version": appendversion = (await argument.Expression.EvaluateAsync(context)).ToBooleanValue(); break;
                case "cdn_src": cdnSrc = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                case "debug_src": debugSrc = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                case "debug_cdn_src": debugCdnSrc = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                case "use_cdn": useCdn = (await argument.Expression.EvaluateAsync(context)).ToBooleanValue(); break;
                case "condition": condition = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                case "culture": culture = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                case "debug": debug = (await argument.Expression.EvaluateAsync(context)).ToBooleanValue(); break;
                case "depends_on": dependsOn = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                case "version": version = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                case "at": Enum.TryParse((await argument.Expression.EvaluateAsync(context)).ToStringValue(), ignoreCase: true, out at); break;
                default: (customAttributes ??= [])[argument.Name] = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
            }
        }

        if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(src))
        {
            // Include custom style
            var setting = resourceManager.RegisterUrl("stylesheet", src, debugSrc);

            if (customAttributes != null)
            {
                foreach (var attribute in customAttributes)
                {
                    setting.SetAttribute(attribute.Key, attribute.Value);
                }
            }

            if (at != ResourceLocation.Unspecified)
            {
                setting.AtLocation(at);
            }
            else
            {
                setting.AtLocation(ResourceLocation.Head);
            }

            if (!string.IsNullOrEmpty(condition))
            {
                setting.UseCondition(condition);
            }

            if (appendversion.HasValue)
            {
                setting.ShouldAppendVersion(appendversion);
            }

            if (debug != null)
            {
                setting.UseDebugMode(debug.Value);
            }

            if (!string.IsNullOrEmpty(culture))
            {
                setting.UseCulture(culture);
            }

            if (!string.IsNullOrEmpty(dependsOn))
            {
                setting.SetDependencies(dependsOn.Split(_separators, StringSplitOptions.RemoveEmptyEntries));
            }

            if (at == ResourceLocation.Inline)
            {
                resourceManager.RenderLocalStyle(setting, writer);
            }
        }
        else if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(src))
        {
            // Resource required

            var setting = resourceManager.RegisterResource("stylesheet", name);

            if (customAttributes != null)
            {
                foreach (var attribute in customAttributes)
                {
                    setting.SetAttribute(attribute.Key, attribute.Value);
                }
            }

            if (at != ResourceLocation.Unspecified)
            {
                setting.AtLocation(at);
            }
            else
            {
                setting.AtLocation(ResourceLocation.Head);
            }

            if (useCdn != null)
            {
                setting.UseCdn(useCdn.Value);
            }

            if (!string.IsNullOrEmpty(condition))
            {
                setting.UseCondition(condition);
            }

            if (debug != null)
            {
                setting.UseDebugMode(debug.Value);
            }

            if (!string.IsNullOrEmpty(culture))
            {
                setting.UseCulture(culture);
            }

            if (appendversion.HasValue)
            {
                setting.ShouldAppendVersion(appendversion);
            }

            if (!string.IsNullOrEmpty(version))
            {
                setting.UseVersion(version);
            }

            // This allows additions to the pre registered style dependencies.
            if (!string.IsNullOrEmpty(dependsOn))
            {
                setting.SetDependencies(dependsOn.Split(_separators, StringSplitOptions.RemoveEmptyEntries));
            }

            if (at == ResourceLocation.Inline)
            {
                resourceManager.RenderLocalStyle(setting, writer);
            }
        }
        else if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(src))
        {
            // Inline declaration

            var definition = resourceManager.InlineManifest.DefineStyle(name);
            definition.SetUrl(src, debugSrc);

            if (customAttributes != null)
            {
                foreach (var attribute in customAttributes)
                {
                    definition.SetAttribute(attribute.Key, attribute.Value);
                }
            }

            if (!string.IsNullOrEmpty(version))
            {
                definition.SetVersion(version);
            }

            if (!string.IsNullOrEmpty(cdnSrc))
            {
                definition.SetCdn(cdnSrc, debugCdnSrc);
            }

            if (!string.IsNullOrEmpty(culture))
            {
                definition.SetCultures(culture.Split(_separators, StringSplitOptions.RemoveEmptyEntries));
            }

            if (!string.IsNullOrEmpty(dependsOn))
            {
                definition.SetDependencies(dependsOn.Split(_separators, StringSplitOptions.RemoveEmptyEntries));
            }

            // Also include the style.
            var setting = resourceManager.RegisterResource("stylesheet", name);

            if (useCdn != null)
            {
                setting.UseCdn(useCdn.Value);
            }

            if (!string.IsNullOrEmpty(condition))
            {
                setting.UseCondition(condition);
            }

            if (debug != null)
            {
                setting.UseDebugMode(debug.Value);
            }

            if (!string.IsNullOrEmpty(culture))
            {
                setting.UseCulture(culture);
            }

            if (at != ResourceLocation.Unspecified)
            {
                setting.AtLocation(at);
            }
            else
            {
                setting.AtLocation(ResourceLocation.Head);
            }

            if (at == ResourceLocation.Inline)
            {
                resourceManager.RenderLocalStyle(setting, writer);
            }
        }

        return Completion.Normal;
    }
}
