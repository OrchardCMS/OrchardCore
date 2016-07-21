using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Orchard.ResourceManagement.TagHelpers
{
    public enum ResourceType
    {
        Meta,
        HeadLink,
        Stylesheet,
        HeadScript,
        FootScript
    }

    [HtmlTargetElement("resources", Attributes = nameof(Type))]
    public class ResourcesTagHelper : TagHelper
    {
        public ResourceType Type { get; set; }

        private readonly IResourceManager _resourceManager;

        public ResourcesTagHelper(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public override void Process(TagHelperContext tagHelperContext, TagHelperOutput output)
        {

            var defaultSettings = new RequireSettings
            {
                DebugMode = true,
                CdnMode = false,
                Culture = "en-US",
            };

            switch (Type)
            {
                case ResourceType.Meta:
                    foreach (var meta in _resourceManager.GetRegisteredMetas())
                    {
                        output.Content.AppendHtml(meta.GetTag());
                        output.Content.AppendHtml(Environment.NewLine);
                    }

                    break;

                case ResourceType.HeadLink:
                    foreach (var link in _resourceManager.GetRegisteredLinks())
                    {
                        output.Content.AppendHtml(link.GetTag());
                        output.Content.AppendHtml(Environment.NewLine);
                    }

                    break;

                case ResourceType.Stylesheet:
                    var styleSheets = _resourceManager.GetRequiredResources("stylesheet");

                    foreach (var context in styleSheets)
                    {
                        output.Content.AppendHtml(context.GetTagBuilder(defaultSettings, "/"));
                        output.Content.AppendHtml(Environment.NewLine);
                    }

                    break;

                case ResourceType.HeadScript:
                    var headScripts = _resourceManager.GetRequiredResources("script");

                    foreach (var context in headScripts.Where(r => r.Settings.Location == ResourceLocation.Head))
                    {
                        output.Content.AppendHtml(context.GetTagBuilder(defaultSettings, "/"));
                        output.Content.AppendHtml(Environment.NewLine);
                    }

                    break;

                case ResourceType.FootScript:
                    var footScripts = _resourceManager.GetRequiredResources("script");

                    foreach (var context in footScripts.Where(r => r.Settings.Location == ResourceLocation.Foot))
                    {
                        output.Content.AppendHtml(context.GetTagBuilder(defaultSettings, "/"));
                        output.Content.AppendHtml(Environment.NewLine);
                    }

                    break;

                default:
                    break;
            }


            // We don't want any encapsulating tag around the shape
            output.TagName = null;
        }
    }
}
