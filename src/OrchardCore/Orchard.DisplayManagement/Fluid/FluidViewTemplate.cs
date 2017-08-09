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
        public static readonly string ViewsFolder = "Views";
        public static readonly string ViewExtension = ".liquid";
        public static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        static FluidViewTemplate()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register(typeof(ViewContext));
            TemplateContext.GlobalMemberAccessStrategy.Register<ModelStateNode>();
            TemplateContext.GlobalFilters.WithFluidViewFilters();
        }

        internal static async Task RenderAsync(FluidPage page)
        {
            var path = Path.ChangeExtension(page.ViewContext.ExecutingFilePath, ViewExtension);
            var fileProviderAccessor = page.GetService<IFluidViewFileProviderAccessor>();
            var fileInfo = fileProviderAccessor.ShellFileProvider.GetFileInfo(path);
            var shellCache = page.GetService<IMemoryCache>();

            IMemoryCache cache;
            IFileProvider fileProvider;

            if (fileInfo != null && fileInfo.Exists)
            {
                cache = shellCache;
                fileProvider = fileProviderAccessor.ShellFileProvider;
            }
            else
            {
                fileInfo = fileProviderAccessor.SharedFileProvider.GetFileInfo(path);

                if (fileInfo == null || !fileInfo.Exists)
                {
                    page.WriteLiteral($"<h1>{ Path.GetFileName(path) }</h1>");
                    return;
                }

                cache = Cache;
                fileProvider = fileProviderAccessor.SharedFileProvider;
            }

            var viewImportLocations = ViewHierarchyUtility.GetViewImportsLocations(
                Path.ChangeExtension(path, RazorViewEngine.ViewExtension))
                .Select(p => Path.ChangeExtension(p, ViewExtension));

            IFluidTemplate viewImportsTemplate = null;
            foreach (var location in viewImportLocations)
            {
                fileInfo = fileProviderAccessor.ShellFileProvider.GetFileInfo(location);

                if (fileInfo != null && fileInfo.Exists)
                {
                    viewImportsTemplate = Parse(location, fileProviderAccessor.ShellFileProvider, shellCache);
                    break;
                }
                else
                {
                    fileInfo = fileProviderAccessor.SharedFileProvider.GetFileInfo(location);

                    if (fileInfo != null && fileInfo.Exists)
                    {
                        viewImportsTemplate = Parse(location, fileProviderAccessor.SharedFileProvider, Cache);
                        break;
                    }
                }
            }

            path = Path.ChangeExtension(path, ViewExtension);
            var template = Parse(path, fileProvider, cache);

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

            context.RegisterObject((object)page.Model);
            context.LocalScope.SetValue("Model", page.Model);

            if (page.Model is Shape)
            {
                if (page.Model.Items.Count > 0)
                {
                    foreach (var item in page.Model.Items)
                    {
                        context.RegisterObject((object)item);
                    }
                }

                if (page.Model.Properties.Count > 0)
                {
                    foreach (var prop in page.Model.Properties)
                    {
                        context.RegisterObject((object)prop.Value);
                        context.LocalScope.SetValue(prop.Key, prop.Value);
                    }
                }
            }

            if (page.Model.Field != null)
            {
                context.RegisterObject(((object)page.Model.Field));
            }

            if (viewImportsTemplate != null)
            {
                await viewImportsTemplate.RenderAsync(context);
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
            }

            context.MemberAccessStrategy.Register(type);
        }
    }
}