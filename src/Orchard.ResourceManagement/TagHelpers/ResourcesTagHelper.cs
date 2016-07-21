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

            bool first = true; ;

            switch (Type)
            {
                case ResourceType.Meta:
                    foreach (var meta in _resourceManager.GetRegisteredMetas())
                    {
                        if (!first)
                        {
                            output.Content.AppendHtml(Environment.NewLine);
                        }

                        first = false;

                        output.Content.AppendHtml(meta.GetTag());
                    }

                    break;

                case ResourceType.HeadLink:
                    foreach (var link in _resourceManager.GetRegisteredLinks())
                    {
                        if (!first)
                        {
                            output.Content.AppendHtml(Environment.NewLine);
                        }

                        first = false;

                        output.Content.AppendHtml(link.GetTag());
                    }

                    break;

                case ResourceType.Stylesheet:
                    var styleSheets = _resourceManager.GetRequiredResources("stylesheet");

                    foreach (var context in styleSheets)
                    {
                        if (!first)
                        {
                            output.Content.AppendHtml(Environment.NewLine);
                        }

                        first = false;

                        output.Content.AppendHtml(context.GetTagBuilder(defaultSettings, "/"));
                    }

                    break;

                case ResourceType.HeadScript:
                    var headScripts = _resourceManager.GetRequiredResources("script");

                    foreach (var context in headScripts.Where(r => r.Settings.Location == ResourceLocation.Head))
                    {
                        if (!first)
                        {
                            output.Content.AppendHtml(Environment.NewLine);
                        }

                        first = false;

                        output.Content.AppendHtml(context.GetTagBuilder(defaultSettings, "/"));
                    }

                    break;

                case ResourceType.FootScript:
                    var footScripts = _resourceManager.GetRequiredResources("script");

                    foreach (var context in footScripts.Where(r => r.Settings.Location == ResourceLocation.Foot))
                    {
                        if (!first)
                        {
                            output.Content.AppendHtml(Environment.NewLine);
                        }

                        first = false;

                        output.Content.AppendHtml(context.GetTagBuilder(defaultSettings, "/"));
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
