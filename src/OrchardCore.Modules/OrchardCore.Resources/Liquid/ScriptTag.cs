using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources.Liquid
{
    public class ScriptTag
    {
        private static readonly char[] Separators = new[] { ',', ' ' };
        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, TextWriter writer, TextEncoder encoder, TemplateContext context)
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
                    default: (customAttributes ??= new Dictionary<string, string>())[argument.Name] = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                }
            }

            if (String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(src))
            {
                // {% script src:"~/TheBlogTheme/js/clean-blog.min.js" %}
                RequireSettings setting;

                if (String.IsNullOrEmpty(dependsOn))
                {
                    // Include custom script url
                    setting = resourceManager.RegisterUrl("script", src, debugSrc);
                }
                else
                {
                    // Anonymous declaration with dependencies, then display

                    // Using the source as the name to prevent duplicate references to the same file
                    var s = src.ToLowerInvariant();

                    var definition = resourceManager.InlineManifest.DefineScript(s);
                    definition.SetUrl(src, debugSrc);

                    if (!String.IsNullOrEmpty(version))
                    {
                        definition.SetVersion(version);
                    }

                    if (!String.IsNullOrEmpty(cdnSrc))
                    {
                        definition.SetCdn(cdnSrc, debugCdnSrc);
                    }

                    if (!String.IsNullOrEmpty(culture))
                    {
                        definition.SetCultures(culture.Split(Separators, StringSplitOptions.RemoveEmptyEntries));
                    }

                    if (!String.IsNullOrEmpty(dependsOn))
                    {
                        definition.SetDependencies(dependsOn.Split(Separators, StringSplitOptions.RemoveEmptyEntries));
                    }

                    if (appendVersion.HasValue)
                    {
                        definition.ShouldAppendVersion(appendVersion);
                    }

                    if (!String.IsNullOrEmpty(version))
                    {
                        definition.SetVersion(version);
                    }

                    setting = resourceManager.RegisterResource("script", s);
                }

                if (at != ResourceLocation.Unspecified)
                {
                    setting.AtLocation(at);
                }

                if (!String.IsNullOrEmpty(condition))
                {
                    setting.UseCondition(condition);
                }

                if (debug != null)
                {
                    setting.UseDebugMode(debug.Value);
                }

                if (!String.IsNullOrEmpty(culture))
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

                if (at == ResourceLocation.Unspecified || at == ResourceLocation.Inline)
                {
                    resourceManager.RenderLocalScript(setting, writer);
                }
            }
            else if (!String.IsNullOrEmpty(name) && String.IsNullOrEmpty(src))
            {
                // Resource required
                // {% script name:"bootstrap" %}

                var setting = resourceManager.RegisterResource("script", name);

                if (at != ResourceLocation.Unspecified)
                {
                    setting.AtLocation(at);
                }

                if (useCdn != null)
                {
                    setting.UseCdn(useCdn.Value);
                }

                if (!String.IsNullOrEmpty(condition))
                {
                    setting.UseCondition(condition);
                }

                if (debug != null)
                {
                    setting.UseDebugMode(debug.Value);
                }

                if (!String.IsNullOrEmpty(culture))
                {
                    setting.UseCulture(culture);
                }

                if (appendVersion.HasValue)
                {
                    setting.ShouldAppendVersion(appendVersion);
                }

                if (!String.IsNullOrEmpty(version))
                {
                    setting.UseVersion(version);
                }

                // This allows additions to the pre registered scripts dependencies.
                if (!String.IsNullOrEmpty(dependsOn))
                {
                    setting.SetDependencies(dependsOn.Split(Separators, StringSplitOptions.RemoveEmptyEntries));
                }

                if (at == ResourceLocation.Unspecified || at == ResourceLocation.Inline)
                {
                    resourceManager.RenderLocalScript(setting, writer);
                }
            }
            else if (!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(src))
            {
                // Inline declaration

                var definition = resourceManager.InlineManifest.DefineScript(name);
                definition.SetUrl(src, debugSrc);

                if (!String.IsNullOrEmpty(version))
                {
                    definition.SetVersion(version);
                }

                if (!String.IsNullOrEmpty(cdnSrc))
                {
                    definition.SetCdn(cdnSrc, debugCdnSrc);
                }

                if (!String.IsNullOrEmpty(culture))
                {
                    definition.SetCultures(culture.Split(Separators, StringSplitOptions.RemoveEmptyEntries));
                }

                if (!String.IsNullOrEmpty(dependsOn))
                {
                    definition.SetDependencies(dependsOn.Split(Separators, StringSplitOptions.RemoveEmptyEntries));
                }

                if (appendVersion.HasValue)
                {
                    definition.ShouldAppendVersion(appendVersion);
                }

                if (!String.IsNullOrEmpty(version))
                {
                    definition.SetVersion(version);
                }

                // If At is specified then we also render it
                if (at != ResourceLocation.Unspecified)
                {
                    var setting = resourceManager.RegisterResource("script", name);

                    setting.AtLocation(at);

                    if (useCdn != null)
                    {
                        setting.UseCdn(useCdn.Value);
                    }

                    if (!String.IsNullOrEmpty(condition))
                    {
                        setting.UseCondition(condition);
                    }

                    if (debug != null)
                    {
                        setting.UseDebugMode(debug.Value);
                    }

                    if (!String.IsNullOrEmpty(culture))
                    {
                        setting.UseCulture(culture);
                    }

                    if (customAttributes != null)
                    {
                        foreach (var attribute in customAttributes)
                        {
                            setting.SetAttribute(attribute.Key, attribute.Value);
                        }
                    }

                    if (at == ResourceLocation.Inline)
                    {
                        resourceManager.RenderLocalScript(setting, writer);
                    }

                }
            }

            return Completion.Normal;
        }
    }
}
