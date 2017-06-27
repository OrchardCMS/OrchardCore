using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Orchard.DisplayManagement.Fluid.Filters;
using Orchard.DisplayManagement.Fluid.ModelBinding;
using Orchard.Settings;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidViewTemplate : FluidTemplate<ActivatorFluidParserFactory<FluidViewParser>>
    {
        public static readonly string ViewExtension = ".fluid";
        private static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        static FluidViewTemplate()
        {
            TemplateContext.GlobalFilters.WithFluidViewFilters();
            TemplateContext.GlobalMemberAccessStrategy.Register(typeof(ViewContext));
            TemplateContext.GlobalMemberAccessStrategy.Register<ModelStateNode>();
        }

        internal static async Task RenderAsync(FluidPage page)
        {
            var serviceProvider = page.Context.RequestServices;
            var path = page.ViewContext.ExecutingFilePath.Replace(RazorViewEngine.ViewExtension, ViewExtension);

            // Todo: use custom FileProviders
            var environment = serviceProvider.GetRequiredService<IHostingEnvironment>();
            var template = Parse(path, environment.ContentRootFileProvider);

            var context = new TemplateContext();
            context.AmbientValues.Add("FluidPage", page);

            var site = await serviceProvider.GetService<ISiteService>().GetSiteSettingsAsync();
            context.MemberAccessStrategy.Register(site.GetType());
            context.LocalScope.SetValue("Site", site);

            var urlHelperFactory = serviceProvider.GetService<IUrlHelperFactory>();
            context.AmbientValues.Add("UrlHelper", urlHelperFactory.GetUrlHelper(page.ViewContext));

            context.MemberAccessStrategy.Register(page.Context.GetType());
            context.MemberAccessStrategy.Register(page.Context.Request.GetType());
            context.LocalScope.SetValue("Context", page.Context);

            context.LocalScope.SetValue("ViewData", page.ViewData);
            context.LocalScope.SetValue("ViewContext", page.ViewContext);

            var modelState = page.ViewContext.ModelState.ToDictionary(
                kv => kv.Key, kv => (object)new ModelStateNode(kv.Value));

            modelState["IsValid"] = page.ViewContext.ModelState.IsValid;
            context.LocalScope.SetValue("ModelState", modelState);

            if (page.Model != null)
            {
                context.LocalScope.SetValue("Model", page.Model);
                context.MemberAccessStrategy.Register(((object)page.Model).GetType());
            }

            page.WriteLiteral(await template.RenderAsync(context));
        }

        internal static IFluidTemplate Parse(string path, IFileProvider fileProvider)
        {
            return Cache.GetOrCreate(path, viewEntry =>
            {
                viewEntry.SlidingExpiration = TimeSpan.FromHours(1);

                var fileInfo = fileProvider.GetFileInfo(path);
                viewEntry.ExpirationTokens.Add(fileProvider.Watch(path));

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
}