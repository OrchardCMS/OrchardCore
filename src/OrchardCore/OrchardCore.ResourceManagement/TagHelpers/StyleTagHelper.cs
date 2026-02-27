using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement.TagHelpers;

[HtmlTargetElement("style", Attributes = NameAttributeName)]
[HtmlTargetElement("style", Attributes = SrcAttributeName)]
[HtmlTargetElement("style", Attributes = AtAttributeName)]
public class StyleTagHelper : TagHelper
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

    public StyleTagHelper(IResourceManager resourceManager)
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
            // <style asp-src="~/example.css" at="Head"></style>
            RequireSettings setting;

            if (string.IsNullOrEmpty(DependsOn))
            {
                // Include custom style url.
                setting = _resourceManager.RegisterUrl("stylesheet", Src, DebugSrc);
            }
            else
            {
                // Anonymous declaration with dependencies, then display.

                // Using the source as the name to prevent duplicate references to the same file.
                var name = Src.ToLowerInvariant();

                PopulateResourceDefinition(_resourceManager.InlineManifest.DefineStyle(name));

                setting = _resourceManager.RegisterResource("stylesheet", name);
            }

            PopulateRequireSettings(setting, output, hasName: false);

            if (AppendVersion.HasValue)
            {
                setting.ShouldAppendVersion(AppendVersion);
            }

            if (At == ResourceLocation.Inline)
            {
                RenderStyle(output, setting);
            }
        }
        else if (hasName && !hasSource)
        {
            // Resource required.
            // <style asp-name="example" at="Head"></style>

            var setting = _resourceManager.RegisterResource("stylesheet", Name);

            PopulateRequireSettings(setting, output, hasName: true);

            if (AppendVersion.HasValue)
            {
                setting.ShouldAppendVersion(AppendVersion);
            }

            if (!string.IsNullOrEmpty(Version))
            {
                setting.UseVersion(Version);
            }

            // This allows additions to the pre registered style dependencies.
            if (!string.IsNullOrEmpty(DependsOn))
            {
                setting.SetDependencies(DependsOn.Split(ResourceManagementConstants.ParameterValuesSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            }

            // Allow Inline to work with both named styles, and named inline styles.
            if (At != ResourceLocation.Unspecified)
            {
                // Named inline declaration.
                var childContent = await output.GetChildContentAsync();
                if (!childContent.IsEmptyOrWhiteSpace)
                {
                    // Inline content definition.
                    _resourceManager.InlineManifest.DefineStyle(Name)
                       .SetInnerContent(childContent.GetContent());
                }

                if (At == ResourceLocation.Inline)
                {
                    RenderStyle(output, setting);
                }
            }
            else
            {
                RenderStyle(output, setting);
            }
        }
        else if (hasName && hasSource)
        {
            // Inline declaration.

            PopulateResourceDefinition(_resourceManager.InlineManifest.DefineStyle(Name));

            // If At is specified then we also render it.
            if (At != ResourceLocation.Unspecified)
            {
                var setting = _resourceManager.RegisterResource("stylesheet", Name);

                PopulateRequireSettings(setting, output, hasName: true);

                if (At == ResourceLocation.Inline)
                {
                    RenderStyle(output, setting);
                }
            }
        }
        else
        {
            // Custom style content.
            // <style at="Head"> /* example css code*/ </style>

            var childContent = await output.GetChildContentAsync();

            if (!string.IsNullOrEmpty(DependsOn))
            {
                var dependencies = DependsOn.Split(ResourceManagementConstants.ParameterValuesSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                foreach (var dependency in dependencies)
                {
                    var versionParts = dependency.Split(ResourceManagementConstants.VersionSeparator, 2);

                    var resourceName = versionParts[0];

                    var style = _resourceManager.RegisterResource("stylesheet", resourceName);

                    if (versionParts.Length == 2)
                    {
                        style.Version = versionParts[1];
                    }

                    style.AtLocation(At);
                }
            }

            var builder = new TagBuilder("style");
            builder.InnerHtml.AppendHtml(childContent);
            builder.TagRenderMode = TagRenderMode.Normal;

            foreach (var attribute in output.Attributes)
            {
                builder.Attributes.Add(attribute.Name, attribute.Value.ToString());
            }

            // If no type was specified, define a default one.
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

    private void PopulateResourceDefinition(ResourceDefinition definition)
    {
        definition.SetUrl(Src, DebugSrc);

        if (!string.IsNullOrEmpty(CdnSrc))
        {
            definition.SetCdn(CdnSrc, DebugCdnSrc);
        }

        if (!string.IsNullOrEmpty(Culture))
        {
            definition.SetCultures(Culture.Split(ResourceManagementConstants.ParameterValuesSeparator, StringSplitOptions.RemoveEmptyEntries));
        }

        if (!string.IsNullOrEmpty(DependsOn))
        {
            definition.SetDependencies(DependsOn.Split(ResourceManagementConstants.ParameterValuesSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
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
        else
        {
            setting.AtLocation(ResourceLocation.Head);
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

    private void RenderStyle(TagHelperOutput output, RequireSettings setting)
    {
        using var sw = new StringWriter();
        _resourceManager.RenderLocalStyle(setting, sw);
        output.Content.AppendHtml(sw.ToString());
    }
}
