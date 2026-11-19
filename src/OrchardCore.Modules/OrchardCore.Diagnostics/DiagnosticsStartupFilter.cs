using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Diagnostics;

public sealed class DiagnosticsStartupFilter : IStartupFilter
{
    // The key of the action context cached by 'HttpContextExtensions.GetActionContextAsync()'.
    private const string ActionContextKey = "OrchardCore:ActionContext";

    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    private readonly IHostEnvironment _hostEnvironment;

    public DiagnosticsStartupFilter(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            if (!_hostEnvironment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.Use(next => context =>
                context.Features.Get<IExceptionHandlerFeature>() is not null ||
                context.Features.Get<IStatusCodeReExecuteFeature>() is not null
                    ? ExecuteInErrorScopeAsync(context, next)
                    : next(context));

            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode < 200 || context.Response.StatusCode >= 400)
                {
                    if (_contentTypeProvider.TryGetContentType(context.Request.Path.Value, out var contentType))
                    {
                        var statusCodePagesFeature = context.Features.Get<IStatusCodePagesFeature>();
                        if (statusCodePagesFeature != null)
                        {
                            statusCodePagesFeature.Enabled = false;
                        }
                    }
                }
            });

            next(app);
        };
    }

    private static Task ExecuteInErrorScopeAsync(HttpContext context, RequestDelegate next)
    {
        // A scope on the current shell context, rather than one from 'IShellHost', keeps the error page on the
        // shell that handled the request. RequestServices already follows the ambient ShellScope through ShellScopeServices.
        return new ShellScope(ShellScope.Context).UsingAsync(async scope =>
        {
            var razorViewFeature = context.Features.Get<RazorViewFeature>();
            var hasActionContext = context.Items.Remove(ActionContextKey, out var actionContext);

            context.Features.Set<RazorViewFeature>(null);

            try
            {
                await next(context);
            }
            finally
            {
                context.Features.Set(razorViewFeature);
                context.Items.Remove(ActionContextKey);

                if (hasActionContext)
                {
                    context.Items[ActionContextKey] = actionContext;
                }
            }
        }, activateShell: false);
    }
}
