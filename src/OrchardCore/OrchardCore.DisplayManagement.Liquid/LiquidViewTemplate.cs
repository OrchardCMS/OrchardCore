using System;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Accessors;
using Fluid.Values;
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.DisplayManagement.Liquid.Internal;
using OrchardCore.DisplayManagement.Liquid.Tags;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.DynamicCache.Liquid;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidViewTemplate : BaseFluidTemplate<LiquidViewTemplate>
    {
        private const string ContextKey = "LiquidViewTemplateContext";

        public static readonly string ViewsFolder = "Views";
        public static readonly string ViewExtension = ".liquid";
        public static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        static LiquidViewTemplate()
        {
            FluidValue.SetTypeMapping<Shape>(o => new ObjectValue(o));
            FluidValue.SetTypeMapping<ZoneHolding>(o => new ObjectValue(o));

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
            Factory.RegisterTag<ShapeAddPropertyTag>("shape_add_properties");
            Factory.RegisterTag<ShapeRemovePropertyTag>("shape_remove_property");
            Factory.RegisterTag<ShapePagerTag>("shape_pager");

            Factory.RegisterTag<HelperTag>("helper");
            Factory.RegisterTag<NamedHelperTag>("shape");
            Factory.RegisterTag<NamedHelperTag>("contentitem");
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
            var isDevelopment = services.GetRequiredService<IHostEnvironment>().IsDevelopment();

            var template = await ParseAsync(path, fileProviderAccessor.FileProvider, Cache, isDevelopment);

            var context = Context;
            var htmlEncoder = services.GetRequiredService<HtmlEncoder>();

            await context.ContextualizeAsync(services, page.ViewContext, (object)page.Model);
            await template.RenderAsync(page.Output, htmlEncoder, context);
        }

        public static Task<LiquidViewTemplate> ParseAsync(string path, IFileProvider fileProvider, IMemoryCache cache, bool isDevelopment)
        {
            return cache.GetOrCreateAsync(path, async entry =>
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
                        if (TryParse(await sr.ReadToEndAsync(), out var template, out var errors))
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

        /// <summary>
        /// Retrieve the current <see cref="TemplateContext"/> from the current shell scope.
        /// </summary>
        public static TemplateContext Context => ShellScope.GetOrCreate<TemplateContext>(ContextKey);
    }

    internal class ShapeAccessor : DelegateAccessor
    {
        public ShapeAccessor() : base(_getter) { }

        private static Func<object, string, object> _getter => (o, n) =>
        {
            if (o is Shape shape)
            {
                if (shape.Properties.TryGetValue(n, out var result))
                {
                    return result;
                }

                if (n == "Items")
                {
                    return shape.Items;
                }

                // Resolve Model.Content.MyNamedPart
                // Resolve Model.Content.MyType__MyField OR Resolve Model.Content.MyType-MyField
                return shape.Named(n.Replace("__", "-"));
            }

            return null;
        };
    }

    public static class LiquidViewTemplateExtensions
    {
        public static async Task<string> RenderAsync(this LiquidViewTemplate template, IServiceProvider services, TextEncoder encoder, TemplateContext context, object model)
        {
            var viewContextAccessor = services.GetRequiredService<ViewContextAccessor>();
            var viewContext = viewContextAccessor.ViewContext;

            if (viewContext == null)
            {
                var actionContext = services.GetService<IActionContextAccessor>()?.ActionContext;

                if (actionContext == null)
                {
                    actionContext = await GetActionContextAsync(services);
                    viewContext = GetViewContext(services, actionContext);

                    // If the model is a shape using a dynamic binding.
                    if (model is Shape shape && shape.Metadata.IsDynamic)
                    {
                        // We need to use the view engine to render the liquid page.
                        await context.ContextualizeAsync(services, viewContext, model);
                        return await template.RenderAsync(viewContext, encoder, context);
                    }
                }
                else
                {
                    viewContext = GetViewContext(services, actionContext);
                }
            }

            await context.ContextualizeAsync(services, viewContext, model);
            return await template.RenderAsync(context, encoder);
        }

        internal static async Task<string> RenderAsync(this LiquidViewTemplate template, ViewContext viewContext, TextEncoder encoder, TemplateContext context)
        {
            (((RazorView)viewContext.View).RazorPage as LiquidPage).RenderAsync = output => template.RenderAsync(output, encoder, context);

            using (var sb = StringBuilderPool.GetInstance())
            {
                using (var writer = new StringWriter(sb.Builder))
                {
                    viewContext.Writer = writer;

                    // Use the view engine to render the liquid page.
                    await viewContext.View.RenderAsync(viewContext);
                    await writer.FlushAsync();
                }

                return sb.Builder.ToString();
            }
        }

        internal async static Task<ActionContext> GetActionContextAsync(IServiceProvider services)
        {
            var routeData = new RouteData();
            routeData.Routers.Add(new RouteCollection());

            var httpContext = services.GetRequiredService<IHttpContextAccessor>().HttpContext;

            var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
            var filters = httpContext.RequestServices.GetServices<IAsyncViewActionFilter>();

            foreach (var filter in filters)
            {
                await filter.OnActionExecutionAsync(actionContext);
            }

            return actionContext;
        }

        internal static ViewContext GetViewContext(IServiceProvider services, ActionContext actionContext)
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

    public static class TemplateContextExtensions
    {
        internal static async Task ContextualizeAsync(this TemplateContext context, IServiceProvider services, ViewContext viewContext, object model)
        {
            // Check if already contextualized.
            if (!context.AmbientValues.ContainsKey("Services"))
            {
                // Shared contextualization.
                context.AmbientValues.EnsureCapacity(9);
                context.AmbientValues.Add("Services", services);

                var displayHelper = services.GetRequiredService<IDisplayHelper>();
                context.AmbientValues.Add("DisplayHelper", displayHelper);

                var urlHelperFactory = services.GetRequiredService<IUrlHelperFactory>();
                var urlHelper = urlHelperFactory.GetUrlHelper(viewContext);
                context.AmbientValues.Add("UrlHelper", urlHelper);

                var shapeFactory = services.GetRequiredService<IShapeFactory>();
                context.AmbientValues.Add("ShapeFactory", shapeFactory);

                var viewLocalizer = services.GetRequiredService<IViewLocalizer>();
                context.AmbientValues.Add("ViewLocalizer", viewLocalizer);

                var layoutAccessor = services.GetRequiredService<ILayoutAccessor>();
                context.AmbientValues.Add("LayoutAccessor", layoutAccessor);

                var layout = await layoutAccessor.GetLayoutAsync();
                context.AmbientValues.Add("ThemeLayout", layout);

                var options = services.GetRequiredService<IOptions<LiquidOptions>>().Value;

                foreach (var handler in services.GetServices<ILiquidTemplateEventHandler>())
                {
                    await handler.RenderingAsync(context);
                }

                context.AddAsyncFilters(options, services);

                context.CultureInfo = CultureInfo.CurrentUICulture;
            }

            // Specific contextualization before each rendering.
            var localizer = context.AmbientValues["ViewLocalizer"];

            if (localizer is IViewContextAware contextable)
            {
                contextable.Contextualize(viewContext);
            }

            if (viewContext.View is RazorView razorView)
            {
                context.AmbientValues["LiquidPage"] = razorView.RazorPage;
            }

            if (model != null)
            {
                context.MemberAccessStrategy.Register(model.GetType());
            }

            context.SetValue("Model", model);
        }

        internal static void AddAsyncFilters(this TemplateContext templateContext, LiquidOptions options, IServiceProvider services)
        {
            templateContext.Filters.EnsureCapacity(options.FilterRegistrations.Count);

            foreach (var registration in options.FilterRegistrations)
            {
                templateContext.Filters.AddAsyncFilter(registration.Key, (input, arguments, ctx) =>
                {
                    var filter = (ILiquidFilter)services.GetRequiredService(registration.Value);
                    return filter.ProcessAsync(input, arguments, ctx);
                });
            }
        }
    }
}
