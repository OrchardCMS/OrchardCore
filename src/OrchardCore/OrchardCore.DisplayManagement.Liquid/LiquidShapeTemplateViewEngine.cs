using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using Fluid;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid;

/// <summary>
/// Renders the Liquid shape templates with Fluid directly, instead of rendering a Razor page as a partial view
/// for each of them, which creates and initializes many MVC objects for each rendered shape.
/// </summary>
public class LiquidShapeTemplateViewEngine : IShapeTemplateViewEngine
{
    private static readonly ConcurrentDictionary<string, LiquidShapeTemplateView> s_views = new(StringComparer.Ordinal);

    private readonly ViewContextAccessor _viewContextAccessor;
    private readonly LiquidTemplateContextAccessor _templateContextAccessor;
    private readonly RazorShapeTemplateViewEngine _razorShapeTemplateViewEngine;
    private readonly IServiceProvider _serviceProvider;
    private readonly TemplateOptions _templateOptions;
    private readonly HtmlEncoder _htmlEncoder;

    public LiquidShapeTemplateViewEngine(
        ViewContextAccessor viewContextAccessor,
        LiquidTemplateContextAccessor templateContextAccessor,
        RazorShapeTemplateViewEngine razorShapeTemplateViewEngine,
        IServiceProvider serviceProvider,
        IOptions<TemplateOptions> templateOptions,
        HtmlEncoder htmlEncoder)
    {
        _viewContextAccessor = viewContextAccessor;
        _templateContextAccessor = templateContextAccessor;
        _razorShapeTemplateViewEngine = razorShapeTemplateViewEngine;
        _serviceProvider = serviceProvider;
        _templateOptions = templateOptions.Value;
        _htmlEncoder = htmlEncoder;
    }

    public IEnumerable<string> TemplateFileExtensions => [LiquidViewTemplate.ViewExtension];

    public Task<IHtmlContent> RenderAsync(string relativePath, DisplayContext displayContext)
    {
        var viewContext = _viewContextAccessor.ViewContext;

        // Without a current view, the Razor page of the template creates the view context it is rendered with.
        if (viewContext?.View is null)
        {
            return _razorShapeTemplateViewEngine.RenderAsync(relativePath, displayContext);
        }

        var view = s_views.GetOrAdd(relativePath, static path => new LiquidShapeTemplateView('/' + path));

        var templateTask = LiquidViewTemplate.GetTemplateAsync(view.TemplatePath, _serviceProvider);

        if (!templateTask.IsCompletedSuccessfully)
        {
            return AwaitedAsync(templateTask, view, viewContext, displayContext);
        }

        return RenderAsyncCore(templateTask.Result, view, viewContext, displayContext);

        async Task<IHtmlContent> AwaitedAsync(ValueTask<IFluidTemplate> templateTask, LiquidShapeTemplateView view, ViewContext viewContext, DisplayContext displayContext)
            => await RenderAsyncCore(await templateTask, view, viewContext, displayContext);
    }

    private async Task<IHtmlContent> RenderAsyncCore(IFluidTemplate template, LiquidShapeTemplateView view, ViewContext viewContext, DisplayContext displayContext)
    {
        // As when it is rendered by its Razor page as a partial view, the template gets its own view data whose model
        // is the shape, e.g. for the model expressions of the tag helpers, and whose prefix is the one of the shape.
        var viewData = new ViewDataDictionary<object>(viewContext.ViewData, displayContext.Value);
        viewData.TemplateInfo.HtmlFieldPrefix = displayContext.HtmlFieldPrefix;

        // Defer the buffer disposing so that a template can be rendered twice.
        var content = new ViewBufferTextWriterContent(releaseOnWrite: false);
        ShellScope.Current.RegisterBeforeDispose(scope => content.Dispose());

        // As when it is rendered by its Razor page, the template gets its own view context, which is the current one
        // while it is rendered, and whose executing file path is the one of its Razor page, e.g. to localize the template.
        var templateViewContext = new ViewContext(viewContext, view, viewData, content)
        {
            ExecutingFilePath = view.Path,
        };

        _viewContextAccessor.ViewContext = templateViewContext;

        try
        {
            // The context is shared by all the templates of the request, each one is rendered in its own scope.
            var context = _templateContextAccessor.GetContext(_templateOptions);

            await context.InvokeInScopeAsync(
                templateViewContext,
                displayContext.Value,
                static state => state.Template.RenderAsync(state.Content, state.HtmlEncoder, state.Context),
                (Template: template, Content: content, HtmlEncoder: _htmlEncoder, Context: context));
        }
        finally
        {
            _viewContextAccessor.ViewContext = viewContext;
        }

        return content;
    }

    /// <summary>
    /// The view of a Liquid shape template, which is only used to describe it in its view context.
    /// </summary>
    private sealed class LiquidShapeTemplateView : IView
    {
        public LiquidShapeTemplateView(string templatePath)
        {
            TemplatePath = templatePath;
            Path = System.IO.Path.ChangeExtension(templatePath, RazorViewEngine.ViewExtension);
        }

        /// <summary>
        /// Gets the path of the Liquid template file.
        /// </summary>
        public string TemplatePath { get; }

        /// <summary>
        /// Gets the path of the Razor page of the template.
        /// </summary>
        public string Path { get; }

        public Task RenderAsync(ViewContext context)
            => throw new NotSupportedException("A Liquid shape template is rendered by the 'LiquidShapeTemplateViewEngine'.");
    }
}
