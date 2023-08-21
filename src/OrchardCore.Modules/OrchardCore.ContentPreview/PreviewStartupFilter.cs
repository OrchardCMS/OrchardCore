using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.ContentPreview
{
    public class PreviewStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (context, next) =>
                {
                    await next();

                    if (!context.Items.TryGetValue("PreviewPath", out var previewPathObject) || previewPathObject == null)
                    {
                        return;
                    }

                    var previewPath = previewPathObject.ToString();

                    if (!String.IsNullOrWhiteSpace(previewPath) && previewPath.StartsWith('/'))
                    {
                        var originalPath = context.Request.Path;
                        var originalQueryString = context.Request.QueryString;

                        context.Request.Path = previewPath;
                        context.Items.Remove("PreviewPath");

                        context.SetEndpoint(endpoint: null);
                        context.Request.RouteValues.Clear();

                        try
                        {
                            await next();
                        }
                        finally
                        {
                            context.Request.QueryString = originalQueryString;
                            context.Request.Path = originalPath;
                        }
                    }
                });

                next(app);
            };
        }
    }
}
