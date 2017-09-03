using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Fluid;
using Fluid.Accessors;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.Liquid.Filters;
using Orchard.DisplayManagement.Liquid.Internal;
using Orchard.DisplayManagement.Liquid.Tags;
using Orchard.DisplayManagement.Shapes;
using Orchard.DisplayManagement.Zones;
using Orchard.Liquid;
using Orchard.Settings;

namespace Orchard.DisplayManagement.Liquid
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
            Factory.RegisterTag<DisplayTag>("display");

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

            NamedHelperTag.RegisterDefaultArgument("shape", "type");
            NamedHelperBlock.RegisterDefaultArgument("zone", "name");

            TemplateContext.GlobalFilters.WithLiquidViewFilters();
        }

        internal static async Task RenderAsync(RazorPage<dynamic> page)
        {
            var services = page.Context.RequestServices;
            var path = Path.ChangeExtension(page.ViewContext.ExecutingFilePath, ViewExtension);
            var fileProviderAccessor = services.GetRequiredService<ILiquidViewFileProviderAccessor>();
            var template = Parse(path, fileProviderAccessor.FileProvider, Cache);

            var context = new TemplateContext();
            context.Contextualize(page, (object)page.Model);

            var liquidOptions = services.GetRequiredService<IOptions<LiquidOptions>>().Value;

            foreach (var registration in liquidOptions.FilterRegistrations)
            {
                context.Filters.AddAsyncFilter(registration.Key, (input, arguments, ctx) =>
                {
                    var type = registration.Value;
                    var filter = services.GetRequiredService(registration.Value) as ILiquidFilter;
                    return filter.ProcessAsync(input, arguments, ctx);
                });
            }

            page.WriteLiteral(await template.RenderAsync(context));
        }

        public static IFluidTemplate Parse(string path, IFileProvider fileProvider, IMemoryCache cache)
        {
            return cache.GetOrCreate(path, entry =>
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                var fileInfo = fileProvider.GetFileInfo(path);
                entry.ExpirationTokens.Add(fileProvider.Watch(path));

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
                            throw new Exception(String.Join(System.Environment.NewLine, errors));
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
            if ((o as Shape).Properties.TryGetValue(n, out object result))
            {
                return result;
            }

            foreach (var item in (o as Shape).Items)
            {
                if (item is IShape && item.Metadata.Type == n)
                {
                    return item;
                }
            }

            return null;
        };
    }

    public static class TemplateContextExtensions
    {
        public static void Contextualize(this TemplateContext context, RazorPage page, object model)
        {
            var services = page.Context.RequestServices;
            var displayHelper = services.GetRequiredService<IDisplayHelperFactory>().CreateHelper(page.ViewContext);

            context.Contextualize(new DisplayContext()
            {
                ServiceProvider = page.Context.RequestServices,
                DisplayAsync = displayHelper,
                ViewContext = page.ViewContext,
                Value = model
            });
        }

        public static async void Contextualize(this TemplateContext context, DisplayContext displayContext)
        {
            var services = displayContext.ServiceProvider;
            context.AmbientValues.Add("Services", services);

            var displayHelperFactory = services.GetRequiredService<IDisplayHelperFactory>();
            context.AmbientValues.Add("DisplayHelperFactory", displayHelperFactory);

            context.AmbientValues.Add("DisplayHelper", displayContext.DisplayAsync);
            context.AmbientValues.Add("ViewContext", displayContext.ViewContext);

            var urlHelperFactory = services.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory.GetUrlHelper(displayContext.ViewContext);
            context.AmbientValues.Add("UrlHelper", urlHelper);

            var shapeFactory = services.GetRequiredService<IShapeFactory>();
            context.AmbientValues.Add("ShapeFactory", shapeFactory);

            var localizer = services.GetRequiredService<IViewLocalizer>();
            if (localizer is IViewContextAware contextable)
            {
                contextable.Contextualize(displayContext.ViewContext);
            }

            context.AmbientValues.Add("ViewLocalizer", localizer);

            var layoutAccessor = services.GetRequiredService<ILayoutAccessor>();
            context.AmbientValues.Add("LayoutAccessor", layoutAccessor);

            var layout = layoutAccessor.GetLayout();
            context.AmbientValues.Add("ThemeLayout", layout);

            var site = await services.GetRequiredService<ISiteService>().GetSiteSettingsAsync();
            context.MemberAccessStrategy.Register(site.GetType());
            context.LocalScope.SetValue("Site", site);

            // TODO: Extract the request culture instead of the default site's one
            if (site.Culture != null)
            {
                context.CultureInfo = new CultureInfo(site.Culture);
            }

            var model = displayContext.Value;
            context.MemberAccessStrategy.Register(model.GetType());
            context.LocalScope.SetValue("Model", model);
        }
    }
}