using System;
using System.IO;
using System.Threading.Tasks;
using Fluid;
using Fluid.Accessors;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Fluid.Internal;
using Orchard.DisplayManagement.Shapes;
using Orchard.Liquid;
using Orchard.Settings;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidViewTemplate : FluidTemplate<ActivatorFluidParserFactory<FluidViewParser>>
    {
        public static readonly string ViewsFolder = "Views";
        public static readonly string ViewExtension = ".liquid";
        public static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        internal static async Task RenderAsync(FluidPage page)
        {
            var path = Path.ChangeExtension(page.ViewContext.ExecutingFilePath, ViewExtension);
            var fileProviderAccessor = page.GetService<IFluidViewFileProviderAccessor>();
            var template = Parse(path, fileProviderAccessor.FileProvider, Cache);

            var context = new TemplateContext();
            context.AmbientValues.Add("FluidPage", page);

            var urlHelperFactory = page.GetService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory.GetUrlHelper(page.ViewContext);
            context.AmbientValues.Add("UrlHelper", urlHelper);

            var displayHelperFactory = page.GetService<IDisplayHelperFactory>();
            var displayHelper = displayHelperFactory.CreateHelper(page.ViewContext);
            context.AmbientValues.Add("DisplayHelper", displayHelper);

            var shapeFactory = page.GetService<IShapeFactory>();
            context.AmbientValues.Add("ShapeFactory", shapeFactory);

            var localizer = page.GetService<IViewLocalizer>();
            var contextable = localizer as IViewContextAware;

            if (contextable != null)
            {
                contextable.Contextualize(page.ViewContext);
                context.AmbientValues.Add("ViewLocalizer", localizer);
            }

            var site = await page.GetService<ISiteService>().GetSiteSettingsAsync();
            context.MemberAccessStrategy.Register(site.GetType());
            context.LocalScope.SetValue("Site", site);

            context.RegisterObject((object)page.Model);
            context.LocalScope.SetValue("Model", page.Model);

            if (page.Model is Shape)
            {
                if (page.Model.Properties.Count > 0)
                {
                    foreach (var prop in page.Model.Properties)
                    {
                        context.RegisterObject((object)prop.Value);
                    }
                }
            }

            if (page.Model.Field != null)
            {
                context.RegisterObject(((object)page.Model.Field));
            }

            var liquidOptions = page.GetService<IOptions<LiquidOptions>>().Value;

            foreach (var registration in liquidOptions.FilterRegistrations)
            {
                context.Filters.AddAsyncFilter(registration.Key, (input, arguments, ctx) =>
                {
                    var type = registration.Value;
                    var filter = page.GetService(registration.Value) as ILiquidFilter;
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

        public static FluidPage EnsureFluidPage(TemplateContext context, string action)
        {
            if (!context.AmbientValues.TryGetValue("FluidPage", out var page))
            {
                throw new ParseException("FluidPage missing while invoking: " + action);
            }

            return (FluidPage)page;
        }
    }

    public static class TemplateContextExtensions
    {
        public static void RegisterObject(this TemplateContext context, object obj)
        {
            var type = obj.GetType();

            if (obj is IShape && !FluidValue.TypeMappings.TryGetValue(type, out var value))
            {
                FluidValue.TypeMappings.Add(type, o => new ObjectValue(o));

                if (obj is Shape)
                {
                    TemplateContext.GlobalMemberAccessStrategy.Register(type, "*", new DelegateAccessor((o, n) =>
                    {
                        if ((o as Shape).Properties.TryGetValue(n, out object result))
                        {
                            return result;
                        }

                        return null;
                    }));
                }
            }

            context.MemberAccessStrategy.Register(type);
        }
    }
}