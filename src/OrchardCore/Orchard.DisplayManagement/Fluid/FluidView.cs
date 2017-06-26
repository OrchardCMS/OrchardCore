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
    public class FluidView : Razor.RazorPage<dynamic>
    {
        public static readonly string ViewExtension = ".fluid";
        private static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        private IServiceProvider ServiceProvider => Context.RequestServices;

        static FluidView()
        {
            TemplateContext.GlobalFilters.WithFluidViewFilters();
            TemplateContext.GlobalMemberAccessStrategy.Register(typeof(ViewContext));
            TemplateContext.GlobalMemberAccessStrategy.Register<ModelStateNode>();
        }

        public override async Task ExecuteAsync()
        {
            var path = ViewContext.ExecutingFilePath.Replace(RazorViewEngine.ViewExtension, ViewExtension);
            var environment = ServiceProvider.GetRequiredService<IHostingEnvironment>();

            // Todo: use custom FileProviders through options
            // E.g to point to Module Projects files while in dev
            // E.g if some templates are defined through the database
            var template = ParseFluidFile(path, environment.ContentRootFileProvider);

            var context = new TemplateContext();
            context.AmbientValues.Add("FluidView", this);

            var site = await ServiceProvider.GetService<ISiteService>().GetSiteSettingsAsync();
            context.MemberAccessStrategy.Register(site.GetType());
            context.LocalScope.SetValue("Site", site);

            var urlHelperFactory = ServiceProvider.GetService<IUrlHelperFactory>();
            context.AmbientValues.Add("UrlHelper", urlHelperFactory.GetUrlHelper(ViewContext));

            context.MemberAccessStrategy.Register(Context.GetType());
            context.MemberAccessStrategy.Register(Context.Request.GetType());
            context.LocalScope.SetValue("Context", Context);

            context.LocalScope.SetValue("ViewData", ViewData);
            context.LocalScope.SetValue("ViewContext", ViewContext);

            var modelState = ViewContext.ModelState.ToDictionary(kv => kv.Key,
                kv => (object)new ModelStateNode(kv.Value));

            modelState["IsValid"] = ViewContext.ModelState.IsValid;
            context.LocalScope.SetValue("ModelState", modelState);

            if (Model != null)
            {
                context.LocalScope.SetValue("Model", Model);
                context.MemberAccessStrategy.Register(((object)Model).GetType());
            }

            WriteLiteral(await template.RenderAsync(context));
        }

        internal static IFluidTemplate ParseFluidFile(string path, IFileProvider fileProvider)
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
                        if (FluidViewTemplate.TryParse(sr.ReadToEnd(), out var template, out var errors))
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
