using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.Shapes
{
    [Feature(Application.DefaultFeatureId)]
    public class JsonCodeShapes : IShapeAttributeProvider
    {
        [Shape]
        public IHtmlContent JsonCode(dynamic Shape, dynamic DisplayAsync, JObject jObject)
        {
            var tagBuilder = new TagBuilder("textarea");
            //TagBuilder tagBuilder = OrchardCore.DisplayManagement.Shapes.Shape.GetTagBuilder(Shape, "textarea");
            //JObject jObject = Shape.JObject;
            tagBuilder.AddCssClass("oc-json-code");
            //tagBuilder.InnerHtml.AppendHtml()
            tagBuilder.InnerHtml.AppendHtml(new HtmlString(HtmlEncoder.Default.Encode(jObject.ToString())));
            return tagBuilder;
        }
    }
}
