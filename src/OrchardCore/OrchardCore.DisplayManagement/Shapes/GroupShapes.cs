using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Shapes
{
    [Feature(Application.DefaultFeatureId)]
    public class GroupShapes : IShapeAttributeProvider
    {
        [Shape]
        public IHtmlContent AdminTabs(IShape shape)
        {
            var tabsGrouping = shape.GetProperty<IList<IGrouping<string, object>>>("Tabs");
            if (tabsGrouping == null)
            {
                return HtmlString.Empty;
            }

            var tabs = tabsGrouping.Select(group => group.Key).ToArray();
            if (!tabs.Any())
            {
                return HtmlString.Empty;
            }

            var tagBuilder = shape.GetTagBuilder("ul");
            var identifier = shape.GetProperty<string>("Identifier");
            tagBuilder.AddCssClass("nav nav-tabs flex-column flex-md-row");
            tagBuilder.Attributes["role"] = "tablist";

            var tabIndex = 0;
            foreach (var tab in tabs)
            {
                var linkTag = new TagBuilder("li");
                linkTag.AddCssClass("nav-item");

                if (tabIndex != tabs.Length - 1)
                {
                    linkTag.AddCssClass("pr-md-2");
                }

                var aTag = new TagBuilder("a");
                aTag.AddCssClass("nav-item nav-link");

                if (tabIndex == 0)
                {
                    aTag.AddCssClass("active");
                }

                var tabId = $"tab-{tab}-{identifier}".HtmlClassify();

                aTag.Attributes["href"] = '#' + tabId;
                aTag.Attributes["data-toggle"] = "tab";
                aTag.Attributes["role"] = "tab";
                aTag.Attributes["aria-controls"] = tabId;
                aTag.Attributes["aria-selected"] = "false";
                aTag.InnerHtml.Append(tab);

                linkTag.InnerHtml.AppendHtml(aTag);
                tagBuilder.InnerHtml.AppendHtml(linkTag);
                tabIndex++;
            }

            return tagBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> Card(IDisplayHelper displayAsync, GroupingViewModel shape)
        {
            var tagBuilder = shape.GetTagBuilder("div");
            tagBuilder.AddCssClass("card-body");
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "ColumnGrouping";

            tagBuilder.InnerHtml.AppendHtml(await displayAsync.ShapeExecuteAsync(shape));
            var cardIdPrefix = $"card-{shape.Identifier}-{shape.Grouping.Key}".HtmlClassify();

            var cardTag = new TagBuilder("div");
            cardTag.AddCssClass("card");

            var headerTag = new TagBuilder("div");
            headerTag.AddCssClass("card-header");
            headerTag.Attributes["id"] = $"heading-{cardIdPrefix}";

            var buttonTag = new TagBuilder("button");
            buttonTag.AddCssClass("btn btn-link btn-block text-left");
            buttonTag.Attributes["type"] = "button";
            buttonTag.Attributes["data-toggle"] = "collapse";
            buttonTag.Attributes["data-target"] = $"#collapse-{cardIdPrefix}";
            buttonTag.Attributes["aria-expanded"] = "true";
            buttonTag.Attributes["aria-controls"] = $"collapse-{cardIdPrefix}";

            buttonTag.InnerHtml.Append(shape.Grouping.Key);
            headerTag.InnerHtml.AppendHtml(buttonTag);

            var bodyTag = new TagBuilder("div");
            bodyTag.AddCssClass("collapse show");
            bodyTag.Attributes["id"] = $"collapse-{cardIdPrefix}";

            bodyTag.InnerHtml.AppendHtml(tagBuilder);
            cardTag.InnerHtml.AppendHtml(headerTag);
            cardTag.InnerHtml.AppendHtml(bodyTag);

            return cardTag;
        }

        [Shape]
        public async Task<IHtmlContent> CardContainer(IDisplayHelper displayAsync, GroupViewModel shape)
        {
            var tagBuilder = shape.GetTagBuilder("div");
            tagBuilder.AddCssClass("mb-3");

            foreach (var card in shape.Items.OfType<IShape>())
            {
                tagBuilder.InnerHtml.AppendHtml(await displayAsync.ShapeExecuteAsync(card));
            }

            return tagBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> Column(IDisplayHelper displayAsync, GroupViewModel shape)
        {
            var tagBuilder = shape.GetTagBuilder("div");
            foreach (var column in shape.Items)
            {
                tagBuilder.InnerHtml.AppendHtml(await displayAsync.ShapeExecuteAsync((IShape)column));
            }

            return tagBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> ColumnContainer(IDisplayHelper displayAsync, GroupViewModel shape)
        {
            var tagBuilder = shape.GetTagBuilder("div");
            tagBuilder.AddCssClass("row");

            foreach (var column in shape.Items)
            {
                tagBuilder.InnerHtml.AppendHtml(await displayAsync.ShapeExecuteAsync((IShape)column));
            }

            return tagBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> Tab(IDisplayHelper displayAsync, GroupingViewModel shape)
        {
            var tagBuilder = shape.GetTagBuilder("div");
            tagBuilder.Attributes["id"] = $"tab-{shape.Grouping.Key}-{shape.Identifier}".HtmlClassify();
            tagBuilder.AddCssClass("tab-pane fade");

            // Morphing this shape to a grouping shape to keep Model untouched.
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "CardGrouping";

            tagBuilder.InnerHtml.AppendHtml(await displayAsync.ShapeExecuteAsync(shape));

            return tagBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> TabContainer(IDisplayHelper displayAsync, GroupingsViewModel shape, IShapeFactory shapeFactory)
        {
            var localNavigation = await shapeFactory.CreateAsync("LocalNavigation", Arguments.From(new
            {
                shape.Identifier,
                Tabs = shape.Groupings
            }));

            var htmlContentBuilder = new HtmlContentBuilder();
            htmlContentBuilder.AppendHtml(await displayAsync.ShapeExecuteAsync(localNavigation));

            var tagBuilder = shape.GetTagBuilder("div");
            tagBuilder.AddCssClass("tab-content");

            htmlContentBuilder.AppendHtml(tagBuilder);

            var first = true;
            foreach (var tab in shape.Items.OfType<IShape>())
            {
                if (first)
                {
                    first = false;
                    tab.Classes.Add("show active");
                }

                tagBuilder.InnerHtml.AppendHtml(await displayAsync.ShapeExecuteAsync(tab));
            }

            return htmlContentBuilder;
        }

        [Shape]
        public Task<IHtmlContent> LocalNavigation(IDisplayHelper displayAsync, IShape shape)
        {
            // Morphing this shape to keep Model untouched.
            shape.Metadata.Alternates.Clear();
            shape.Metadata.Type = "AdminTabs";

            return displayAsync.ShapeExecuteAsync(shape);
        }
    }
}
