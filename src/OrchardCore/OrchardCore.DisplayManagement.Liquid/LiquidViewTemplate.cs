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

            Factory.RegisterTag<HttpContextAddItemTag>("httpcontext_add_items");
            Factory.RegisterTag<HttpContextRemoveItemTag>("httpcontext_remove_items");

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
            Factory.RegisterBlock<NamedHelperBlock>("form");
            Factory.RegisterBlock<NamedHelperBlock>("scriptblock");
            Factory.RegisterBlock<NamedHelperBlock>("styleblock");

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

        /// <summary>
        /// Retrieve the current <see cref="LiquidTemplateContext"/> from the current shell scope.
        /// </summary>
        public static LiquidTemplateContext Context => ShellScope.GetOrCreateFeature(() => new LiquidTemplateContextInternal(ShellScope.Services));

        internal static async Task RenderAsync(RazorPage<dynamic> page)
        {
            var services = page.Context.RequestServices;

            var path = Path.ChangeExtension(page.ViewContext.ExecutingFilePath, ViewExtension);
            var fileProviderAccessor = services.GetRequiredService<ILiquidViewFileProviderAccessor>();
            var isDevelopment = services.GetRequiredService<IHostEnvironment>().IsDevelopment();

            var template = await ParseAsync(path, fileProviderAccessor.FileProvider, Cache, isDevelopment);

            var context = Context;
            var htmlEncoder = services.GetRequiredService<HtmlEncoder>();

            try
            {
                await context.EnterScopeAsync(page.ViewContext, (object)page.Model, scopeAction: null);
                await template.RenderAsync(page.Output, htmlEncoder, context);
            }
            finally
            {
                context.ReleaseScope();
            }
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
    }

    internal class ShapeAccessor : DelegateAccessor
    {
        public ShapeAccessor() : base(_getter)
        {
        }

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

                // Resolves Model.Content.MyType-MyField-FieldType_Display__DisplayMode
                var namedShaped = shape.Named(n);
                if (namedShaped != null)
                {
                    return namedShaped;
                }

                // Resolves Model.Content.MyNamedPart
                // Resolves Model.Content.MyType__MyField
                // Resolves Model.Content.MyType-MyField
                return shape.NormalizedNamed(n.Replace("__", "-"));
            }

            return null;
        };
    }

    public static class LiquidViewTemplateExtensions
    {
        public static async Task<string> RenderAsync(this LiquidViewTemplate template, TextEncoder encoder, LiquidTemplateContext context, object model, Action<Scope> scopeAction)
        {
            var viewContextAccessor = context.Services.GetRequiredService<ViewContextAccessor>();
            var viewContext = viewContextAccessor.ViewContext;

            if (viewContext == null)
            {
                viewContext = viewContextAccessor.ViewContext = await GetViewContextAsync(context);
            }

            try
            {
                await context.EnterScopeAsync(viewContext, model, scopeAction);
                return await template.RenderAsync(context, encoder);
            }
            finally
            {
                context.ReleaseScope();
            }
        }

        public static async Task RenderAsync(this LiquidViewTemplate template, TextWriter writer, TextEncoder encoder, LiquidTemplateContext context, object model, Action<Scope> scopeAction)
        {
            var viewContextAccessor = context.Services.GetRequiredService<ViewContextAccessor>();
            var viewContext = viewContextAccessor.ViewContext;

            if (viewContext == null)
            {
                viewContext = viewContextAccessor.ViewContext = await GetViewContextAsync(context);
            }

            try
            {
                await context.EnterScopeAsync(viewContext, model, scopeAction);
                await template.RenderAsync(writer, encoder, context);
            }
            finally
            {
                context.ReleaseScope();
            }
        }

        public static async Task<ViewContext> GetViewContextAsync(LiquidTemplateContext context)
        {
            var actionContext = context.Services.GetService<IActionContextAccessor>()?.ActionContext;

            if (actionContext == null)
            {
                var httpContext = context.Services.GetRequiredService<IHttpContextAccessor>().HttpContext;
                actionContext = await GetActionContextAsync(httpContext);
            }

            return GetViewContext(actionContext);
        }

        internal static async Task<ActionContext> GetActionContextAsync(HttpContext httpContext)
        {
            var routeData = new RouteData();
            routeData.Routers.Add(new RouteCollection());

            var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
            var filters = httpContext.RequestServices.GetServices<IAsyncViewActionFilter>();

            foreach (var filter in filters)
            {
                await filter.OnActionExecutionAsync(actionContext);
            }

            return actionContext;
        }

        internal static ViewContext GetViewContext(ActionContext actionContext)
        {
            var services = actionContext.HttpContext.RequestServices;

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
                TextWriter.Null,
                new HtmlHelperOptions());
        }
    }

    public static class LiquidTemplateContextExtensions
    {
        internal static async Task EnterScopeAsync(this LiquidTemplateContext context, ViewContext viewContext, object model, Action<Scope> scopeAction)
        {
            var contextInternal = context as LiquidTemplateContextInternal;

            if (!contextInternal.IsInitialized)
            {
                context.AmbientValues.EnsureCapacity(9);
                context.AmbientValues.Add("Services", context.Services);

                var displayHelper = context.Services.GetRequiredService<IDisplayHelper>();
                context.AmbientValues.Add("DisplayHelper", displayHelper);

                var urlHelperFactory = context.Services.GetRequiredService<IUrlHelperFactory>();
                var urlHelper = urlHelperFactory.GetUrlHelper(viewContext);
                context.AmbientValues.Add("UrlHelper", urlHelper);

                var shapeFactory = context.Services.GetRequiredService<IShapeFactory>();
                context.AmbientValues.Add("ShapeFactory", shapeFactory);

                var layoutAccessor = context.Services.GetRequiredService<ILayoutAccessor>();
                var layout = await layoutAccessor.GetLayoutAsync();
                context.AmbientValues.Add("ThemeLayout", layout);

                var options = context.Services.GetRequiredService<IOptions<LiquidOptions>>().Value;

                context.AddAsyncFilters(options);

                foreach (var handler in context.Services.GetServices<ILiquidTemplateEventHandler>())
                {
                    await handler.RenderingAsync(context);
                }

                context.CultureInfo = CultureInfo.CurrentUICulture;

                contextInternal.IsInitialized = true;
            }

            context.EnterChildScope();

            var viewLocalizer = context.Services.GetRequiredService<IViewLocalizer>();

            if (viewLocalizer is IViewContextAware contextable)
            {
                contextable.Contextualize(viewContext);
            }

            context.SetValue("ViewLocalizer", viewLocalizer);

            if (model != null)
            {
                context.MemberAccessStrategy.Register(model.GetType());
            }

            if (context.GetValue("Model")?.ToObjectValue() == model && model is IShape shape)
            {
                if (contextInternal.ShapeRecursions++ > LiquidTemplateContextInternal.MaxShapeRecursions)
                {
                    throw new InvalidOperationException(
                        $"The '{shape.Metadata.Type}' shape has been called recursively more than {LiquidTemplateContextInternal.MaxShapeRecursions} times.");
                }
            }
            else
            {
                contextInternal.ShapeRecursions = 0;
            }

            context.SetValue("Model", model);

            scopeAction?.Invoke(context.LocalScope);
        }

        internal static void AddAsyncFilters(this LiquidTemplateContext context, LiquidOptions options)
        {
            context.Filters.EnsureCapacity(options.FilterRegistrations.Count);

            foreach (var registration in options.FilterRegistrations)
            {
                context.Filters.AddAsyncFilter(registration.Key, (input, arguments, ctx) =>
                {
                    var filter = (ILiquidFilter)context.Services.GetRequiredService(registration.Value);
                    return filter.ProcessAsync(input, arguments, ctx);
                });
            }
        }
    }

    internal class LiquidTemplateContextInternal : LiquidTemplateContext
    {
        public const int MaxShapeRecursions = 3;

        public LiquidTemplateContextInternal(IServiceProvider services) : base(services) { }

        public bool IsInitialized { get; set; }

        public int ShapeRecursions { get; set; }
    }
}
