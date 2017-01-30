using System.Collections.Generic;
using System.IO;
using System.Linq;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement.HandleBars
{
    public static class HandleBarsShapeDisplayHelpers
    {
        public static void RegisterShapeDisplayHelpers()
        {
            Handlebars.RegisterHelper("foreach", (output, options, context, arguments) =>
            {
                foreach (var shape in (IEnumerable<dynamic>)arguments[0])
                {
                    if (shape is Shape)
                    {
                        shape.DisplayContext = context.DisplayContext;
                        shape.Html = context.Html;
                        shape.Url = context.Url;
                    }

                    options.Template(output, shape);
                }
            });

            Handlebars.RegisterHelper("Display", async (output, context, arguments) =>
            {
                var displayContext = context.DisplayContext as DisplayContext;
                var shape = arguments.Count() > 0 ? arguments[0] : (object)context;
                output.WriteSafeString(await displayContext.DisplayAsync.ShapeExecuteAsync(shape));
            });

            Handlebars.RegisterHelper("UrlContent", (output, context, arguments) =>
            {
                var urlHelper = context.Url as IUrlHelper;
                output.WriteSafeString(urlHelper.Content((string)arguments[0]));
            });

            Handlebars.RegisterHelper("UrlAction", (output, context, arguments) =>
            {
                var urlHelper = context.Url as IUrlHelper;

                output.WriteSafeString(urlHelper.Action(
                    action: (string)arguments[0],
                    controller: (string)arguments[1],
                    values: arguments.Count() > 2 ? arguments[2] : new { }));
            });

            Handlebars.RegisterHelper("HtmlRaw", (output, context, arguments) =>
            {
                output.WriteSafeString((string)arguments[0]);
            });

            Handlebars.RegisterHelper("HtmlActionLink", (output, context, arguments) =>
            {
                var htmlHelper = context.Html as IHtmlHelper;
                var content = (TagBuilder)htmlHelper.ActionLink(
                    linkText: (string)arguments[0],
                    actionName: (string)arguments[1],
                    routeValues: new { },
                    htmlAttributes: arguments.Count() > 2 ? arguments[2] : new { });

                using (var writer = new StringWriter())
                {
                    content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                    output.WriteSafeString(writer.ToString());
                }
            });
        }
    }
}
