using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.DisplayManagement.TagHelpers;
using Orchard.ResourceManagement;
using Orchard.ResourceManagement.TagHelpers;
using Orchard.Settings;

namespace Orchard.DisplayManagement.HandleBars
{
    public static class HandleBarsShapeDisplayHelpers
    {
        public static void RegisterShapeDisplayHelpers()
        {
            Handlebars.RegisterHelper("foreach", (output, options, context, arguments) =>
            {
                if (arguments.Any())
                {
                    foreach (var shape in (IEnumerable<dynamic>)arguments[0])
                    {
                        if (shape is Shape)
                        {
                            shape.DisplayContext = context.DisplayContext;
                        }

                        options.Template(output, shape);
                    }
                }
            });

            Handlebars.RegisterHelper("T", (output, context, arguments) =>
            {
                if (arguments.Any())
                {
                    output.WriteSafeString(
                        (new HelperContext(context)
                        .T[arguments[0].ToString()])
                        .Value);
                }
            });

            Handlebars.RegisterHelper("SiteName", async (output, context, arguments) =>
            {
                output.Write(
                    (await new HelperContext(context)
                    .GetService<ISiteService>()
                    .GetSiteSettingsAsync())
                    .SiteName);
            });

            Handlebars.RegisterHelper("IsAuthenticated", (output, context, arguments) =>
            {
                output.Write(
                    new HelperContext(context)
                    .User.Identity.IsAuthenticated
                    ? "yes" : String.Empty);
            });

            Handlebars.RegisterHelper("UserName", (output, context, arguments) =>
            {
                output.Write(
                    new HelperContext(context)
                    .User.Identity.Name);
            });

            Handlebars.RegisterHelper("menu", (output, context, arguments) =>
            {
                if (!arguments.Any())
                {
                    return;
                }

                var helperContext = new HelperContext(context);

                var attributes = new TagHelperAttributeList(
                    (arguments[0] as IDictionary<string, object>)
                    .Select(x => new TagHelperAttribute(x.Key, x.Value)));

                var tagHelperContext = new TagHelperContext(attributes,
                    new Dictionary<object, object>(),
                    Guid.NewGuid().ToString("N"));

                var tagHelperOutput = new TagHelperOutput("menu", attributes,
                    getChildContentAsync: (useCachedResult, encoder) =>
                        Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

                var shapeTagHelper =
                    new ShapeTagHelper(
                        helperContext.GetService<IShapeFactory>(),
                        helperContext.GetService<IDisplayHelperFactory>())
                    {
                        ViewContext = helperContext.ViewContext
                    };

                shapeTagHelper.ProcessAsync(tagHelperContext, tagHelperOutput).GetAwaiter().GetResult();

                using (var writer = new StringWriter())
                {
                    tagHelperOutput.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                    output.WriteSafeString(writer.ToString());
                }
            });

            Handlebars.RegisterHelper("form", (output, context, arguments) =>
            {
                if (!arguments.Any())
                {
                    return;
                }

                var helperContext = new HelperContext(context);

                var attributes = new TagHelperAttributeList(
                    (arguments[0] as IDictionary<string, object>)
                    .Select(x => new TagHelperAttribute(x.Key, x.Value)));

                var formTagHelper =
                    new Microsoft.AspNetCore.Mvc.TagHelpers
                    .FormTagHelper(
                        helperContext.GetService<IHtmlGenerator>())
                    {
                        ViewContext = helperContext.ViewContext
                    };

                TagHelperAttribute attribute;

                if (attributes.TryGetAttribute("asp-action", out attribute))
                {
                    formTagHelper.Action = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-antiforgery", out attribute))
                {
                    formTagHelper.Antiforgery = Convert.ToBoolean(attribute.Value);
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-area", out attribute))
                {
                    formTagHelper.Area = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-fragment", out attribute))
                {
                    formTagHelper.Fragment = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-controller", out attribute))
                {
                    formTagHelper.Controller = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-route", out attribute))
                {
                    formTagHelper.Route = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-all-route-data", out attribute))
                {
                    formTagHelper.RouteValues = attribute.Value as IDictionary<string, string>;
                    attributes.Remove(attribute);
                }

                foreach (var routeAttribute in attributes
                    .Where(a => a.Name.StartsWith("asp-route-"))
                    .ToList())
                {
                    formTagHelper.RouteValues.Add(
                        routeAttribute.Name.Replace("asp-route-", String.Empty),
                        routeAttribute.Value.ToString());

                    attributes.Remove(routeAttribute);
                }

                if (attributes.TryGetAttribute("method", out attribute))
                {
                    if (!formTagHelper.Antiforgery.HasValue)
                    {
                        formTagHelper.Antiforgery = !attribute.Value.ToString()
                            .Equals("get", StringComparison.OrdinalIgnoreCase);
                    }
                }

                var tagHelperContext = new TagHelperContext(attributes,
                    new Dictionary<object, object>(),
                    Guid.NewGuid().ToString("N"));

                var tagHelperOutput = new TagHelperOutput("form", attributes,
                    getChildContentAsync: (useCachedResult, encoder) =>
                        Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

                formTagHelper.Process(tagHelperContext, tagHelperOutput);

                if (!tagHelperOutput.PostContent.IsEmptyOrWhiteSpace)
                {
                    helperContext.FormContext.EndOfFormContent.Add(tagHelperOutput.PostContent);
                }

                using (var writer = new StringWriter())
                {
                    tagHelperOutput.TagMode = TagMode.StartTagOnly;
                    tagHelperOutput.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                    output.WriteSafeString(writer.ToString());
                }
            });

            Handlebars.RegisterHelper("/form", (output, context, arguments) =>
            {
                var helperContext = new HelperContext(context);

                if (helperContext.FormContext.HasEndOfFormContent)
                {
                    foreach (var content in helperContext.FormContext.EndOfFormContent)
                    {
                        using (var writer = new StringWriter())
                        {
                            content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                            output.WriteSafeString(writer.ToString());
                        }
                    }
                }

                output.WriteSafeString("</form>");
                helperContext.FormContext = new FormContext();
            });

            Handlebars.RegisterHelper("a", (output, context, arguments) =>
            {
                if (!arguments.Any())
                {
                    return;
                }

                var helperContext = new HelperContext(context);

                var attributes = new TagHelperAttributeList(
                    (arguments[0] as IDictionary<string, object>)
                    .Select(x => new TagHelperAttribute(x.Key, x.Value)));

                var anchorTagHelper =
                    new Microsoft.AspNetCore.Mvc.TagHelpers
                    .AnchorTagHelper(
                        helperContext.GetService<IHtmlGenerator>())
                    {
                        ViewContext = helperContext.ViewContext
                    };

                TagHelperAttribute attribute;
                if (attributes.TryGetAttribute("asp-action", out attribute))
                {
                    anchorTagHelper.Action = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-controller", out attribute))
                {
                    anchorTagHelper.Controller = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-area", out attribute))
                {
                    anchorTagHelper.Area = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-fragment", out attribute))
                {
                    anchorTagHelper.Fragment = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-host", out attribute))
                {
                    anchorTagHelper.Host = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-protocol", out attribute))
                {
                    anchorTagHelper.Protocol = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-route", out attribute))
                {
                    anchorTagHelper.Route = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-all-route-data", out attribute))
                {
                    anchorTagHelper.RouteValues = attribute.Value as IDictionary<string, string>;
                    attributes.Remove(attribute);
                }

                foreach (var routeAttribute in attributes
                    .Where(a => a.Name.StartsWith("asp-route-"))
                    .ToList())
                {
                    anchorTagHelper.RouteValues.Add(
                        routeAttribute.Name.Replace("asp-route-", String.Empty),
                        routeAttribute.Value.ToString());

                    attributes.Remove(routeAttribute);
                }

                var tagHelperContext = new TagHelperContext(attributes,
                    new Dictionary<object, object>(),
                    Guid.NewGuid().ToString("N"));

                var tagHelperOutput = new TagHelperOutput("a", attributes,
                    getChildContentAsync: (useCachedResult, encoder) =>
                        Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

                anchorTagHelper.Process(tagHelperContext, tagHelperOutput);

                using (var writer = new StringWriter())
                {
                    tagHelperOutput.TagMode = TagMode.StartTagOnly;
                    tagHelperOutput.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                    output.WriteSafeString(writer.ToString());
                }
            });

            Handlebars.RegisterHelper("/a", (output, context, arguments) =>
            {
                output.WriteSafeString("</a>");
            });

            Handlebars.RegisterHelper("script", (output, context, arguments) =>
            {
                if (!arguments.Any())
                {
                    return;
                }

                var helperContext = new HelperContext(context);

                TagHelperAttributeList attributes;
                if (arguments.Count() > 1)
                {
                    attributes = new TagHelperAttributeList();
                    attributes.Add("at", arguments[0]);
                }
                else
                {
                    attributes = new TagHelperAttributeList(
                        (arguments[0] as IDictionary<string, object>)
                        .Select(x => new TagHelperAttribute(x.Key, x.Value)));
                }

                var scriptTagHelper = new ScriptTagHelper(helperContext.GetService<IResourceManager>());

                TagHelperAttribute attribute;
                if (attributes.TryGetAttribute("asp-name", out attribute))
                {
                    scriptTagHelper.Name = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-src", out attribute))
                {
                    scriptTagHelper.Src = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("cdn-src", out attribute))
                {
                    scriptTagHelper.CdnSrc = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("debug-src", out attribute))
                {
                    scriptTagHelper.DebugSrc = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("debug-cdn-src", out attribute))
                {
                    scriptTagHelper.DebugCdnSrc = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("use-cdn", out attribute))
                {
                    scriptTagHelper.UseCdn = Convert.ToBoolean(attribute.Value);
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("condition", out attribute))
                {
                    scriptTagHelper.Condition = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("culture", out attribute))
                {
                    scriptTagHelper.Culture = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("debug", out attribute))
                {
                    scriptTagHelper.Debug = Convert.ToBoolean(attribute.Value);
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("depend-on", out attribute))
                {
                    scriptTagHelper.DependsOn = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("version", out attribute))
                {
                    scriptTagHelper.Version = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("at", out attribute))
                {
                    scriptTagHelper.At = (ResourceLocation)Enum.Parse(typeof(ResourceLocation), attribute.Value.ToString());
                    attributes.Remove(attribute);
                }

                string content = String.Empty;
                if (arguments.Count() > 1)
                {
                    content = arguments[1].ToString();
                }

                var tagHelperContext = new TagHelperContext(attributes,
                    new Dictionary<object, object>(),
                    Guid.NewGuid().ToString("N"));

                var tagHelperOutput = new TagHelperOutput("script", attributes,
                    getChildContentAsync: (useCachedResult, encoder) =>
                        Task.FromResult(new DefaultTagHelperContent().AppendHtml(content)));

                scriptTagHelper.Process(tagHelperContext, tagHelperOutput);
            });

            Handlebars.RegisterHelper("style", (output, context, arguments) =>
            {
                if (!arguments.Any())
                {
                    return;
                }

                var helperContext = new HelperContext(context);

                var attributes = new TagHelperAttributeList(
                    (arguments[0] as IDictionary<string, object>)
                    .Select(x => new TagHelperAttribute(x.Key, x.Value)));

                var styleTagHelper = new StyletTagHelper(helperContext.GetService<IResourceManager>());

                TagHelperAttribute attribute;
                if (attributes.TryGetAttribute("asp-name", out attribute))
                {
                    styleTagHelper.Name = attribute.Value.ToString();
                }

                if (attributes.TryGetAttribute("asp-src", out attribute))
                {
                    styleTagHelper.Src = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("cdn-src", out attribute))
                {
                    styleTagHelper.CdnSrc = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("debug-src", out attribute))
                {
                    styleTagHelper.DebugSrc = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("debug-cdn-src", out attribute))
                {
                    styleTagHelper.DebugCdnSrc = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("use-cdn", out attribute))
                {
                    styleTagHelper.UseCdn = Convert.ToBoolean(attribute.Value);
                }

                if (attributes.TryGetAttribute("condition", out attribute))
                {
                    styleTagHelper.Condition = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("culture", out attribute))
                {
                    styleTagHelper.Culture = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("debug", out attribute))
                {
                    styleTagHelper.Debug = Convert.ToBoolean(attribute.Value);
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("dependencies", out attribute))
                {
                    styleTagHelper.Dependencies = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("version", out attribute))
                {
                    styleTagHelper.Version = attribute.Value.ToString();
                }

                if (attributes.TryGetAttribute("at", out attribute))
                {
                    styleTagHelper.At = (ResourceLocation)Enum.Parse(typeof(ResourceLocation), attribute.Value.ToString());
                }

                var tagHelperContext = new TagHelperContext(attributes,
                    new Dictionary<object, object>(),
                    Guid.NewGuid().ToString("N"));

                var tagHelperOutput = new TagHelperOutput("script", attributes,
                    getChildContentAsync: (useCachedResult, encoder) =>
                        Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

                styleTagHelper.Process(tagHelperContext, tagHelperOutput);
            });

            Handlebars.RegisterHelper("resources", (output, context, arguments) =>
            {
                if (!arguments.Any())
                {
                    return;
                }

                var helperContext = new HelperContext(context);

                var attributes = new TagHelperAttributeList(
                    (arguments[0] as IDictionary<string, object>)
                    .Select(x => new TagHelperAttribute(x.Key, x.Value)));

                var resourcesTagHelper = new ResourcesTagHelper(
                    helperContext.GetService<IResourceManager>(),
                    helperContext.GetService<IRequireSettingsProvider>());

                TagHelperAttribute attribute;
                if (attributes.TryGetAttribute("type", out attribute))
                {
                    resourcesTagHelper.Type = (ResourceType)Enum.Parse(typeof(ResourceType), attribute.Value.ToString());
                }

                var tagHelperContext = new TagHelperContext(attributes,
                    new Dictionary<object, object>(),
                    Guid.NewGuid().ToString("N"));

                var tagHelperOutput = new TagHelperOutput("script", attributes,
                    getChildContentAsync: (useCachedResult, encoder) =>
                        Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

                resourcesTagHelper.Process(tagHelperContext, tagHelperOutput);

                using (var writer = new StringWriter())
                {
                    tagHelperOutput.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                    output.WriteSafeString(writer.ToString());
                }
            });

            Handlebars.RegisterHelper("RenderTitleSegments", (output, context, arguments) =>
            {
                if (!arguments.Any())
                {
                    return;
                }

                var content =
                    new HelperContext(context)
                    .RenderTitleSegments(
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
                if (arguments.Any())
                {
                    output.WriteSafeString(
                        await new HelperContext(context)
                        .RenderSectionAsync(
                            arguments[0].ToString(),
                            Convert.ToBoolean(arguments[1])));
                }
            });

            Handlebars.RegisterHelper("RenderBodyAsync", async (output, context, arguments) =>
            {
                output.WriteSafeString(
                    await new HelperContext(context)
                    .RenderBodyAsync());
            });

            Handlebars.RegisterHelper("ClearAlternates", (output, context, arguments) =>
            {
                context.Model.Metadata.Alternates.Clear();
            });

            Handlebars.RegisterHelper("SetMetadataType", (output, context, arguments) =>
            {
                if (arguments.Any())
                {
                    context.Model.Metadata.Type = arguments[0].ToString();
                }
            });

            Handlebars.RegisterHelper("DisplayAsync", async (output, context, arguments) =>
            {
                output.WriteSafeString(
                    await new HelperContext(context)
                    .DisplayAsync(
                        arguments.Any() ? arguments[0] : (object)context));
            });

            Handlebars.RegisterHelper("UrlContent", (output, context, arguments) =>
            {
                if (arguments.Any())
                {
                    output.WriteSafeString(
                        new HelperContext(context)
                        .Url.Content(
                            arguments[0].ToString()));
                }
            });

            Handlebars.RegisterHelper("UrlAction", (output, context, arguments) =>
            {
                if (!arguments.Any())
                {
                    return;
                }

                var attributes = arguments[0] as IDictionary<string, object>;

                object action;
                if (attributes.TryGetValue("action", out action))
                {
                    attributes.Remove("action");
                }

                object controller;
                if (attributes.TryGetValue("controller", out controller))
                {
                    attributes.Remove("controller");
                }

                output.WriteSafeString(
                    new HelperContext(context)
                    .Url.Action(
                        action: action?.ToString(),
                        controller: controller?.ToString(),
                        values: attributes));
            });

            Handlebars.RegisterHelper("HtmlRaw", (output, context, arguments) =>
            {
                if (arguments.Any())
                {
                    output.WriteSafeString(arguments[0].ToString());
                }
            });

            Handlebars.RegisterHelper("HtmlActionLink", (output, context, arguments) =>
            {
                if (arguments.Count() < 2)
                {
                    return;
                }

                var attributes = arguments[1] as IDictionary<string, object>;

                object action;
                if (attributes.TryGetValue("action", out action))
                {
                    attributes.Remove("action");
                }

                object controller;
                if (attributes.TryGetValue("controller", out controller))
                {
                    attributes.Remove("controller");
                }

                object area;
                if (attributes.TryGetValue("area", out area))
                {
                    attributes.Remove("area");
                }

                var routesValues = new Dictionary<string, object>();

                if (area != null)
                {
                    routesValues.Add("area", area);
                }

                var content =
                    new HelperContext(context)
                    .Html.ActionLink(
                        linkText: arguments[0].ToString(),
                        actionName: action?.ToString(),
                        controllerName: controller?.ToString(),
                        routeValues: routesValues,
                        htmlAttributes: attributes);

                using (var writer = new StringWriter())
                {
                    content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                    output.WriteSafeString(writer.ToString());
                }
            });
        }

        internal class HelperContext
        {
            private dynamic _context;

            public HelperContext(dynamic context)
            {
                _context = context;
            }

            public DisplayContext DisplayContext
            {
                get
                {
                    return _context.DisplayContext;
                }
            }

            public ViewContext ViewContext
            {
                get
                {
                    return DisplayContext.ViewContext;
                }
            }

            public FormContext FormContext
            {
                get
                {
                    return ViewContext.FormContext;
                }
                set
                {
                    ViewContext.FormContext = value;
                }
            }

            public dynamic RazorPage
            {
                get
                {
                    return ((dynamic)ViewContext.View).RazorPage;
                }
            }

            public ViewLocalizer T
            {
                get
                {
                    return RazorPage.T;
                }
            }

            public IHtmlHelper Html
            {
                get
                {
                    return RazorPage.Html;
                }
            }

            public IUrlHelper Url
            {
                get
                {
                    return RazorPage.Url;
                }
            }

            public ClaimsPrincipal User
            {
                get
                {
                    return RazorPage.User;
                }
            }

            public IServiceProvider ServiceProvider
            {
                get
                {
                    return DisplayContext.ServiceProvider;
                }
            }

            public T GetService<T>()
            {
                return ServiceProvider.GetService<T>();
            }

            public async Task<IHtmlContent> DisplayAsync(object shape)
            {
                return await DisplayContext.DisplayAsync.ShapeExecuteAsync(shape);
            }

            public async Task<IHtmlContent> RenderBodyAsync()
            {
                return await DisplayAsync(RazorPage.ThemeLayout.Content);
            }

            public async Task<IHtmlContent> RenderSectionAsync(string name, bool required)
            {
                return await RazorPage.RenderSectionAsync(name, required);
            }

            public IHtmlContent RenderTitleSegments(IHtmlContent segment, string position = "0", IHtmlContent separator = null)
            {
                return RazorPage.RenderTitleSegments(segment, position, separator);
            }
        }
    }
}
