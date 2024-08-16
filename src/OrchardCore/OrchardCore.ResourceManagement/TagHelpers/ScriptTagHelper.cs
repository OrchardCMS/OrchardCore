using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement.TagHelpers;

[HtmlTargetElement("script", Attributes = NameAttributeName)]
[HtmlTargetElement("script", Attributes = SrcAttributeName)]
[HtmlTargetElement("script", Attributes = AtAttributeName)]
public class ScriptTagHelper : TagHelper
{
    private static readonly char[] _separator = [',', ' '];

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

        var hasName = !string.IsNullOrEmpty(Name);
        var hasSource = !string.IsNullOrEmpty(Src);

        if (!hasName && hasSource)
        {
            // <script asp-src="~/TheBlogTheme/js/clean-blog.min.js"></script>
            RequireSettings setting;

            if (string.IsNullOrEmpty(DependsOn))
            {
                // Include custom script url
                setting = _resourceManager.RegisterUrl("script", Src, DebugSrc);
            }
            else
            {
                // Anonymous declaration with dependencies, then display

                // Using the source as the name to prevent duplicate references to the same file
                var name = Src.ToLowerInvariant();

                PopulateResourceDefinition(_resourceManager.InlineManifest.DefineScript(name));

                setting = _resourceManager.RegisterResource("script", name);
            }

            PopulateRequireSettings(setting, output, hasName: false);

            if (AppendVersion.HasValue)
            {
                setting.ShouldAppendVersion(AppendVersion);
            }

            if (At == ResourceLocation.Unspecified || At == ResourceLocation.Inline)
            {
                RenderScript(output, setting);
            }
        }
        else if (hasName && !hasSource)
        {
            // Resource required
            // <script asp-name="bootstrap"></script>

            var setting = _resourceManager.RegisterResource("script", Name);

            PopulateRequireSettings(setting, output, hasName: true);

            if (AppendVersion.HasValue)
            {
                setting.ShouldAppendVersion(AppendVersion);
            }

            if (!string.IsNullOrEmpty(Version))
            {
                setting.UseVersion(Version);
            }

            // This allows additions to the pre registered scripts dependencies.
            if (!string.IsNullOrEmpty(DependsOn))
            {
                setting.SetDependencies(DependsOn.Split(_separator, StringSplitOptions.RemoveEmptyEntries));
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
                    RenderScript(output, setting);
                }
            }
            else
            {
                RenderScript(output, setting);
            }
        }
        else if (hasName && hasSource)
        {
            // Inline declaration

            PopulateResourceDefinition(_resourceManager.InlineManifest.DefineScript(Name));

            // If At is specified then we also render it
            if (At != ResourceLocation.Unspecified)
            {
                var setting = _resourceManager.RegisterResource("script", Name);

                PopulateRequireSettings(setting, output, hasName: true);

                if (At == ResourceLocation.Inline)
                {
                    RenderScript(output, setting);
                }
            }
        }
        else
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

    private void PopulateResourceDefinition(ResourceDefinition definition)
    {
        definition.SetUrl(Src, DebugSrc);

        if (!string.IsNullOrEmpty(CdnSrc))
        {
            definition.SetCdn(CdnSrc, DebugCdnSrc);
        }

        if (!string.IsNullOrEmpty(Culture))
        {
            definition.SetCultures(Culture.Split(_separator, StringSplitOptions.RemoveEmptyEntries));
        }

        if (!string.IsNullOrEmpty(DependsOn))
        {
            definition.SetDependencies(DependsOn.Split(_separator, StringSplitOptions.RemoveEmptyEntries));
        }

        if (AppendVersion.HasValue)
        {
            definition.ShouldAppendVersion(AppendVersion);
        }

        if (!string.IsNullOrEmpty(Version))
        {
            definition.SetVersion(Version);
        }
    }

    private void PopulateRequireSettings(RequireSettings setting, TagHelperOutput output, bool hasName)
    {
        if (At != ResourceLocation.Unspecified)
        {
            setting.AtLocation(At);
        }

        if (hasName && UseCdn != null)
        {
            setting.UseCdn(UseCdn.Value);
        }

        if (!string.IsNullOrEmpty(Condition))
        {
            setting.UseCondition(Condition);
        }

        if (Debug != null)
        {
            setting.UseDebugMode(Debug.Value);
        }

        if (!string.IsNullOrEmpty(Culture))
        {
            setting.UseCulture(Culture);
        }

        foreach (var attribute in output.Attributes)
        {
            setting.SetAttribute(attribute.Name, attribute.Value.ToString());
        }
    }

    private void RenderScript(TagHelperOutput output, RequireSettings setting)
    {
        using var sw = new StringWriter();
        _resourceManager.RenderLocalScript(setting, sw);
        output.Content.AppendHtml(sw.ToString());
    }
}
