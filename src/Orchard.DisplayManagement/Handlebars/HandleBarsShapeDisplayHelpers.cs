using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Settings;

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

            Handlebars.RegisterHelper("SiteName", async (output, context, arguments) =>
            {
                var displayContext = context.DisplayContext as DisplayContext;
                var siteService = displayContext.ServiceProvider.GetService<ISiteService>();

                var siteSettings = await siteService.GetSiteSettingsAsync();
                var siteName = siteSettings.SiteName;
                output.WriteSafeString(siteName);
            });

            Handlebars.RegisterHelper("script", (output, context, arguments) =>
            {
                var attributes = new TagHelperAttributeList(
                    (arguments[0] as IDictionary<string, object>)
                    .Select(x => new TagHelperAttribute(x.Key, x.Value))
                    .ToList());
            });

            Handlebars.RegisterHelper("RenderTitleSegments", (output, context, arguments) =>
            {
                var razorPage = context.DisplayContext.ViewContext.View.RazorPage;

                var content = razorPage.RenderTitleSegments(
                    new HtmlString(arguments[0].ToString()),
                    arguments.Count() > 1 ? arguments[1].ToString() : "0",
                    arguments.Count() > 2 ? new HtmlString(arguments[2].ToString()) : null);

                using (var writer = new StringWriter())
                {
                    content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                    output.WriteSafeString(writer.ToString());
                }
            });

            Handlebars.RegisterHelper("RenderSectionAsync", async (output, context, arguments) =>
            {
                var razorPage = context.DisplayContext.ViewContext.View.RazorPage;

                var content = await razorPage.RenderSectionAsync(
                    arguments[0].ToString(), Convert.ToBoolean(arguments[1]));

                using (var writer = new StringWriter())
                {
                    content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                    output.WriteSafeString(writer.ToString());
                }
            });

            Handlebars.RegisterHelper("ClearAlternates", (output, context, arguments) =>
            {
                context.Model.Metadata.Alternates.Clear();
            });

            Handlebars.RegisterHelper("SetMetadataType", (output, context, arguments) =>
            {
                context.Model.Metadata.Type = arguments[0].ToString();
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
                output.WriteSafeString(urlHelper.Content(arguments[0].ToString()));
            });

            Handlebars.RegisterHelper("UrlAction", (output, context, arguments) =>
            {
                var urlHelper = context.Url as IUrlHelper;

                output.WriteSafeString(urlHelper.Action(
                    action: arguments[0].ToString(),
                    controller: arguments[1].ToString(),
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
