using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement.TagHelpers
{
    [HtmlTargetElement("style", Attributes = NameAttributeName)]
    [HtmlTargetElement("style", Attributes = SrcAttributeName)]
    [HtmlTargetElement("style", Attributes = AtAttributeName)]
    public class StyleTagHelper : TagHelper
    {
        private static readonly char[] _splitSeparators = { ',', ' ' };
        private const string NameAttributeName = "asp-name";
        private const string SrcAttributeName = "asp-src";
        private const string AtAttributeName = "at";
        private const string AppendVersionAttributeName = "asp-append-version";

        [HtmlAttributeName(NameAttributeName)]
        public string Name { get; set; }

        [HtmlAttributeName(SrcAttributeName)]
        public string Src { get; set; }

        [HtmlAttributeName(AppendVersionAttributeName)]
        public bool? AppendVersion { get; set; }

        public string CdnSrc { get; set; }
        public string DebugSrc { get; set; }
        public string DebugCdnSrc { get; set; }

        public bool? UseCdn { get; set; }
        public string Condition { get; set; }
        public string Culture { get; set; }
        public bool? Debug { get; set; }
        public string DependsOn { get; set; }
        public string Version { get; set; }

        [HtmlAttributeName(AtAttributeName)]
        public ResourceLocation At { get; set; }

        private readonly IResourceManager _resourceManager;

        public StyleTagHelper(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            if (String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Src))
            {
                // Include custom style
                var setting = _resourceManager.RegisterUrl("stylesheet", Src, DebugSrc);

                foreach (var attribute in output.Attributes)
                {
                    setting.SetAttribute(attribute.Name, attribute.Value.ToString());
                }

                if (At != ResourceLocation.Unspecified)
                {
                    setting.AtLocation(At);
                }
                else
                {
                    setting.AtLocation(ResourceLocation.Head);
                }

                if (!String.IsNullOrEmpty(Condition))
                {
                    setting.UseCondition(Condition);
                }

                if (AppendVersion.HasValue == true)
                {
                    setting.ShouldAppendVersion(AppendVersion);
                }

                if (Debug != null)
                {
                    setting.UseDebugMode(Debug.Value);
                }

                if (!String.IsNullOrEmpty(Culture))
                {
                    setting.UseCulture(Culture);
                }

                if (!String.IsNullOrEmpty(DependsOn))
                {
                    setting.SetDependencies(DependsOn.Split(_splitSeparators, StringSplitOptions.RemoveEmptyEntries));
                }

                if (At == ResourceLocation.Inline)
                {
                    using var sw = new StringWriter();
                    _resourceManager.RenderLocalStyle(setting, sw);
                    output.Content.AppendHtml(sw.ToString());
                }
            }
            else if (!String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Src))
            {
                // Resource required

                var setting = _resourceManager.RegisterResource("stylesheet", Name);

                foreach (var attribute in output.Attributes)
                {
                    setting.SetAttribute(attribute.Name, attribute.Value.ToString());
                }

                if (At != ResourceLocation.Unspecified)
                {
                    setting.AtLocation(At);
                }
                else
                {
                    setting.AtLocation(ResourceLocation.Head);
                }

                if (UseCdn != null)
                {
                    setting.UseCdn(UseCdn.Value);
                }

                if (!String.IsNullOrEmpty(Condition))
                {
                    setting.UseCondition(Condition);
                }

                if (Debug != null)
                {
                    setting.UseDebugMode(Debug.Value);
                }

                if (!String.IsNullOrEmpty(Culture))
                {
                    setting.UseCulture(Culture);
                }

                if (AppendVersion.HasValue == true)
                {
                    setting.ShouldAppendVersion(AppendVersion);
                }

                if (!String.IsNullOrEmpty(Version))
                {
                    setting.UseVersion(Version);
                }

                // This allows additions to the pre registered style dependencies.
                if (!String.IsNullOrEmpty(DependsOn))
                {
                    setting.SetDependencies(DependsOn.Split(_splitSeparators, StringSplitOptions.RemoveEmptyEntries));
                }

                var childContent = await output.GetChildContentAsync();
                if (!childContent.IsEmptyOrWhiteSpace)
                {
                    // Inline named style definition
                    _resourceManager.InlineManifest.DefineStyle(Name)
                        .SetInnerContent(childContent.GetContent());
                }

                if (At == ResourceLocation.Inline)
                {
                    using var sw = new StringWriter();
                    _resourceManager.RenderLocalStyle(setting, sw);
                    output.Content.AppendHtml(sw.ToString());
                }
            }
            else if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Src))
            {
                // Inline declaration

                var definition = _resourceManager.InlineManifest.DefineStyle(Name);
                definition.SetUrl(Src, DebugSrc);

                foreach (var attribute in output.Attributes)
                {
                    definition.SetAttribute(attribute.Name, attribute.Value.ToString());
                }

                if (!String.IsNullOrEmpty(Version))
                {
                    definition.SetVersion(Version);
                }

                if (!String.IsNullOrEmpty(CdnSrc))
                {
                    definition.SetCdn(CdnSrc, DebugCdnSrc);
                }

                if (!String.IsNullOrEmpty(Culture))
                {
                    definition.SetCultures(Culture.Split(',', StringSplitOptions.RemoveEmptyEntries));
                }

                if (!String.IsNullOrEmpty(DependsOn))
                {
                    definition.SetDependencies(DependsOn.Split(',', StringSplitOptions.RemoveEmptyEntries));
                }

                // Also include the style.
                var setting = _resourceManager.RegisterResource("stylesheet", Name);

                if (UseCdn != null)
                {
                    setting.UseCdn(UseCdn.Value);
                }

                if (!String.IsNullOrEmpty(Condition))
                {
                    setting.UseCondition(Condition);
                }

                if (Debug != null)
                {
                    setting.UseDebugMode(Debug.Value);
                }

                if (!String.IsNullOrEmpty(Culture))
                {
                    setting.UseCulture(Culture);
                }

                if (At != ResourceLocation.Unspecified)
                {
                    setting.AtLocation(At);
                }
                else
                {
                    setting.AtLocation(ResourceLocation.Head);
                }

                if (At == ResourceLocation.Inline)
                {
                    using var sw = new StringWriter();
                    _resourceManager.RenderLocalStyle(setting, sw);
                    output.Content.AppendHtml(sw.ToString());
                }
            }
            else if (String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Src))
            {
                // Custom style content

                var childContent = await output.GetChildContentAsync();

                var builder = new TagBuilder("style");
                builder.InnerHtml.AppendHtml(childContent);
                builder.TagRenderMode = TagRenderMode.Normal;

                foreach (var attribute in output.Attributes)
                {
                    builder.Attributes.Add(attribute.Name, attribute.Value.ToString());
                }

                // If no type was specified, define a default one
                if (!builder.Attributes.ContainsKey("type"))
                {
                    builder.Attributes.Add("type", "text/css");
                }

                if (At == ResourceLocation.Inline)
                {
                    output.Content.SetHtmlContent(builder);
                }
                else
                {
                    _resourceManager.RegisterStyle(builder);
                }
            }
        }
    }
}
