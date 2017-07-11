using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;
using Orchard.DisplayManagement.Fluid.Filters;
using Orchard.DisplayManagement.Fluid.Internal;
using Orchard.DisplayManagement.Fluid.ModelBinding;
using Orchard.DisplayManagement.Shapes;
using Orchard.Settings;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidViewTemplate : FluidTemplate<ActivatorFluidParserFactory<FluidViewParser>>
    {
        public static readonly string ViewExtension = ".fluid";
        private static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        static FluidViewTemplate()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<JObject>((obj, name) => obj[name]);

            if (!FluidValue.TypeMappings.TryGetValue(typeof(JObject), out var value))
            {
                FluidValue.TypeMappings.Add(typeof(JObject), o => new ObjectValue(o));
                FluidValue.TypeMappings.Add(typeof(JValue), o => FluidValue.Create(((JValue)o).Value));
            }

            if (!FluidValue.TypeMappings.TryGetValue(typeof(DateTime?), out value))
            {
                FluidValue.TypeMappings.Add(typeof(DateTime?), o => new ObjectValue(o));
            }

            TemplateContext.GlobalMemberAccessStrategy.Register(typeof(ViewContext));
            TemplateContext.GlobalMemberAccessStrategy.Register<ModelStateNode>();

            TemplateContext.GlobalFilters.WithFluidViewFilters();
        }

        internal static async Task RenderAsync(FluidPage page)
        {
            var path = page.ViewContext.ExecutingFilePath.Replace(RazorViewEngine.ViewExtension, ViewExtension);
            var fileProvider = page.GetService<IFluidViewFileProviderAccessor>().FileProvider;

            var viewImportLocations = ViewHierarchyUtility.GetViewImportsLocations(
                path.Replace(ViewExtension, RazorViewEngine.ViewExtension))
                .Select(x => x.Replace(RazorViewEngine.ViewExtension, ViewExtension));

            IFluidTemplate viewImportsTemplate = null;
            foreach (var location in viewImportLocations)
            {
                var fileInfo = fileProvider.GetFileInfo(location);

                if (fileInfo.Exists)
                {
                    viewImportsTemplate = Parse(location, fileProvider);
                }
            }

            path = path.Replace(RazorViewEngine.ViewExtension, ViewExtension);
            var template = Parse(path, fileProvider);

            var context = new TemplateContext();
            context.AmbientValues.Add("FluidPage", page);

            var site = await page.GetService<ISiteService>().GetSiteSettingsAsync();
            context.MemberAccessStrategy.Register(site.GetType());
            context.LocalScope.SetValue("Site", site);

            context.MemberAccessStrategy.Register(page.Context.GetType());
            context.MemberAccessStrategy.Register(page.Context.Request.GetType());
            context.LocalScope.SetValue("Context", page.Context);

            context.LocalScope.SetValue("ViewData", page.ViewData);
            context.LocalScope.SetValue("ViewContext", page.ViewContext);

            var modelState = page.ViewContext.ModelState.ToDictionary(
                kv => kv.Key, kv => (object)new ModelStateNode(kv.Value));

            modelState["IsValid"] = page.ViewContext.ModelState.IsValid;
            context.LocalScope.SetValue("ModelState", modelState);

            var modelType = ((object)page.Model)?.GetType();

            if (page.Model is IShape && !FluidValue.TypeMappings.TryGetValue(modelType, out var value))
            {
                FluidValue.TypeMappings.Add(modelType, obj => new ObjectValue(obj));
            }

            if (modelType != null)
            {
                context.MemberAccessStrategy.Register(modelType);
                context.LocalScope.SetValue("Model", page.Model);

                if (page.Model is Shape && page.Model.Items.Count > 0)
                {
                    foreach (var item in page.Model.Items)
                    {
                        var itemType = ((object)item).GetType();

                        if (!FluidValue.TypeMappings.TryGetValue(itemType, out value))
                        {
                            FluidValue.TypeMappings.Add(itemType, obj => new ObjectValue(obj));
                            context.MemberAccessStrategy.Register(itemType);
                        }
                    }
                }

                if (page.Model is Shape && page.Model.Properties.Count > 0)
                {
                    foreach (var prop in page.Model.Properties)
                    {
                        var propType = ((object)prop.Value).GetType();

                        if (prop.Value is IShape && !FluidValue.TypeMappings.TryGetValue(propType, out value))
                        {
                            FluidValue.TypeMappings.Add(propType, obj => new ObjectValue(obj));
                            context.MemberAccessStrategy.Register(propType);
                        }

                        context.LocalScope.SetValue(prop.Key, prop.Value);
                    }
                }
            }

            if (viewImportsTemplate != null)
            {
                await viewImportsTemplate.RenderAsync(context);
            }

            page.WriteLiteral(await template.RenderAsync(context));
        }

        internal static IFluidTemplate Parse(string path, IFileProvider fileProvider)
        {
            return Cache.GetOrCreate(path, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(1);

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
}