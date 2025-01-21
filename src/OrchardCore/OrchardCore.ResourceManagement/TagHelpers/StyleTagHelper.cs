using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement.TagHelpers;

[HtmlTargetElement("style", Attributes = NameAttributeName)]
[HtmlTargetElement("style", Attributes = SrcAttributeName)]
[HtmlTargetElement("style", Attributes = AtAttributeName)]
public class StyleTagHelper : TagHelper
{
    private static readonly char[] _splitSeparators = [',', ' '];
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
            // Include custom style.
            // <style asp-src="~/example.css" at="Head"></style>

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

            if (!string.IsNullOrEmpty(Condition))
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

            if (!string.IsNullOrEmpty(Culture))
            {
                setting.UseCulture(Culture);
            }

            if (!string.IsNullOrEmpty(DependsOn))
            {
                setting.SetDependencies(DependsOn.Split(_splitSeparators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            }

            if (At == ResourceLocation.Inline)
            {
                using var sw = new StringWriter();
                _resourceManager.RenderLocalStyle(setting, sw);
                output.Content.AppendHtml(sw.ToString());
            }
        }
        else if (hasName && !hasSource)
        {
            // Resource required.
            // <style asp-name="example" at="Head"></style>

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

            if (AppendVersion.HasValue == true)
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
                setting.SetDependencies(DependsOn.Split(_splitSeparators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            }

            var childContent = await output.GetChildContentAsync();
            if (!childContent.IsEmptyOrWhiteSpace)
            {
                // Inline named style definition.
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
        else if (hasName && hasSource)
        {
            // Inline declaration.

            var definition = _resourceManager.InlineManifest.DefineStyle(Name);
            definition.SetUrl(Src, DebugSrc);

            foreach (var attribute in output.Attributes)
            {
                definition.SetAttribute(attribute.Name, attribute.Value.ToString());
            }

            if (!string.IsNullOrEmpty(Version))
            {
                definition.SetVersion(Version);
            }

            if (!string.IsNullOrEmpty(CdnSrc))
            {
                definition.SetCdn(CdnSrc, DebugCdnSrc);
            }

            if (!string.IsNullOrEmpty(Culture))
            {
                definition.SetCultures(Culture.Split(_splitSeparators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            }

            if (!string.IsNullOrEmpty(DependsOn))
            {
                definition.SetDependencies(DependsOn.Split(_splitSeparators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            }

            // Also include the style.
            var setting = _resourceManager.RegisterResource("stylesheet", Name);

            if (UseCdn != null)
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
        else
        {
            // Custom style content.
            // <style at="Head"> /* example css code*/ </style>

            var childContent = await output.GetChildContentAsync();

            if (!string.IsNullOrEmpty(DependsOn))
            {
                var dependencies = DependsOn.Split(_splitSeparators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                foreach (var dependency in dependencies)
                {
                    var versionParts = dependency.Split(':', 2);

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
}
