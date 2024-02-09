using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Contents.Endpoints;

public class EndpointView<T> : IResult
{
    private const string ModelKey = "Model";

    public string ViewName { get; }

    public T Model { get; set; }

    public EndpointView(string viewName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(viewName);

        ViewName = viewName;
    }

    public EndpointView(string viewName, T model)
        : this(viewName)
    {
        Model = model;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var viewEngineOptions = httpContext.RequestServices.GetService<IOptions<MvcViewOptions>>().Value;

        var viewEngines = viewEngineOptions.ViewEngines;

        if (viewEngines.Count == 0)
        {
            throw new InvalidOperationException(string.Format("'{0}.{1}' must not be empty. At least one '{2}' is required to locate a view for rendering.",
                typeof(MvcViewOptions).FullName,
                nameof(MvcViewOptions.ViewEngines),
                typeof(IViewEngine).FullName));
        }

        var viewEngine = viewEngines[0];

        var viewEngineResult = viewEngine.GetView(executingFilePath: null, ViewName, isMainPage: true);
        var modelStateAccessor = httpContext.RequestServices.GetService<IUpdateModelAccessor>();

        var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor(), modelStateAccessor.ModelUpdater.ModelState);

        if (viewEngineResult.Success)
        {
            await WriteAsync(httpContext, viewEngineResult.View, actionContext, modelStateAccessor.ModelUpdater.ModelState);

            return;
        }

        var findViewResult = viewEngine.FindView(actionContext, ViewName, isMainPage: true);
        if (findViewResult.Success)
        {
            await WriteAsync(httpContext, viewEngineResult.View, actionContext, modelStateAccessor.ModelUpdater.ModelState);

            return;
        }

        httpContext.Response.StatusCode = 404;

        var searchedLocations = viewEngineResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(System.Environment.NewLine,
            new[] { $"Unable to find view '{ViewName}'. The following locations were searched:" }.Concat(searchedLocations));

        var bytes = Encoding.UTF8.GetBytes(errorMessage);
        await httpContext.Response.Body.WriteAsync(bytes);
    }

    private async Task WriteAsync(HttpContext httpContext, IView view, ActionContext actionContext, ModelStateDictionary modelState)
    {
        var modelMetadataProvider = httpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();

        var viewDataDictionary = new ViewDataDictionary<T>(modelMetadataProvider, modelState);

        if (Model is not null)
        {
            viewDataDictionary.Add(ModelKey, Model);
        }

        var tempData = httpContext.RequestServices.GetService<ITempDataProvider>();
        var streamWriter = new StreamWriter(httpContext.Response.Body);

        var viewContext = new ViewContext(
            actionContext,
            view,
            viewDataDictionary,
            new TempDataDictionary(httpContext, tempData),
            streamWriter,
            new HtmlHelperOptions());

        await view.RenderAsync(viewContext);
    }
}

public class EndpointView : EndpointView<object>
{
    public EndpointView(string viewName)
        : base(viewName)
    {

    }


    public EndpointView(string viewName, object model)
        : base(viewName, model)
    {
    }
}
