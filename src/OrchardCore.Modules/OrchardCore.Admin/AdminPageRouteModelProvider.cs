using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Hosting;
using OrchardCore.Mvc;

namespace OrchardCore.Admin
{
    internal class AdminPageRouteModelProvider : IPageRouteModelProvider
    {
        private const string RazorPageDocumentKind = "mvc.1.0.razor-page";

        private readonly IHostEnvironment _hostingEnvironment;
        private readonly ApplicationPartManager _applicationManager;

        public AdminPageRouteModelProvider(IHostEnvironment hostingEnvironment, ApplicationPartManager applicationManager)
        {
            _hostingEnvironment = hostingEnvironment;
            _applicationManager = applicationManager;
        }

        public int Order => 1000;

        public void OnProvidersExecuting(PageRouteModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IEnumerable<CompiledViewDescriptor> descriptors;

            var refsFolderExists = Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "refs"));

            if (_hostingEnvironment.IsDevelopment() && refsFolderExists)
            {
                descriptors = GetPageDescriptors<DevelopmentViewsFeature>(_applicationManager);
            }
            else
            {
                descriptors = GetPageDescriptors<ViewsFeature>(_applicationManager);
            }

            var adminPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var descriptor in descriptors)
            {
                foreach (var type in descriptor.Type.GetNestedTypes())
                {
                    var attribute = type.GetCustomAttribute<AdminAttribute>();

                    if (attribute != null)
                    {
                        adminPaths.Add(descriptor.RelativePath);
                        break;
                    }
                }
            }

            foreach (var model in context.RouteModels.ToArray())
            {
                if (adminPaths.Contains(model.RelativePath))
                {
                    model.Properties["Admin"] = null;
                }
            }
        }

        public void OnProvidersExecuted(PageRouteModelProviderContext context)
        {
        }

        private static IEnumerable<CompiledViewDescriptor> GetPageDescriptors<T>(ApplicationPartManager applicationManager) where T : ViewsFeature, new()
        {
            if (applicationManager == null)
            {
                throw new ArgumentNullException(nameof(applicationManager));
            }

            var viewsFeature = GetViewFeature<T>(applicationManager);

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var viewDescriptor in viewsFeature.ViewDescriptors)
            {
                if (!visited.Add(viewDescriptor.RelativePath))
                {
                    continue;
                }

                if (IsRazorPage(viewDescriptor))
                {
                    yield return viewDescriptor;
                }
            }
        }

        private static bool IsRazorPage(CompiledViewDescriptor viewDescriptor) => viewDescriptor.Item?.Kind == RazorPageDocumentKind;

        private static T GetViewFeature<T>(ApplicationPartManager applicationManager) where T : ViewsFeature, new()
        {
            var viewsFeature = new T();
            applicationManager.PopulateFeature(viewsFeature);
            return viewsFeature;
        }
    }
}
