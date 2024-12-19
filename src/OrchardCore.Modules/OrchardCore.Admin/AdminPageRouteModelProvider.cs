using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.Extensions.Hosting;
using OrchardCore.Mvc;

namespace OrchardCore.Admin;

internal sealed class AdminPageRouteModelProvider : IPageRouteModelProvider
{
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
        ArgumentNullException.ThrowIfNull(context);

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

    private static IEnumerable<CompiledViewDescriptor> GetPageDescriptors<T>(ApplicationPartManager applicationManager)
        where T : ViewsFeature, new()
    {
        ArgumentNullException.ThrowIfNull(applicationManager);

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

    private static bool IsRazorPage(CompiledViewDescriptor viewDescriptor) =>
        viewDescriptor.Item?.Kind == RazorPageDocumentClassifierPass.RazorPageDocumentKind;

    private static T GetViewFeature<T>(ApplicationPartManager applicationManager) where T : ViewsFeature, new()
    {
        var viewsFeature = new T();
        applicationManager.PopulateFeature(viewsFeature);
        return viewsFeature;
    }
}
