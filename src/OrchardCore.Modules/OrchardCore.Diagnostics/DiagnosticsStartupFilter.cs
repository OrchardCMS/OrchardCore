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
        // RequestServices already follows the ambient ShellScope through ShellScopeServices.
        return new ShellScope(ShellScope.Context).UsingAsync(async scope =>
        {
            var razorViewFeature = context.Features.Get<RazorViewFeature>();
            var hasActionContext = context.Items.TryGetValue("OrchardCore:ActionContext", out var actionContext);

            context.Features.Set<RazorViewFeature>(null);
            context.Items.Remove("OrchardCore:ActionContext");

            try
            {
                await next(context);
            }
            finally
            {
                context.Features.Set(razorViewFeature);
                if (hasActionContext)
                {
                    context.Items["OrchardCore:ActionContext"] = actionContext;
                }
                else
                {
                    context.Items.Remove("OrchardCore:ActionContext");
                }
            }
        }, activateShell: false);
    }
}
