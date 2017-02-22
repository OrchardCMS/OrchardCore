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
                foreach (var shape in (IEnumerable<dynamic>)arguments[0])
                {
                    if (shape is Shape)
                    {
                        shape.DisplayContext = context.DisplayContext;
                    }

                    options.Template(output, shape);
                }
            });

            Handlebars.RegisterHelper("T", (output, context, arguments) =>
            {
            output.WriteSafeString(
                (new HandleBarsContext(context)
                .T[arguments[0].ToString()])
                .Value);
            });

            Handlebars.RegisterHelper("SiteName", async (output, context, arguments) =>
            {
                output.Write(
                    (await new HandleBarsContext(context)
                    .GetService<ISiteService>()
                    .GetSiteSettingsAsync())
                    .SiteName);
            });

            Handlebars.RegisterHelper("IsAuthenticated", (output, context, arguments) =>
            {
                output.Write(
                    new HandleBarsContext(context)
                    .User.Identity.IsAuthenticated
                    ? "yes" : String.Empty);
            });

            Handlebars.RegisterHelper("UserName", (output, context, arguments) =>
            {
                output.Write(
                    new HandleBarsContext(context)
                    .User.Identity.Name);
            });

            Handlebars.RegisterHelper("menu", (output, context, arguments) =>
            {
                var handleBarsContext = new HandleBarsContext(context);

                var attributes = new TagHelperAttributeList(
                    (arguments[0] as IDictionary<string, object>)
                    .Select(x => new TagHelperAttribute(x.Key, x.Value))
                    .ToList());

                var tagHelperContext = new TagHelperContext(attributes,
                    new Dictionary<object, object>(),
                    Guid.NewGuid().ToString("N"));

                var tagHelperOutput = new TagHelperOutput("menu", attributes,
                    getChildContentAsync: (useCachedResult, encoder) =>
                        Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

                var shapeTagHelper = new ShapeTagHelper(
                    handleBarsContext.GetService<IShapeFactory>(),
                    handleBarsContext.GetService<IDisplayHelperFactory>())
                    {
                        ViewContext = handleBarsContext.ViewContext
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
                var handleBarsContext = new HandleBarsContext(context);

                var attributes = new TagHelperAttributeList(
                    (arguments[0] as IDictionary<string, object>)
                    .Select(x => new TagHelperAttribute(x.Key, x.Value))
                    .ToList());

                var formTagHelper = new Microsoft.AspNetCore.Mvc.TagHelpers
                    .FormTagHelper(handleBarsContext.GetService<IHtmlGenerator>())
                {
                    ViewContext = handleBarsContext.ViewContext
                };

                TagHelperAttribute attribute;
                if (attributes.TryGetAttribute("asp-antiforgery", out attribute))
                {
                    formTagHelper.Antiforgery = Convert.ToBoolean(attribute.Value);
                    attributes.Remove(attribute);
                }
                if (attributes.TryGetAttribute("asp-route-area", out attribute))
                {
                    formTagHelper.RouteValues.Add("area", attribute.Value.ToString());
                    attributes.Remove(attribute);
                }
                if (attributes.TryGetAttribute("asp-controller", out attribute))
                {
                    formTagHelper.Controller = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-action", out attribute))
                {
                    formTagHelper.Action = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                var tagHelperContext = new TagHelperContext(attributes,
                    new Dictionary<object, object>(),
                    Guid.NewGuid().ToString("N"));

                var tagHelperOutput = new TagHelperOutput("form", attributes,
                    getChildContentAsync: (useCachedResult, encoder) =>
                        Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

                formTagHelper.Process(tagHelperContext, tagHelperOutput);

                using (var writer = new StringWriter())
                {
                    tagHelperOutput.TagMode = TagMode.StartTagOnly;
                    tagHelperOutput.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                    output.WriteSafeString(writer.ToString());
                }

                if (!tagHelperOutput.PostContent.IsEmptyOrWhiteSpace)
                {
                    handleBarsContext.FormContext.EndOfFormContent.Add(tagHelperOutput.PostContent);
                }
            });

            Handlebars.RegisterHelper("/form", (output, context, arguments) =>
            {
                var handleBarsContext = new HandleBarsContext(context);

                if (handleBarsContext.FormContext.HasEndOfFormContent)
                {
                    foreach (var content in handleBarsContext.FormContext.EndOfFormContent)
                    {
                        using (var writer = new StringWriter())
                        {
                            content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                            output.WriteSafeString(writer.ToString());
                        }
                    }
                }

                output.WriteSafeString("</form>");
                handleBarsContext.FormContext = new FormContext();
            });

            Handlebars.RegisterHelper("a", (output, context, arguments) =>
            {
                var handleBarsContext = new HandleBarsContext(context);

                var attributes = new TagHelperAttributeList(
                    (arguments[0] as IDictionary<string, object>)
                    .Select(x => new TagHelperAttribute(x.Key, x.Value))
                    .ToList());

                var anchorTagHelper = new Microsoft.AspNetCore.Mvc.TagHelpers
                    .AnchorTagHelper(handleBarsContext.GetService<IHtmlGenerator>())
                    {
                        ViewContext = handleBarsContext.ViewContext
                    };

                TagHelperAttribute attribute;
                if (attributes.TryGetAttribute("asp-route-area", out attribute))
                {
                    anchorTagHelper.RouteValues.Add("area", attribute.Value.ToString());
                    attributes.Remove(attribute);
                }
                if (attributes.TryGetAttribute("asp-controller", out attribute))
                {
                    anchorTagHelper.Controller = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("asp-action", out attribute))
                {
                    anchorTagHelper.Action = attribute.Value.ToString();
                    attributes.Remove(attribute);
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
                var handleBarsContext = new HandleBarsContext(context);

                var attributes = new TagHelperAttributeList(
                    (arguments[0] as IDictionary<string, object>)
                    .Select(x => new TagHelperAttribute(x.Key, x.Value))
                    .ToList());

                var scriptTagHelper = new ScriptTagHelper(handleBarsContext.GetService<IResourceManager>());

                TagHelperAttribute attribute;
                if (attributes.TryGetAttribute("asp-name", out attribute))
                {
                    scriptTagHelper.Name = attribute.Value.ToString();
                    attributes.Remove(attribute);
                }

                if (attributes.TryGetAttribute("use-cdn", out attribute))
                {
                    scriptTagHelper.UseCdn = Convert.ToBoolean(attribute.Value);
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

                var tagHelperContext = new TagHelperContext(attributes,
                    new Dictionary<object, object>(),
                    Guid.NewGuid().ToString("N"));

                var tagHelperOutput = new TagHelperOutput("script", attributes,
                    getChildContentAsync: (useCachedResult, encoder) =>
                        Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

                scriptTagHelper.Process(tagHelperContext, tagHelperOutput);
            });

            Handlebars.RegisterHelper("style", (output, context, arguments) =>
            {
                var handleBarsContext = new HandleBarsContext(context);

                var attributes = new TagHelperAttributeList(
                    (arguments[0] as IDictionary<string, object>)
                    .Select(x => new TagHelperAttribute(x.Key, x.Value))
                    .ToList());

                var tagHelperContext = new TagHelperContext(attributes,
                    new Dictionary<object, object>(),
                    Guid.NewGuid().ToString("N"));

                var tagHelperOutput = new TagHelperOutput("script", attributes,
                    getChildContentAsync: (useCachedResult, encoder) =>
                        Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

                var styleTagHelper = new StyletTagHelper(handleBarsContext.GetService<IResourceManager>());

                TagHelperAttribute attribute;
                if (attributes.TryGetAttribute("asp-name", out attribute))
                {
                    styleTagHelper.Name = attribute.Value.ToString();
                }

                if (attributes.TryGetAttribute("use-cdn", out attribute))
                {
                    styleTagHelper.UseCdn = Convert.ToBoolean(attribute.Value);
                }

                if (attributes.TryGetAttribute("version", out attribute))
                {
                    styleTagHelper.Version = attribute.Value.ToString();
                }

                if (attributes.TryGetAttribute("at", out attribute))
                {
                    styleTagHelper.At = (ResourceLocation)Enum.Parse(typeof(ResourceLocation), attribute.Value.ToString());
                }

                styleTagHelper.Process(tagHelperContext, tagHelperOutput);
            });

            Handlebars.RegisterHelper("resources", (output, context, arguments) =>
            {
                var handleBarsContext = new HandleBarsContext(context);

                var attributes = new TagHelperAttributeList(
                    (arguments[0] as IDictionary<string, object>)
                    .Select(x => new TagHelperAttribute(x.Key, x.Value))
                    .ToList());

                var tagHelperContext = new TagHelperContext(attributes,
                    new Dictionary<object, object>(),
                    Guid.NewGuid().ToString("N"));

                var tagHelperOutput = new TagHelperOutput("script", attributes,
                    getChildContentAsync: (useCachedResult, encoder) =>
                        Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

                var resourcesTagHelper = new ResourcesTagHelper(
                    handleBarsContext.GetService<IResourceManager>(),
                    handleBarsContext.GetService<IRequireSettingsProvider>());

                TagHelperAttribute attribute;
                if (attributes.TryGetAttribute("type", out attribute))
                {
                    resourcesTagHelper.Type = (ResourceType)Enum.Parse(typeof(ResourceType), attribute.Value.ToString());
                }

                resourcesTagHelper.Process(tagHelperContext, tagHelperOutput);

                using (var writer = new StringWriter())
                {
                    tagHelperOutput.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                    output.WriteSafeString(writer.ToString());
                }
            });

            Handlebars.RegisterHelper("RenderTitleSegments", (output, context, arguments) =>
            {
                new HandleBarsContext(context)
                    .RenderTitleSegments(
                        new HtmlString(arguments[0].ToString()),
                        arguments.Count() > 1 ? arguments[1].ToString() : "0",
                        arguments.Count() > 2 ? new HtmlString(arguments[2].ToString()) : null)
                    .WriteTo(output, System.Text.Encodings.Web.HtmlEncoder.Default);
            });

            Handlebars.RegisterHelper("RenderSectionAsync", async (output, context, arguments) =>
            {
                output.WriteSafeString(
                    await new HandleBarsContext(context)
                    .RenderSectionAsync(
                        arguments[0].ToString(),
                        Convert.ToBoolean(arguments[1])));
            });

            Handlebars.RegisterHelper("RenderBodyAsync", async (output, context, arguments) =>
            {
                output.WriteSafeString(
                    await new HandleBarsContext(context)
                    .RenderBodyAsync());
            });

            Handlebars.RegisterHelper("ClearAlternates", (output, context, arguments) =>
            {
                context.Model.Metadata.Alternates.Clear();
            });

            Handlebars.RegisterHelper("SetMetadataType", (output, context, arguments) =>
            {
                context.Model.Metadata.Type = arguments[0].ToString();
            });

            Handlebars.RegisterHelper("DisplayAsync", async (output, context, arguments) =>
            {
                output.WriteSafeString(
                    await new HandleBarsContext(context).DisplayAsync(
                        arguments.Count() > 0 ? arguments[0] : (object)context));
            });

            Handlebars.RegisterHelper("UrlContent", (output, context, arguments) =>
            {
                output.WriteSafeString(
                    new HandleBarsContext(context).Url.Content(
                        arguments[0].ToString()));
            });

            Handlebars.RegisterHelper("UrlAction", (output, context, arguments) =>
            {
                output.WriteSafeString(
                    new HandleBarsContext(context).Url.Action(
                        action: arguments[0].ToString(),
                        controller: arguments[1].ToString(),
                        values: arguments.Count() > 2 ? arguments[2] : new { }));
            });

            Handlebars.RegisterHelper("HtmlRaw", (output, context, arguments) =>
            {
                output.WriteSafeString(arguments[0].ToString());
            });

            Handlebars.RegisterHelper("HtmlActionLink", (output, context, arguments) =>
            {
                var content =
                    new HandleBarsContext(context).Html.ActionLink(
                        linkText: arguments[0].ToString(),
                        actionName: arguments[1].ToString(),
                        routeValues: new { },
                        htmlAttributes: arguments.Count() > 2 ? arguments[2] : new { });

                using (var writer = new StringWriter())
                {
                    content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                    output.WriteSafeString(writer.ToString());
                }
            });
        }

        internal class HandleBarsContext
        {
            private dynamic _context;

            public HandleBarsContext(dynamic context)
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
