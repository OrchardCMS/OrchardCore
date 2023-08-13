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
    public class StyleBlock
    {
        private static readonly char[] _separators = new[] { ',', ' ' };

        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, IReadOnlyList<Statement> statements, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;
            var resourceManager = services.GetRequiredService<IResourceManager>();

            string name = null;
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
                    case "at": Enum.TryParse((await argument.Expression.EvaluateAsync(context)).ToStringValue(), ignoreCase: true, out at); break;
                    default: (customAttributes ??= new Dictionary<string, string>())[argument.Name] = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                }
            }

            if (!String.IsNullOrEmpty(name))
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

                // This allows additions to the pre registered style dependencies.
                if (!String.IsNullOrEmpty(dependsOn))
                {
                    setting.SetDependencies(dependsOn.Split(_separators, StringSplitOptions.RemoveEmptyEntries));
                }

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

                if (!String.IsNullOrWhiteSpace(content))
                {
                    // Inline named style definition
                    resourceManager.InlineManifest.DefineStyle(name).SetInnerContent(content);
                }

                if (at == ResourceLocation.Inline)
                {
                    resourceManager.RenderLocalStyle(setting, writer);
                }
            }
            else
            {
                // Custom style content

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

                var builder = new TagBuilder("style");
                builder.InnerHtml.AppendHtml(content);
                builder.TagRenderMode = TagRenderMode.Normal;

                if (customAttributes != null)
                {
                    foreach (var attribute in customAttributes)
                    {
                        builder.Attributes.Add(attribute.Key, attribute.Value);
                    }
                }

                // If no type was specified, define a default one
                if (!builder.Attributes.ContainsKey("type"))
                {
                    builder.Attributes.Add("type", "text/css");
                }

                if (at == ResourceLocation.Inline)
                {
                    builder.WriteTo(writer, (HtmlEncoder)encoder);
                }
                else
                {
                    resourceManager.RegisterStyle(builder);
                }
            }

            return Completion.Normal;
        }
    }
}
