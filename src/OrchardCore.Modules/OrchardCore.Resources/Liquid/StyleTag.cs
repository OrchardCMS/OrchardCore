using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources.Liquid
{
    public class StyleTag
    {
        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, TextWriter writer, TextEncoder encoder, TemplateContext context)
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

            var customAttributes = new Dictionary<string, string>();

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
                    case "at": Enum.TryParse((await argument.Expression.EvaluateAsync(context)).ToStringValue(), out at); break;
                    default: customAttributes[argument.Name] = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                }
            }

            if (String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(src))
            {
                // Include custom style
                var setting = resourceManager.RegisterUrl("stylesheet", src, debugSrc);

                foreach (var attribute in customAttributes)
                {
                    setting.SetAttribute(attribute.Key, attribute.Value);
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

                if (appendversion.HasValue == true)
                {
                    setting.ShouldAppendVersion(appendversion);
                }

                if (debug != null)
                {
                    setting.UseDebugMode(debug.Value);
                }

                if (!String.IsNullOrEmpty(culture))
                {
                    setting.UseCulture(culture);
                }

                if (!String.IsNullOrEmpty(dependsOn))
                {
                    setting.SetDependencies(dependsOn.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }

                if (at == ResourceLocation.Inline)
                {
                    var buffer = new HtmlContentBuilder();
                    resourceManager.RenderLocalStyle(setting, buffer);
                    var htmlEncoder = services.GetRequiredService<HtmlEncoder>();

                    buffer.WriteTo(writer, htmlEncoder);
                }
            }
            else if (!String.IsNullOrEmpty(name) && String.IsNullOrEmpty(src))
            {
                // Resource required

                var setting = resourceManager.RegisterResource("stylesheet", name);

                foreach (var attribute in customAttributes)
                {
                    setting.SetAttribute(attribute.Key, attribute.Value);
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

                if (appendversion.HasValue == true)
                {
                    setting.ShouldAppendVersion(appendversion);
                }

                if (!String.IsNullOrEmpty(version))
                {
                    setting.UseVersion(version);
                }

                // This allows additions to the pre registered style dependencies.
                if (!String.IsNullOrEmpty(dependsOn))
                {
                    setting.SetDependencies(dependsOn.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }

                // TODO: implement styleblock

                //var childContent = await output.GetChildContentAsync();
                //if (!childContent.IsEmptyOrWhiteSpace)
                //{
                //    // Inline named style definition
                //    resourceManager.InlineManifest.DefineStyle(name)
                //        .SetInnerContent(childContent.GetContent());
                //}

                if (at == ResourceLocation.Inline)
                {
                    var buffer = new HtmlContentBuilder();
                    resourceManager.RenderLocalStyle(setting, buffer);
                    var htmlEncoder = services.GetRequiredService<HtmlEncoder>();

                    buffer.WriteTo(writer, htmlEncoder);
                }
            }
            else if (!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(src))
            {
                // Inline declaration

                var definition = resourceManager.InlineManifest.DefineStyle(name);
                definition.SetUrl(src, debugSrc);

                foreach (var attribute in customAttributes)
                {
                    definition.SetAttribute(attribute.Key, attribute.Value);
                }

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
                    definition.SetCultures(culture.Split(',', StringSplitOptions.RemoveEmptyEntries));
                }

                if (!String.IsNullOrEmpty(dependsOn))
                {
                    definition.SetDependencies(dependsOn.Split(',', StringSplitOptions.RemoveEmptyEntries));
                }

                // Also include the style.
                var setting = resourceManager.RegisterResource("stylesheet", name);

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
                    var buffer = new HtmlContentBuilder();
                    resourceManager.RenderLocalStyle(setting, buffer);
                    var htmlEncoder = services.GetRequiredService<HtmlEncoder>();

                    buffer.WriteTo(writer, htmlEncoder);
                }
            }
            else if (String.IsNullOrEmpty(name) && String.IsNullOrEmpty(src))
            {
                // TODO: implement styleblock

                // Custom style content

                //var childContent = await output.GetChildContentAsync();

                //var builder = new TagBuilder("style");
                //builder.InnerHtml.AppendHtml(childContent);
                //builder.TagRenderMode = TagRenderMode.Normal;

                //foreach (var attribute in customAttributes)
                //{
                //    builder.Attributes.Add(attribute.Key, attribute.Value);
                //}

                //// If no type was specified, define a default one
                //if (!builder.Attributes.ContainsKey("type"))
                //{
                //    builder.Attributes.Add("type", "text/css");
                //}

                //if (at == ResourceLocation.Inline)
                //{
                //    output.Content.SetHtmlContent(builder);
                //}
                //else
                //{
                //    resourceManager.RegisterStyle(builder);
                //}
            }

            return Completion.Normal;
        }
    }
}
