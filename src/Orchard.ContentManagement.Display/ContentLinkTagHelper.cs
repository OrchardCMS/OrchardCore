using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Display
{
    [HtmlTargetElement("a", Attributes = ContentLinkAdmin)]
    [HtmlTargetElement("a", Attributes = ContentLinkDisplay)]
    [HtmlTargetElement("a", Attributes = ContentLinkEdit)]
    [HtmlTargetElement("a", Attributes = ContentLinkRemove)]
    [HtmlTargetElement("a", Attributes = ContentLinkCreate)]
    public class ContentLinkTagHelper : TagHelper
    {
        private const string ContentLinkAdmin = "admin-for";
        private const string ContentLinkDisplay = "display-for";
        private const string ContentLinkEdit = "edit-for";
        private const string ContentLinkRemove = "remove-for";
        private const string ContentLinkCreate = "create-for";

        private readonly IContentManager _contentManager;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentLinkTagHelper(
            IContentManager contentManager,
            IUrlHelperFactory urlHelperFactory,
            IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _urlHelperFactory = urlHelperFactory;
            _contentManager = contentManager;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Links to the admin page of this content item.
        /// </summary>
        [HtmlAttributeName(ContentLinkAdmin)]
        public ContentItem AdminFor { get; set; }

        /// <summary>
        /// Links to the display page of this content item.
        /// </summary>
        [HtmlAttributeName(ContentLinkDisplay)]
        public ContentItem DisplayFor { get; set; }

        /// <summary>
        /// Links to the edition page of this content item.
        /// </summary>
        [HtmlAttributeName(ContentLinkEdit)]
        public ContentItem EditFor { get; set; }

        /// <summary>
        /// Links to the removal page of this content item.
        /// </summary>
        [HtmlAttributeName(ContentLinkRemove)]
        public ContentItem RemoveFor { get; set; }

        /// <summary>
        /// Links to the creation page of this content item.
        /// </summary>
        [HtmlAttributeName(ContentLinkCreate)]
        public ContentItem CreateFor { get; set; }

        public override void Process(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            ContentItemMetadata metadata = null;
            ContentItem contentItem = null;

            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            if (DisplayFor != null)
            {
                contentItem = DisplayFor;
                metadata = _contentManager.GetItemMetadata(DisplayFor);

                if (metadata.DisplayRouteValues == null)
                {
                    return;
                }

                output.Attributes.SetAttribute("href", urlHelper.Action(metadata.DisplayRouteValues["action"].ToString(), metadata.DisplayRouteValues));
            }
            else if (EditFor != null)
            {
                contentItem = EditFor;
                metadata = _contentManager.GetItemMetadata(EditFor);

                if (metadata.EditorRouteValues == null)
                {
                    return;
                }

                output.Attributes.SetAttribute("href", urlHelper.Action(metadata.EditorRouteValues["action"].ToString(), metadata.EditorRouteValues));
            }
            else if (AdminFor != null)
            {
                contentItem = AdminFor;
                metadata = _contentManager.GetItemMetadata(AdminFor);

                if (metadata.AdminRouteValues == null)
                {
                    return;
                }

                output.Attributes.SetAttribute("href", urlHelper.Action(metadata.AdminRouteValues["action"].ToString(), metadata.AdminRouteValues));
            }
            else if (RemoveFor != null)
            {
                contentItem = RemoveFor;
                metadata = _contentManager.GetItemMetadata(RemoveFor);

                if (metadata.RemoveRouteValues == null)
                {
                    return;
                }

                output.Attributes.SetAttribute("href", urlHelper.Action(metadata.RemoveRouteValues["action"].ToString(), metadata.RemoveRouteValues));
            }
            else if (CreateFor != null)
            {
                contentItem = CreateFor;
                metadata = _contentManager.GetItemMetadata(CreateFor);

                if (metadata.CreateRouteValues == null)
                {
                    return;
                }

                output.Attributes.SetAttribute("href", urlHelper.Action(metadata.CreateRouteValues["action"].ToString(), metadata.CreateRouteValues));
            }

            // A self closing anchor tag will be rendered using the display text
            if (output.TagMode == TagMode.SelfClosing && metadata != null)
            {
                output.TagMode = TagMode.StartTagAndEndTag;
                if(!string.IsNullOrEmpty(metadata.DisplayText))
                {
                    output.Content.Append(metadata.DisplayText);
                }
                else
                {
                    var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
                    output.Content.Append(typeDefinition.ToString());
                }

            }

            return;
        }
    }
}
