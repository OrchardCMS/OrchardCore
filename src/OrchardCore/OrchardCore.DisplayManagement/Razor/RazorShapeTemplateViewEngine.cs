using System.Collections.Concurrent;
using Cysharp.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Implementation;

using MvcRazorPage = Microsoft.AspNetCore.Mvc.Razor.IRazorPage;

namespace OrchardCore.DisplayManagement.Razor;

public class RazorShapeTemplateViewEngine : IShapeTemplateViewEngine
{
    // Maps the relative paths of the shape templates to their views, to not allocate them on each rendering.
    private static readonly ConcurrentDictionary<string, ShapeTemplateView> s_views = new(StringComparer.Ordinal);

    private readonly IOptions<MvcViewOptions> _options;
    private readonly IEnumerable<IRazorViewExtensionProvider> _viewExtensionProviders;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ViewContextAccessor _viewContextAccessor;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IHtmlHelper _htmlHelper;
    private readonly IRazorViewEngine _razorViewEngine;
    private readonly IRazorPageActivator _pageActivator;

    public RazorShapeTemplateViewEngine(
        IOptions<MvcViewOptions> options,
        IEnumerable<IRazorViewExtensionProvider> viewExtensionProviders,
        IHttpContextAccessor httpContextAccessor,
        ViewContextAccessor viewContextAccessor,
        ITempDataProvider tempDataProvider,
        IHtmlHelper htmlHelper)
        : this(options, viewExtensionProviders, httpContextAccessor, viewContextAccessor, tempDataProvider, htmlHelper, null, null)
    {
    }

    public RazorShapeTemplateViewEngine(
        IOptions<MvcViewOptions> options,
        IEnumerable<IRazorViewExtensionProvider> viewExtensionProviders,
        IHttpContextAccessor httpContextAccessor,
        ViewContextAccessor viewContextAccessor,
        ITempDataProvider tempDataProvider,
        IHtmlHelper htmlHelper,
        IRazorViewEngine razorViewEngine,
        IRazorPageActivator pageActivator)
    {
        _options = options;
        _viewExtensionProviders = viewExtensionProviders;
        _httpContextAccessor = httpContextAccessor;
        _viewContextAccessor = viewContextAccessor;
        _tempDataProvider = tempDataProvider;
        _htmlHelper = htmlHelper;
        _razorViewEngine = razorViewEngine;
        _pageActivator = pageActivator;
    }

    public IEnumerable<string> TemplateFileExtensions
    {
        get
        {
            yield return RazorViewEngine.ViewExtension;
            foreach (var provider in _viewExtensionProviders)
            {
                yield return provider.ViewExtension;
            }
        }
    }

    public Task<IHtmlContent> RenderAsync(string relativePath, DisplayContext displayContext)
    {
        var view = s_views.GetOrAdd(relativePath, static path => new ShapeTemplateView(Path.ChangeExtension("/" + path, RazorViewEngine.ViewExtension)));

        var viewContext = _viewContextAccessor.ViewContext;

        if (viewContext?.View != null)
        {
            // The compiled page of the template is executed directly in the current view context, instead of being
            // rendered as a partial view, which creates a second view buffer and moves the rendered pages of the
            // template into the buffer of the partial view, for each rendered shape.
            // The page is obtained from the view engine, whose lookup cache holds the page factory of each view path
            // and the change tokens of its compilation, so that it is still recompiled when runtime compilation is on.
            if (_razorViewEngine is not null && _pageActivator is not null &&
                _razorViewEngine.GetView(executingFilePath: null, view.Path, isMainPage: false) is { Success: true, View: RazorView razorView })
            {
                return RenderPageAsync(razorView.RazorPage, view, viewContext, displayContext);
            }

            // The partial view gets its own copy of the view data, so a copy with a different 'HtmlFieldPrefix'
            // is only needed when the prefix changes, which is not the case of most displayed shapes.
            var viewData = viewContext.ViewData;

            if (!string.Equals(viewData.TemplateInfo.HtmlFieldPrefix, displayContext.HtmlFieldPrefix ?? string.Empty, StringComparison.Ordinal))
            {
                viewData = new ViewDataDictionary(viewContext.ViewData);
                viewData.TemplateInfo.HtmlFieldPrefix = displayContext.HtmlFieldPrefix;
            }

            var htmlHelper = MakeHtmlHelper(viewContext, viewData);
            return htmlHelper.PartialAsync(view.Path, displayContext.Value);
        }
        else
        {
            return RenderRazorViewAsync(view.Path, displayContext);
        }
    }

    /// <summary>
    /// Executes the compiled page of a shape template as a partial view of the current view would, with its own view
    /// data whose model is the shape and whose prefix is the one of the shape, and its own view context whose executing
    /// file path is the one of the template, e.g. to localize it.
    /// </summary>
    private async Task<IHtmlContent> RenderPageAsync(MvcRazorPage page, ShapeTemplateView view, ViewContext viewContext, DisplayContext displayContext)
    {
        var viewData = new ViewDataDictionary<object>(viewContext.ViewData, displayContext.Value);
        viewData.TemplateInfo.HtmlFieldPrefix = displayContext.HtmlFieldPrefix ?? string.Empty;

        // The template writes to a buffer, as a partial view does, in which the content of its child shapes is
        // kept by reference, so that the whole page is written once to the response.
        var writer = new ShapeTemplateWriter();

        var pageContext = new ViewContext(viewContext, view, viewData, writer)
        {
            ExecutingFilePath = page.Path ?? view.Path,
        };

        try
        {
            // Setting the view context of an Orchard Core Razor page also makes it the current one.
            page.ViewContext = pageContext;
            _pageActivator.Activate(page, pageContext);

            await page.ExecuteAsync();
        }
        finally
        {
            _viewContextAccessor.ViewContext = viewContext;
        }

        if (!string.IsNullOrEmpty(page.Layout))
        {
            throw new InvalidOperationException($"The shape template '{view.Path}' sets a layout, which is not supported. A shape is rendered where its parent displays it.");
        }

        return writer;
    }

    private async Task<IHtmlContent> RenderRazorViewAsync(string viewName, DisplayContext displayContext)
    {
        var viewEngines = _options.Value.ViewEngines;

        if (viewEngines.Count == 0)
        {
            throw new InvalidOperationException(string.Format("'{0}.{1}' must not be empty. At least one '{2}' is required to locate a view for rendering.",
                typeof(MvcViewOptions).FullName,
                nameof(MvcViewOptions.ViewEngines),
                typeof(IViewEngine).FullName));
        }

        var viewEngine = viewEngines[0];

        var result = await RenderViewToStringAsync(viewName, displayContext.Value, viewEngine);

        return new HtmlString(result);
    }

    public async Task<string> RenderViewToStringAsync(string viewName, object model, IViewEngine viewEngine)
    {
        var actionContext = await _httpContextAccessor.HttpContext.GetActionContextAsync();
        var view = FindView(actionContext, viewName, viewEngine);

        using var output = new ZStringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            new ViewDataDictionary(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: new ModelStateDictionary())
            {
                Model = model,
            },
            new TempDataDictionary(
                actionContext.HttpContext,
                _tempDataProvider),
            output,
            new HtmlHelperOptions());

        await view.RenderAsync(viewContext);

        return output.ToString();
    }

    private static IView FindView(ActionContext actionContext, string viewName, IViewEngine viewEngine)
    {
        var getViewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        var findViewResult = viewEngine.FindView(actionContext, viewName, isMainPage: true);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(
            System.Environment.NewLine,
            new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations));

        throw new InvalidOperationException(errorMessage);
    }

    private IHtmlHelper MakeHtmlHelper(ViewContext viewContext, ViewDataDictionary viewData)
    {
        if (_htmlHelper is IViewContextAware contextAwareHelper)
        {
            var newViewContext = viewData == viewContext.ViewData
                ? viewContext
                : new ViewContext(viewContext, viewContext.View, viewData, viewContext.Writer);

            contextAwareHelper.Contextualize(newViewContext);
        }

        return _htmlHelper;
    }

    /// <summary>
    /// The view of a Razor shape template, which only describes it in the view context of its page.
    /// </summary>
    private sealed class ShapeTemplateView : IView
    {
        public ShapeTemplateView(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public Task RenderAsync(ViewContext context)
            => throw new NotSupportedException("A Razor shape template is executed by the 'RazorShapeTemplateViewEngine'.");
    }
}
