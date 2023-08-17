using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement.TagHelpers
{
    [HtmlTargetElement("script", Attributes = NameAttributeName)]
    [HtmlTargetElement("script", Attributes = SrcAttributeName)]
    [HtmlTargetElement("script", Attributes = AtAttributeName)]
    public class ScriptTagHelper : TagHelper
    {
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

        public ScriptTagHelper(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            if (String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Src))
            {
                // <script asp-src="~/TheBlogTheme/js/clean-blog.min.js"></script>
                RequireSettings setting;

                if (String.IsNullOrEmpty(DependsOn))
                {
                    // Include custom script url
                    setting = _resourceManager.RegisterUrl("script", Src, DebugSrc);
                }
                else
                {
                    // Anonymous declaration with dependencies, then display

                    // Using the source as the name to prevent duplicate references to the same file
                    var name = Src.ToLowerInvariant();

                    var definition = _resourceManager.InlineManifest.DefineScript(name);
                    definition.SetUrl(Src, DebugSrc);

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
                        definition.SetCultures(Culture.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    if (!String.IsNullOrEmpty(DependsOn))
                    {
                        definition.SetDependencies(DependsOn.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    if (AppendVersion.HasValue)
                    {
                        definition.ShouldAppendVersion(AppendVersion);
                    }

                    if (!String.IsNullOrEmpty(Version))
                    {
                        definition.SetVersion(Version);
                    }

                    setting = _resourceManager.RegisterResource("script", name);
                }

                if (At != ResourceLocation.Unspecified)
                {
                    setting.AtLocation(At);
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

                if (AppendVersion.HasValue)
                {
                    setting.ShouldAppendVersion(AppendVersion);
                }

                foreach (var attribute in output.Attributes)
                {
                    setting.SetAttribute(attribute.Name, attribute.Value.ToString());
                }

                if (At == ResourceLocation.Unspecified || At == ResourceLocation.Inline)
                {
                    using var sw = new StringWriter();
                    _resourceManager.RenderLocalScript(setting, sw);
                    output.Content.AppendHtml(sw.ToString());
                }
            }
            else if (!String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Src))
            {
                // Resource required
                // <script asp-name="bootstrap"></script>

                var setting = _resourceManager.RegisterResource("script", Name);

                if (At != ResourceLocation.Unspecified)
                {
                    setting.AtLocation(At);
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

                if (AppendVersion.HasValue)
                {
                    setting.ShouldAppendVersion(AppendVersion);
                }

                if (!String.IsNullOrEmpty(Version))
                {
                    setting.UseVersion(Version);
                }

                // This allows additions to the pre registered scripts dependencies.
                if (!String.IsNullOrEmpty(DependsOn))
                {
                    setting.SetDependencies(DependsOn.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }

                foreach (var attribute in output.Attributes)
                {
                    setting.SetAttribute(attribute.Name, attribute.Value.ToString());
                }

                // Allow Inline to work with both named scripts, and named inline scripts.
                if (At != ResourceLocation.Unspecified)
                {
                    // Named inline declaration.
                    var childContent = await output.GetChildContentAsync();
                    if (!childContent.IsEmptyOrWhiteSpace)
                    {
                        // Inline content definition
                        _resourceManager.InlineManifest.DefineScript(Name)
                           .SetInnerContent(childContent.GetContent());
                    }

                    if (At == ResourceLocation.Inline)
                    {
                        using var sw = new StringWriter();
                        _resourceManager.RenderLocalScript(setting, sw);
                        output.Content.AppendHtml(sw.ToString());
                    }
                }
                else
                {
                    using var sw = new StringWriter();
                    _resourceManager.RenderLocalScript(setting, sw);
                    output.Content.AppendHtml(sw.ToString());
                }
            }
            else if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Src))
            {
                // Inline declaration

                var definition = _resourceManager.InlineManifest.DefineScript(Name);
                definition.SetUrl(Src, DebugSrc);

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
                    definition.SetCultures(Culture.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }

                if (!String.IsNullOrEmpty(DependsOn))
                {
                    definition.SetDependencies(DependsOn.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }

                if (AppendVersion.HasValue)
                {
                    definition.ShouldAppendVersion(AppendVersion);
                }

                if (!String.IsNullOrEmpty(Version))
                {
                    definition.SetVersion(Version);
                }

                // If At is specified then we also render it
                if (At != ResourceLocation.Unspecified)
                {
                    var setting = _resourceManager.RegisterResource("script", Name);

                    setting.AtLocation(At);

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

                    foreach (var attribute in output.Attributes)
                    {
                        setting.SetAttribute(attribute.Name, attribute.Value.ToString());
                    }

                    if (At == ResourceLocation.Inline)
                    {
                        using var sw = new StringWriter();
                        _resourceManager.RenderLocalScript(setting, sw);
                        output.Content.AppendHtml(sw.ToString());
                    }
                }
            }
            else if (String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Src))
            {
                // Custom script content

                var childContent = await output.GetChildContentAsync();

                var builder = new TagBuilder("script");
                builder.InnerHtml.AppendHtml(childContent);
                builder.TagRenderMode = TagRenderMode.Normal;

                foreach (var attribute in output.Attributes)
                {
                    builder.Attributes.Add(attribute.Name, attribute.Value.ToString());
                }

                if (At == ResourceLocation.Head)
                {
                    _resourceManager.RegisterHeadScript(builder);
                }
                else if (At == ResourceLocation.Inline)
                {
                    output.Content.SetHtmlContent(builder);
                }
                else
                {
                    _resourceManager.RegisterFootScript(builder);
                }
            }
        }
    }
}
