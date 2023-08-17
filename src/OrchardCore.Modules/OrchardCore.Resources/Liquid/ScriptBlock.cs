using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Cysharp.Text;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources.Liquid
{
    public class ScriptBlock
    {
        private static readonly char[] _separators = new[] { ',', ' ' };

        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, IReadOnlyList<Statement> statements, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;
            var resourceManager = services.GetRequiredService<IResourceManager>();

            string name = null;
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
                    case "condition": condition = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "culture": culture = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "debug": debug = (await argument.Expression.EvaluateAsync(context)).ToBooleanValue(); break;
                    case "depends_on": dependsOn = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "version": version = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "at": Enum.TryParse((await argument.Expression.EvaluateAsync(context)).ToStringValue(), ignoreCase: true, out at); break;
                    default: (customAttributes ??= new Dictionary<string, string>())[argument.Name] = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                }
            }

            if (!String.IsNullOrEmpty(name))
            {
                // Resource required

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

                if (!String.IsNullOrEmpty(version))
                {
                    setting.UseVersion(version);
                }

                // This allows additions to the pre registered scripts dependencies.
                if (!String.IsNullOrEmpty(dependsOn))
                {
                    setting.SetDependencies(dependsOn.Split(_separators, StringSplitOptions.RemoveEmptyEntries));
                }

                // Allow Inline to work with both named scripts, and named inline scripts.
                if (at != ResourceLocation.Unspecified)
                {
                    var content = "";

                    if (statements != null && statements.Count > 0)
                    {
                        using var sw = new ZStringWriter();
                        var completion = await statements.RenderStatementsAsync(sw, encoder, context);

                        if (completion != Completion.Normal)
                        {
                            return completion;
                        }

                        content = sw.ToString();
                    }

                    // Named inline declaration.
                    if (!String.IsNullOrWhiteSpace(content))
                    {
                        // Inline content definition
                        resourceManager.InlineManifest.DefineScript(name).SetInnerContent(content);
                    }

                    if (at == ResourceLocation.Inline)
                    {
                        resourceManager.RenderLocalScript(setting, writer);
                    }
                }
                else
                {
                    resourceManager.RenderLocalScript(setting, writer);
                }
            }
            else
            {
                // Custom script content

                var content = "";

                if (statements != null && statements.Count > 0)
                {
                    using var sw = new ZStringWriter();
                    var completion = await statements.RenderStatementsAsync(sw, encoder, context);

                    if (completion != Completion.Normal)
                    {
                        return completion;
                    }

                    content = sw.ToString();
                }

                var builder = new TagBuilder("script");
                builder.InnerHtml.AppendHtml(content);
                builder.TagRenderMode = TagRenderMode.Normal;

                if (customAttributes != null)
                {
                    foreach (var attribute in customAttributes)
                    {
                        builder.Attributes.Add(attribute.Key, attribute.Value);
                    }
                }

                if (at == ResourceLocation.Head)
                {
                    resourceManager.RegisterHeadScript(builder);
                }
                else if (at == ResourceLocation.Inline)
                {
                    builder.WriteTo(writer, (HtmlEncoder)encoder);
                }
                else
                {
                    resourceManager.RegisterFootScript(builder);
                }
            }

            return Completion.Normal;
        }
    }
}
