using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class AnchorTag : IAnchorTag
    {
        public int Order => 0;

        public bool Match(List<FilterArgument> argumentsList)
        {
            return true;
        }

        public async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, IReadOnlyList<Statement> statements, TextWriter writer, TextEncoder encoder, LiquidTemplateContext context)
        {
            var services = context.Services;
            var viewContext = context.ViewContext;
            var generator = services.GetRequiredService<IHtmlGenerator>();

            string action = null;
            string controller = null;
            string area = null;
            string page = null;
            string pageHandler = null;
            string fragment = null;
            string host = null;
            string protocol = null;
            string route = null;
            string href = null;

            Dictionary<string, string> routeValues = null;
            Dictionary<string, string> customAttributes = null;

            foreach (var argument in argumentsList)
            {
                switch (argument.Name)
                {
                    case "action": action = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "controller": controller = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "area": area = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "page": page = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "page_handler": pageHandler = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "fragment": fragment = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "host": host = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "protocol": protocol = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "route": route = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;

                    case "all_route_data":

                        var allRouteData = (await argument.Expression.EvaluateAsync(context)).ToObjectValue();

                        if (allRouteData is Dictionary<string, string> allRouteValues)
                        {
                            routeValues = allRouteValues;
                        }

                        break;

                    case "href": href = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;

                    default:

                        if (argument.Name.StartsWith("route_"))
                        {
                            routeValues ??= new Dictionary<string, string>();
                            routeValues[argument.Name[6..]] = (await argument.Expression.EvaluateAsync(context)).ToStringValue();
                        }
                        else
                        {
                            customAttributes ??= new Dictionary<string, string>();
                            customAttributes[argument.Name] = (await argument.Expression.EvaluateAsync(context)).ToStringValue();
                        }

                        break;
                }
            }

            // If "href" is already set, it means the user is attempting to use a normal anchor.
            if (!String.IsNullOrEmpty(href))
            {
                if (action != null ||
                    controller != null ||
                    area != null ||
                    page != null ||
                    pageHandler != null ||
                    route != null ||
                    protocol != null ||
                    host != null ||
                    fragment != null ||
                    (routeValues != null && routeValues.Count > 0))
                {
                    // User specified an href and one of the bound attributes; can't determine the href attribute.
                    throw new InvalidOperationException("Cannot override href with other properties");
                }

                return Completion.Normal;
            }

            var routeLink = route != null;
            var actionLink = controller != null || action != null;
            var pageLink = page != null || pageHandler != null;

            if ((routeLink && actionLink) || (routeLink && pageLink) || (actionLink && pageLink))
            {
                throw new InvalidOperationException("Format of the link cannot be determined based on the available argument of the a tag");
            }

            RouteValueDictionary localRouteValues = null;
            if (routeValues != null && routeValues.Count > 0)
            {
                localRouteValues = new RouteValueDictionary(routeValues);
            }

            if (area != null)
            {
                // Unconditionally replace any value from asp-route-area.
                localRouteValues ??= new RouteValueDictionary();
                localRouteValues["area"] = area;
            }

            TagBuilder tagBuilder;
            if (pageLink)
            {
                tagBuilder = generator.GeneratePageLink(
                    viewContext,
                    linkText: String.Empty,
                    pageName: page,
                    pageHandler: pageHandler,
                    protocol: protocol,
                    hostname: host,
                    fragment: fragment,
                    routeValues: localRouteValues,
                    htmlAttributes: null);
            }
            else if (routeLink)
            {
                tagBuilder = generator.GenerateRouteLink(
                    viewContext,
                    linkText: String.Empty,
                    routeName: route,
                    protocol: protocol,
                    hostName: host,
                    fragment: fragment,
                    routeValues: localRouteValues,
                    htmlAttributes: null);
            }
            else
            {
                tagBuilder = generator.GenerateActionLink(
                   viewContext,
                   linkText: String.Empty,
                   actionName: action,
                   controllerName: controller,
                   protocol: protocol,
                   hostname: host,
                   fragment: fragment,
                   routeValues: localRouteValues,
                   htmlAttributes: null);
            }

            foreach (var attribute in customAttributes)
            {
                tagBuilder.Attributes[attribute.Key] = attribute.Value;
            }

            tagBuilder.RenderStartTag().WriteTo(writer, (HtmlEncoder)encoder);

            if (statements != null && statements.Count > 0)
            {
                var completion = await statements.RenderStatementsAsync(writer, encoder, context);

                if (completion != Completion.Normal)
                {
                    return completion;
                }
            }

            tagBuilder.RenderEndTag().WriteTo(writer, (HtmlEncoder)encoder);

            return Completion.Normal;
        }
    }
}
