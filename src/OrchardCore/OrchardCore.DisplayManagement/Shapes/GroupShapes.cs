using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.ViewModels;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Modules;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Shapes
{
    [Feature(Application.DefaultFeatureId)]
    public class GroupShapes : IShapeAttributeProvider
    {
        [Shape]
        public IHtmlContent AdminTabs(IShape Shape)
        {
            var tabsGrouping = Shape.GetProperty<IList<IGrouping<string, object>>>("Tabs");
            var tabs = tabsGrouping.Select(x => x.Key).ToArray();

            if (tabs.Any())
            {
                var identifier = Shape.GetProperty<string>("Identifier");
                var tabIndex = 0;
                var tagBuilder = Shape.GetTagBuilder("ul");
                tagBuilder.AddCssClass("nav nav-tabs flex-column flex-md-row");
                tagBuilder.Attributes["role"] = "tablist";

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
            else
            {
                return HtmlString.Empty;
            }
        }

        [Shape]
        public async Task<IHtmlContent> Card(IDisplayHelper DisplayAsync, GroupingViewModel Shape)
        {
            var tagBuilder = Shape.GetTagBuilder("div");
            tagBuilder.AddCssClass("card-body");
            Shape.Metadata.Alternates.Clear();
            Shape.Metadata.Type = "ColumnGrouping";

            tagBuilder.InnerHtml.AppendHtml(await DisplayAsync.ShapeExecuteAsync(Shape));
            var cardIdPrefix = $"card-{Shape.Identifier}-{Shape.Grouping.Key}".HtmlClassify();

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

            buttonTag.InnerHtml.Append(Shape.Grouping.Key);
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
        public async Task<IHtmlContent> CardContainer(IDisplayHelper DisplayAsync, GroupViewModel Shape)
        {
            var tagBuilder = Shape.GetTagBuilder("div");
            tagBuilder.AddCssClass("mb-3");

            foreach (var card in Shape.Items.OfType<IShape>())
            {
                tagBuilder.InnerHtml.AppendHtml(await DisplayAsync.ShapeExecuteAsync(card));
            }

            return tagBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> Column(IDisplayHelper DisplayAsync, GroupViewModel Shape)
        {
            var tagBuilder = Shape.GetTagBuilder("div");

            foreach (var column in Shape.Items)
            {
                tagBuilder.InnerHtml.AppendHtml(await DisplayAsync.ShapeExecuteAsync((IShape)column));
            }

            return tagBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> ColumnContainer(IDisplayHelper DisplayAsync, GroupViewModel Shape)
        {
            var tagBuilder = Shape.GetTagBuilder("div");
            tagBuilder.AddCssClass("row");
            
            foreach (var column in Shape.Items)
            {
                tagBuilder.InnerHtml.AppendHtml(await DisplayAsync.ShapeExecuteAsync((IShape)column));
            }

            return tagBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> Tab(IDisplayHelper DisplayAsync, GroupingViewModel Shape)
        {
            var tagBuilder = Shape.GetTagBuilder("div");
            tagBuilder.Attributes["id"] = $"tab-{Shape.Grouping.Key}-{Shape.Identifier}".HtmlClassify();
            tagBuilder.AddCssClass("tab-pane fade");

            // Morphing this shape to a grouping shape to keep Model untouched
            Shape.Metadata.Alternates.Clear();
            Shape.Metadata.Type = "CardGrouping";

            tagBuilder.InnerHtml.AppendHtml(await DisplayAsync.ShapeExecuteAsync(Shape));

            return tagBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> TabContainer(IDisplayHelper DisplayAsync, GroupingsViewModel Shape, IShapeFactory ShapeFactory)
        {
            var first = true;
            var localNavigation = await ShapeFactory.CreateAsync("LocalNavigation", Arguments.From(new
            {
                Identifier = Shape.Identifier,
                Tabs = Shape.Groupings
            }));

            var htmlContentBuilder = new HtmlContentBuilder();
            htmlContentBuilder.AppendHtml(await DisplayAsync.ShapeExecuteAsync(localNavigation));

            var tagBuilder = Shape.GetTagBuilder("div");
            tagBuilder.AddCssClass("tab-content");

            htmlContentBuilder.AppendHtml(tagBuilder);

            foreach (var tab in Shape.Items.OfType<IShape>())
            {
                if (first)
                {
                    tab.Classes.Add("show active");
                }
                first = false;
                tagBuilder.InnerHtml.AppendHtml(await DisplayAsync.ShapeExecuteAsync(tab));
            }

            return htmlContentBuilder;
        }

        [Shape]
        public Task<IHtmlContent> LocalNavigation(IDisplayHelper DisplayAsync, IShape Shape, IShapeFactory ShapeFactory)
        {
            // Morphing this shape to keep Model untouched
            Shape.Metadata.Alternates.Clear();
            Shape.Metadata.Type = "AdminTabs";

            return DisplayAsync.ShapeExecuteAsync(Shape);
        }
    }
}
