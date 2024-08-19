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

namespace OrchardCore.DisplayManagement.Razor;

public class RazorShapeTemplateViewEngine : IShapeTemplateViewEngine
{
    private readonly IOptions<MvcViewOptions> _options;
    private readonly IEnumerable<IRazorViewExtensionProvider> _viewExtensionProviders;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ViewContextAccessor _viewContextAccessor;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IHtmlHelper _htmlHelper;

    public RazorShapeTemplateViewEngine(
        IOptions<MvcViewOptions> options,
        IEnumerable<IRazorViewExtensionProvider> viewExtensionProviders,
        IHttpContextAccessor httpContextAccessor,
        ViewContextAccessor viewContextAccessor,
        ITempDataProvider tempDataProvider,
        IHtmlHelper htmlHelper)
    {
        _options = options;
        _viewExtensionProviders = viewExtensionProviders;
        _httpContextAccessor = httpContextAccessor;
        _viewContextAccessor = viewContextAccessor;
        _tempDataProvider = tempDataProvider;
        _htmlHelper = htmlHelper;
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
        var viewName = "/" + relativePath;
        viewName = Path.ChangeExtension(viewName, RazorViewEngine.ViewExtension);

        var viewContext = _viewContextAccessor.ViewContext;

        if (viewContext?.View != null)
        {
            var viewData = new ViewDataDictionary(viewContext.ViewData);
            viewData.TemplateInfo.HtmlFieldPrefix = displayContext.HtmlFieldPrefix;

            var htmlHelper = MakeHtmlHelper(viewContext, viewData);
            return htmlHelper.PartialAsync(viewName, displayContext.Value);
        }
        else
        {
            return RenderRazorViewAsync(viewName, displayContext);
        }
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
        var actionContext = await _httpContextAccessor.GetActionContextAsync();
        var view = FindView(actionContext, viewName, viewEngine);

        using var output = new ZStringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            new ViewDataDictionary(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: new ModelStateDictionary())
            {
                Model = model
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
            var newViewContext = new ViewContext(viewContext, viewContext.View, viewData, viewContext.Writer);
            contextAwareHelper.Contextualize(newViewContext);
        }

        return _htmlHelper;
    }
}
