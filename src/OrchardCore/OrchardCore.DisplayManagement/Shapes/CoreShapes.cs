using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.Shapes
{
    [Feature(Application.DefaultFeatureId)]
    public class CoreShapes : IShapeAttributeProvider
    {
        [Shape]
        public void PlaceChildContent(dynamic Source, TextWriter Output)
        {
            throw new NotImplementedException();
        }

        [Shape]
#pragma warning disable CA1822 // Mark members as static
        public async Task<IHtmlContent> List(Shape shape, DisplayContext displayContext, IEnumerable<object> Items,
#pragma warning restore CA1822 // Mark members as static
            string ItemTagName,
            IEnumerable<string> ItemClasses,
            IDictionary<string, string> ItemAttributes,
            string FirstClass,
            string LastClass)
        {
            if (Items == null)
            {
                if (shape.Items != null && shape.Items.Any())
                {
                    Items = shape.Items;
                }
                else
                {
                    return HtmlString.Empty;
                }
            }

            // Prevent multiple enumerations.
            var items = Items.ToList();

            // var itemDisplayOutputs = Items.Select(item => Display(item)).Where(output => !string.IsNullOrWhiteSpace(output.ToHtmlString())).ToList();
            var count = items.Count;
            if (count < 1)
            {
                return HtmlString.Empty;
            }

            var listTagBuilder = shape.GetTagBuilder("ul");

            var itemTagName = String.IsNullOrEmpty(ItemTagName) ? "li" : ItemTagName;

            var index = 0;

            foreach (var item in items)
            {
                var itemTag = new TagBuilder(itemTagName);

                if (ItemAttributes != null)
                {
                    itemTag.MergeAttributes(ItemAttributes, false);
                }

                if (ItemClasses != null)
                {
                    foreach (var cssClass in ItemClasses)
                    {
                        itemTag.AddCssClass(cssClass);
                    }
                }

                if (index == 0)
                {
                    itemTag.AddCssClass(FirstClass ?? "first");
                }

                if (index == count - 1)
                {
                    itemTag.AddCssClass(LastClass ?? "last");
                }

                if (item is IShape itemShape)
                {
                    itemShape.Properties["Tag"] = itemTag;
                }

                // Give the item shape the possibility to alter its container tag
                // by rendering them before rendering the containing list.
                var itemContent = await displayContext.DisplayHelper.ShapeExecuteAsync((IShape)item);

                itemTag.InnerHtml.AppendHtml(itemContent);
                listTagBuilder.InnerHtml.AppendHtml(itemTag);

                ++index;
            }

            return listTagBuilder;
        }

        [Shape]
#pragma warning disable CA1822 // Mark members as static
        public IHtmlContent Message(IShape Shape)
#pragma warning restore CA1822 // Mark members as static
        {
            var tagBuilder = Shape.GetTagBuilder("div");
            var type = Shape.Properties["Type"].ToString().ToLowerInvariant();
            var message = Shape.Properties["Message"] as IHtmlContent;
            tagBuilder.AddCssClass("message");
            tagBuilder.AddCssClass("message-" + type);
            tagBuilder.Attributes["role"] = "alert";
            tagBuilder.InnerHtml.AppendHtml(message);
            return tagBuilder;
        }
    }

    public class CoreShapesTableProvider : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("List")
                .OnCreated(created =>
                {
                    dynamic list = created.Shape;

                    // Intializes the common properties of a List shape
                    // such that views can safely add values to them.
                    list.ItemClasses = new List<string>();
                    list.ItemAttributes = new Dictionary<string, string>();
                });
        }
    }
}
