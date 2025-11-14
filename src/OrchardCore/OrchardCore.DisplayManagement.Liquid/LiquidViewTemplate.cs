using System.Globalization;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using TimeZoneConverter;

namespace OrchardCore.DisplayManagement.Liquid;

internal static class LiquidViewTemplate
{
    public const string ViewsFolder = "Views";
    public const string ViewExtension = ".liquid";
    private static readonly MemoryCache s_cache = new(new MemoryCacheOptions());

    internal static Task RenderAsync(RazorPage<dynamic> page)
    {
        var services = page.Context.RequestServices;
        var path = Path.ChangeExtension(page.ViewContext.ExecutingFilePath, ViewExtension);

        var templateTask = ParseAsync(path, services, s_cache);

        return !templateTask.IsCompletedSuccessfully
            ? Awaited(templateTask, page, services)
            : RenderAsyncCore(page, services, templateTask.Result);

        static async Task Awaited(Task<IFluidTemplate> templateTask, RazorPage<dynamic> page, IServiceProvider services)
        {
            var template = await templateTask;
            await RenderAsyncCore(page, services, template);
        }
    }

    private static async Task RenderAsyncCore(RazorPage<dynamic> page, IServiceProvider services, IFluidTemplate template)
    {
        var templateOptions = services.GetRequiredService<IOptions<TemplateOptions>>().Value;
        var context = new LiquidTemplateContext(services, templateOptions);
        var htmlEncoder = services.GetRequiredService<HtmlEncoder>();

        // Defer the buffer disposing so that a template can be rendered twice.
        var content = new ViewBufferTextWriterContent(releaseOnWrite: false);
        ShellScope.Current.RegisterBeforeDispose((scope, content) => content.Dispose(), content);

        try
        {
            await context.EnterScopeAsync(page.ViewContext, (object)page.Model);
            await template.RenderAsync(content, htmlEncoder, context);

            // Use ViewBufferTextWriter.Write(object) from ASP.NET directly since it will use a special code path
            // for IHtmlContent. This prevent the TextWriter methods from copying the content from our buffer
            // if we did content.WriteTo(page.Output)

            page.Output.Write(content);
        }
        finally
        {
            context.ReleaseScope();
        }
    }

    private static Task<IFluidTemplate> ParseAsync(string path, IServiceProvider services, IMemoryCache cache)
    {
        return cache.GetOrCreateAsync(path, async entry =>
        {
            var parser = services.GetRequiredService<LiquidViewParser>();
            var templateOptions = services.GetRequiredService<IOptions<TemplateOptions>>().Value;
            var isDevelopment = services.GetRequiredService<IHostEnvironment>().IsDevelopment();

            var fileProvider = templateOptions.FileProvider;

            entry.SetSlidingExpiration(TimeSpan.FromHours(1));
            var fileInfo = fileProvider.GetFileInfo(path);

            if (isDevelopment)
            {
                entry.ExpirationTokens.Add(fileProvider.Watch(path));
            }

            await using var stream = fileInfo.CreateReadStream();
            using var sr = new StreamReader(stream);

            if (parser.TryParse(await sr.ReadToEndAsync(), out var template, out var errors))
            {
                return template;
            }

            throw new Exception($"Failed to parse liquid file {path}: {string.Join(System.Environment.NewLine, errors)}");
        });
    }
}

public static class LiquidViewTemplateExtensions
{
    public static async Task<string> RenderAsync(this IFluidTemplate template, TextEncoder encoder, LiquidTemplateContext context, object model)
    {
        var viewContextAccessor = context.Services.GetRequiredService<ViewContextAccessor>();
        var viewContext = viewContextAccessor.ViewContext;

        viewContext ??= viewContextAccessor.ViewContext = await GetViewContextAsync(context);

        try
        {
            await context.EnterScopeAsync(viewContext, model);
            return await template.RenderAsync(context, encoder);
        }
        finally
        {
            context.ReleaseScope();
        }
    }

    public static async Task RenderAsync(this IFluidTemplate template, TextWriter writer, TextEncoder encoder, LiquidTemplateContext context, object model)
    {
        var viewContextAccessor = context.Services.GetRequiredService<ViewContextAccessor>();
        var viewContext = viewContextAccessor.ViewContext;

        viewContext ??= viewContextAccessor.ViewContext = await GetViewContextAsync(context);

        try
        {
            await context.EnterScopeAsync(viewContext, model);
            await template.RenderAsync(writer, encoder, context);
        }
        finally
        {
            context.ReleaseScope();
        }
    }

    public static async Task<ViewContext> GetViewContextAsync(LiquidTemplateContext context)
    {
        // In .NET 10, IActionContextAccessor is obsolete, so we create ActionContext directly
        var httpContext = context.Services.GetRequiredService<IHttpContextAccessor>().HttpContext;
        var actionContext = await httpContext.GetActionContextAsync();

        return GetViewContext(actionContext);
    }

    internal static ViewContext GetViewContext(ActionContext actionContext)
    {
        var services = actionContext.HttpContext.RequestServices;

        var options = services.GetService<IOptions<MvcViewOptions>>();
        var viewEngine = options.Value.ViewEngines[0];

        var viewResult = viewEngine.GetView(executingFilePath: null,
            LiquidViewsFeatureProvider.DefaultRazorViewPath, isMainPage: true);

        var tempDataProvider = services.GetService<ITempDataProvider>();

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            new ViewDataDictionary(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: new ModelStateDictionary()),
            new TempDataDictionary(
                actionContext.HttpContext,
                tempDataProvider),
            TextWriter.Null,
            new HtmlHelperOptions());

        if (viewContext.View is RazorView razorView)
        {
            razorView.RazorPage.ViewContext = viewContext;
        }

        return viewContext;
    }
}

public static class LiquidTemplateContextExtensions
{
    internal static async ValueTask EnterScopeAsync(this LiquidTemplateContext context, ViewContext viewContext, object model)
    {
        if (!context.IsInitialized)
        {
            var localClock = context.Services.GetRequiredService<ILocalClock>();

            // Configure Fluid with the time zone to represent local date and times
            var localTimeZone = await localClock.GetLocalTimeZoneAsync();

            if (TZConvert.TryGetTimeZoneInfo(localTimeZone.TimeZoneId, out var timeZoneInfo))
            {
                context.TimeZone = timeZoneInfo;
            }

            // Configure Fluid with the local date and time
            var now = await localClock.GetLocalNowAsync();

            context.Now = () => now;

            context.ViewContext = viewContext;

            context.CultureInfo = CultureInfo.CurrentUICulture;

            context.IsInitialized = true;
        }

        context.EnterChildScope();

        var viewLocalizer = context.Services.GetRequiredService<IViewLocalizer>();

        if (viewLocalizer is IViewContextAware contextable)
        {
            contextable.Contextualize(viewContext);
        }

        context.SetValue("ViewLocalizer", new ObjectValue(viewLocalizer));

        if (context.GetValue("Model")?.ToObjectValue() == model && model is IShape shape)
        {
            if (context.ShapeRecursions++ > LiquidTemplateContext.MaxShapeRecursions)
            {
                throw new InvalidOperationException(
                    $"The '{shape.Metadata.Type}' shape has been called recursively more than {LiquidTemplateContext.MaxShapeRecursions} times.");
            }
        }
        else
        {
            context.ShapeRecursions = 0;
        }

        context.SetValue("Model", model);
    }
}
