using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Users.Shapes;

namespace OrchardCore.Users.Shapes
{
    public class AfterLoginShapes : IShapeTableProvider, IShapeAttributeProvider
    {

        public void Discover(ShapeTableBuilder builder)
        {
            builder
                .Describe("AfterLogin");
        }

        [Shape]
        public async Task<IHtmlContent> AfterLogin(Shape shape, dynamic displayAsync)
        {
            var tag = Shape.GetTagBuilder(shape);
            foreach (var item in shape.Items)
            {
                tag.InnerHtml.AppendHtml(await displayAsync(item));
            }
            return tag;
        }
    }
}