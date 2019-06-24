using System;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Accessors;
using Fluid.Values;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.DisplayManagement.Liquid.Internal;
using OrchardCore.DisplayManagement.Liquid.Tags;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.DynamicCache.Liquid;
using OrchardCore.Environment.Shell;
using OrchardCore.Liquid;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidViewTemplate : BaseFluidTemplate<LiquidViewTemplate>
    {
        public static readonly string ViewsFolder = "Views";
        public static readonly string ViewExtension = ".liquid";
        public static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        static LiquidViewTemplate()
        {
            FluidValue.TypeMappings.Add(typeof(Shape), o => new ObjectValue(o));
            FluidValue.TypeMappings.Add(typeof(ZoneHolding), o => new ObjectValue(o));

            TemplateContext.GlobalMemberAccessStrategy.Register<Shape>("*", new ShapeAccessor());
            TemplateContext.GlobalMemberAccessStrategy.Register<ZoneHolding>("*", new ShapeAccessor());

            Factory.RegisterTag<RenderBodyTag>("render_body");
            Factory.RegisterTag<RenderSectionTag>("render_section");
            Factory.RegisterTag<RenderTitleSegmentsTag>("page_title");
            Factory.RegisterTag<AntiForgeryTokenTag>("antiforgerytoken");
            Factory.RegisterTag<LayoutTag>("layout");

            Factory.RegisterTag<ClearAlternatesTag>("shape_clear_alternates");
            Factory.RegisterTag<AddAlternatesTag>("shape_add_alternates");
            Factory.RegisterTag<ClearWrappers>("shape_clear_wrappers");
            Factory.RegisterTag<AddWrappersTag>("shape_add_wrappers");
            Factory.RegisterTag<ClearClassesTag>("shape_clear_classes");
            Factory.RegisterTag<AddClassesTag>("shape_add_classes");
            Factory.RegisterTag<ClearAttributesTag>("shape_clear_attributes");
            Factory.RegisterTag<AddAttributesTag>("shape_add_attributes");
            Factory.RegisterTag<ShapeTypeTag>("shape_type");
            Factory.RegisterTag<ShapeDisplayTypeTag>("shape_display_type");
            Factory.RegisterTag<ShapePositionTag>("shape_position");
            Factory.RegisterTag<ShapeCacheTag>("shape_cache");
            Factory.RegisterTag<ShapeTabTag>("shape_tab");
            Factory.RegisterTag<ShapeRemoveItemTag>("shape_remove_item");
            Factory.RegisterTag<ShapePagerTag>("shape_pager");

            Factory.RegisterTag<HelperTag>("helper");
            Factory.RegisterTag<NamedHelperTag>("shape");
            Factory.RegisterTag<NamedHelperTag>("link");
            Factory.RegisterTag<NamedHelperTag>("meta");
            Factory.RegisterTag<NamedHelperTag>("resources");
            Factory.RegisterTag<NamedHelperTag>("script");
            Factory.RegisterTag<NamedHelperTag>("style");

            Factory.RegisterBlock<HelperBlock>("block");
            Factory.RegisterBlock<NamedHelperBlock>("a");
            Factory.RegisterBlock<NamedHelperBlock>("zone");
            Factory.RegisterBlock<NamedHelperBlock>("scriptblock");

            // Dynamic caching
            Factory.RegisterBlock<CacheBlock>("cache");
            Factory.RegisterTag<CacheDependencyTag>("cache_dependency");
            Factory.RegisterTag<CacheExpiresOnTag>("cache_expires_on");
            Factory.RegisterTag<CacheExpiresAfterTag>("cache_expires_after");
            Factory.RegisterTag<CacheExpiresSlidingTag>("cache_expires_sliding");

            NamedHelperTag.RegisterDefaultArgument("shape", "type");
            NamedHelperBlock.RegisterDefaultArgument("zone", "name");

            TemplateContext.GlobalFilters.WithLiquidViewFilters();
        }

        internal static async Task RenderAsync(RazorPage<dynamic> page)
        {
            var services = page.Context.RequestServices;
            var path = Path.ChangeExtension(page.ViewContext.ExecutingFilePath, ViewExtension);
            var fileProviderAccessor = services.GetRequiredService<ILiquidViewFileProviderAccessor>();
            var isDevelopment = services.GetRequiredService<IHostingEnvironment>().IsDevelopment();

            var template = Parse(path, fileProviderAccessor.FileProvider, Cache, isDevelopment);

            var context = new TemplateContext();
            await context.ContextualizeAsync(page, (object)page.Model);

            var options = services.GetRequiredService<IOptions<LiquidOptions>>().Value;
            await template.RenderAsync(options, services, page.Output, HtmlEncoder.Default, context);
        }

        public static LiquidViewTemplate Parse(string path, IFileProvider fileProvider, IMemoryCache cache, bool isDevelopment)
        {
            return cache.GetOrCreate(path, entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromHours(1));
                var fileInfo = fileProvider.GetFileInfo(path);

                if (isDevelopment)
                {
                    entry.ExpirationTokens.Add(fileProvider.Watch(path));
                }

                using (var stream = fileInfo.CreateReadStream())
                {
                    using (var sr = new StreamReader(stream))
                    {
                        if (TryParse(sr.ReadToEnd(), out var template, out var errors))
                        {
                            return template;
                        }
                        else
                        {
                            throw new Exception($"Failed to parse liquid file {path}: {String.Join(System.Environment.NewLine, errors)}");
                        }
                    }
                }
            });
        }
    }

    internal class ShapeAccessor : DelegateAccessor
    {
        public ShapeAccessor() : base(_getter) { }

        private static Func<object, string, object> _getter => (o, n) =>
        {
            if (o is Shape shape)
            {
                if (shape.Properties.TryGetValue(n, out object result))
                {
                    return result;
                }

                foreach (var item in shape.Items)
                {
                    // Resolve Model.Content.MyNamedPart
                    if (item is IShape itemShape && itemShape.Metadata.Name == n)
                    {
                        return item;
                    }
                }
            }

            return null;
        };
    }

    public static class LiquidViewTemplateExtensions
    {
        public static Task RenderAsync(this LiquidViewTemplate template, LiquidOptions options,
            IServiceProvider services, TextWriter writer, TextEncoder encoder, TemplateContext templateContext)
        {
            foreach (var registration in options.FilterRegistrations)
            {
                templateContext.Filters.AddAsyncFilter(registration.Key, (input, arguments, ctx) =>
                {
                    var type = registration.Value;
                    var filter = services.GetRequiredService(registration.Value) as ILiquidFilter;
                    return filter.ProcessAsync(input, arguments, ctx);
                });
            }

            // Check if a 'ViewContext' has been cached for rendering.
            if (templateContext.AmbientValues.TryGetValue("ViewContext", out var context) &&
                context is ViewContext viewContext &&
                viewContext.View is RazorView razorView &&
                razorView.RazorPage is LiquidPage liquidPage)
            {
                liquidPage.RenderAsync = output =>
                {
                    // Render the template through the default liquid page.
                    return template.RenderAsync(output, encoder, templateContext);
                };

                viewContext.Writer = writer;

                // Use the view engine to render the liquid page.
                return viewContext.View.RenderAsync(viewContext);
            }

            // Otherwise, we don't need the view engine for rendering.
            return template.RenderAsync(writer, encoder, templateContext);
        }
    }

    public static class TemplateContextExtensions
    {
        public static async Task ContextualizeAsync(this TemplateContext context, IServiceProvider services)
        {
            if (!context.AmbientValues.ContainsKey("Services"))
            {
                var displayHelper = services.GetRequiredService<IDisplayHelper>();

                await context.ContextualizeAsync(new DisplayContext()
                {
                    ServiceProvider = services,
                    DisplayAsync = displayHelper,
                    Value = null
                });
            }
        }

        public static Task ContextualizeAsync(this TemplateContext context, RazorPage page, object model)
        {
            var services = page.Context.RequestServices;
            var displayHelper = services.GetRequiredService<IDisplayHelper>();

            return context.ContextualizeAsync(new DisplayContext()
            {
                ServiceProvider = page.Context.RequestServices,
                DisplayAsync = displayHelper,
                Value = model
            });
        }

        public static async Task ContextualizeAsync(this TemplateContext context, DisplayContext displayContext)
        {
            var services = displayContext.ServiceProvider;
            context.AmbientValues.Add("Services", services);

            context.AmbientValues.Add("DisplayHelper", displayContext.DisplayAsync);

            var viewContextAccessor = services.GetRequiredService<ViewContextAccessor>();
            var viewContext = viewContextAccessor.ViewContext;

            if (viewContext == null)
            {
                var actionContext = GetActionContext(services);
                viewContext = GetViewContext(services, actionContext);

                // If there was no 'ViewContext' but a 'DisplayContext'.
                if (displayContext.Value != null)
                {
                    // Cache the 'ViewContext' to be used for rendering.
                    context.AmbientValues.Add("ViewContext", viewContext);
                }
            }

            var urlHelperFactory = services.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory.GetUrlHelper(viewContext);
            context.AmbientValues.Add("UrlHelper", urlHelper);

            var shapeFactory = services.GetRequiredService<IShapeFactory>();
            context.AmbientValues.Add("ShapeFactory", shapeFactory);

            var localizer = services.GetRequiredService<IViewLocalizer>();
            if (localizer is IViewContextAware contextable)
            {
                contextable.Contextualize(viewContext);
            }

            context.AmbientValues.Add("ViewLocalizer", localizer);

            var layoutAccessor = services.GetRequiredService<ILayoutAccessor>();
            context.AmbientValues.Add("LayoutAccessor", layoutAccessor);

            var layout = await layoutAccessor.GetLayoutAsync();
            context.AmbientValues.Add("ThemeLayout", layout);

            if (viewContext.View is RazorView razorView)
            {
                context.AmbientValues.Add("LiquidPage", razorView.RazorPage);
            }

            foreach (var handler in services.GetServices<ILiquidTemplateEventHandler>())
            {
                await handler.RenderingAsync(context);
            }

            var model = displayContext.Value;
            if (model != null)
            {
                context.MemberAccessStrategy.Register(model.GetType());
                context.LocalScope.SetValue("Model", model);
            }

            context.CultureInfo = CultureInfo.CurrentUICulture;
        }

        private static ActionContext GetActionContext(IServiceProvider services)
        {
            var actionContext = services.GetService<IActionContextAccessor>()?.ActionContext;

            if (actionContext != null)
            {
                return actionContext;
            }

            var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.HttpContext;
            var shellContext = httpContext.Features.Get<ShellContextFeature>()?.ShellContext;

            var routeData = new RouteData();
            var pipeline = shellContext?.Pipeline as ShellRequestPipeline;
            routeData.Routers.Add(pipeline?.Router ?? new RouteCollection());

            return new ActionContext(httpContext, routeData, new ActionDescriptor());
        }

        private static ViewContext GetViewContext(IServiceProvider services, ActionContext actionContext)
        {
            var options = services.GetService<IOptions<MvcViewOptions>>();
            var viewEngine = options.Value.ViewEngines[0];

            var viewResult = viewEngine.GetView(executingFilePath: null,
                LiquidViewsFeatureProvider.DefaultRazorViewPath, isMainPage: true);

            var tempDataProvider = services.GetService<ITempDataProvider>();

            return new ViewContext(
                actionContext,
                viewResult.View,
                new ViewDataDictionary(
                    metadataProvider: new EmptyModelMetadataProvider(),
                    modelState: new ModelStateDictionary()),
                new TempDataDictionary(
                    actionContext.HttpContext,
                    tempDataProvider),
                StringWriter.Null,
                new HtmlHelperOptions());
        }
    }
}
