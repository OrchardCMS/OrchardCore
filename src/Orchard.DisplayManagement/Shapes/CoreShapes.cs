using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Mvc.Rendering;
using Orchard.DisplayManagement.Descriptors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Shapes
{
    public class CoreShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
        }

        [Shape]
        public void PlaceChildContent(dynamic Source, TextWriter Output)
        {
            throw new NotImplementedException();
        }

        [Shape]
        public IHtmlContent List(
            dynamic Display,
            IEnumerable<dynamic> Items,
            string Tag,
            string Id,
            IEnumerable<string> Classes,
            IDictionary<string, string> Attributes,
            string ItemTag,
            IEnumerable<string> ItemClasses,
            IDictionary<string, string> ItemAttributes)
        {

            if (Items == null)
            {
                return HtmlString.Empty;
            }

            // prevent multiple enumerations
            var items = Items.ToList();

            // var itemDisplayOutputs = Items.Select(item => Display(item)).Where(output => !string.IsNullOrWhiteSpace(output.ToHtmlString())).ToList();
            var count = items.Count();
            if (count < 1)
            {
                return HtmlString.Empty;
            }

            string listTagName = null;

            if (Tag != "-")
            {
                listTagName = string.IsNullOrEmpty(Tag) ? "ul" : Tag;
            }

            var listTag = String.IsNullOrEmpty(listTagName) ? null : Shape.GetTagBuilder(listTagName, Id, Classes, Attributes);

            string itemTagName = null;
            if (ItemTag != "-")
            {
                itemTagName = string.IsNullOrEmpty(ItemTag) ? "li" : ItemTag;
            }

            var itemContents = new List<IHtmlContent>();
            // Give the item shape the possibility to alter its container tag
            // by rendering them before rendering the containing list.

            foreach (var item in items)
            {
                itemContents.Add(Display(item));
            }

            var index = 0;
            foreach (var item in items)
            {
                var itemTag = String.IsNullOrEmpty(itemTagName) ? null : Shape.GetTagBuilder(itemTagName, null, ItemClasses, ItemAttributes);

                if (index == 0)
                {
                    itemTag.AddCssClass("first");
                }

                if (index == count - 1)
                {
                    itemTag.AddCssClass("last");
                }

                if (item is IShape)
                {
                    item.Tag = itemTag;
                }

                itemTag.InnerHtml.Append(itemContents[index]);
                listTag.InnerHtml.Append(itemTag);

                ++index;
            }

            return listTag;
        }
    }
}
