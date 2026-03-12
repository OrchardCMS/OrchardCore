using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources.Liquid;

public class ScriptTag
{
    private static readonly char[] _separators = [',', ' '];

    public static async ValueTask<Completion> WriteToAsync(IReadOnlyList<FilterArgument> argumentsList, TextWriter writer, TextEncoder _, TemplateContext context)
    {
        var services = ((LiquidTemplateContext)context).Services;
        var resourceManager = services.GetRequiredService<IResourceManager>();

        string name = null;
        string src = null;
        bool? appendVersion = null;
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
                case "append_version": appendVersion = (await argument.Expression.EvaluateAsync(context)).ToBooleanValue(); break;
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

        void PopulateDefinition(ResourceDefinition definition)
        {
            definition.SetUrl(src, debugSrc);

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

            if (appendVersion.HasValue)
            {
                definition.ShouldAppendVersion(appendVersion);
            }
        }

        void PopulateSettings(RequireSettings setting, bool hasName)
        {
            if (at != ResourceLocation.Unspecified)
            {
                setting.AtLocation(at);
            }

            if (hasName && useCdn != null)
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

            if (appendVersion.HasValue)
            {
                setting.ShouldAppendVersion(appendVersion);
            }

            if (customAttributes != null)
            {
                foreach (var attribute in customAttributes)
                {
                    setting.SetAttribute(attribute.Key, attribute.Value);
                }
            }
        }

        void ProcessSourceScript()
        {
            // {% script src:"~/TheBlogTheme/js/clean-blog.min.js" %}
            RequireSettings setting;

            if (string.IsNullOrEmpty(dependsOn))
            {
                // Include custom script url.
                setting = resourceManager.RegisterUrl("script", src, debugSrc);
            }
            else
            {
                // Anonymous declaration with dependencies, then display.

                // Using the source as the name to prevent duplicate references to the same file.
                var s = src.ToLowerInvariant();

                PopulateDefinition(resourceManager.InlineManifest.DefineScript(s));

                setting = resourceManager.RegisterResource("script", s);
            }

            PopulateSettings(setting, hasName: false);

            if (at == ResourceLocation.Unspecified || at == ResourceLocation.Inline)
            {
                resourceManager.RenderLocalScript(setting, writer);
            }
        }

        void ProcessNamedScript()
        {
            // Resource required.
            // {% script name:"bootstrap" %}

            var setting = resourceManager.RegisterResource("script", name);

            PopulateSettings(setting, hasName: true);

            if (!string.IsNullOrEmpty(version))
            {
                setting.UseVersion(version);
            }

            // This allows additions to the pre registered scripts dependencies.
            if (!string.IsNullOrEmpty(dependsOn))
            {
                setting.SetDependencies(dependsOn.Split(_separators, StringSplitOptions.RemoveEmptyEntries));
            }

            if (at == ResourceLocation.Unspecified || at == ResourceLocation.Inline)
            {
                resourceManager.RenderLocalScript(setting, writer);
            }
        }

        void ProcessInlineDeclaration()
        {
            // Inline declaration.

            PopulateDefinition(resourceManager.InlineManifest.DefineScript(name));

            // If At is specified then we also render it.
            if (at != ResourceLocation.Unspecified)
            {
                var setting = resourceManager.RegisterResource("script", name);

                PopulateSettings(setting, hasName: true);

                if (at == ResourceLocation.Inline)
                {
                    resourceManager.RenderLocalScript(setting, writer);
                }
            }
        }

        if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(src))
        {
            ProcessSourceScript();
        }
        else if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(src))
        {
            ProcessNamedScript();
        }
        else if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(src))
        {
            ProcessInlineDeclaration();
        }

        return Completion.Normal;
    }
}
