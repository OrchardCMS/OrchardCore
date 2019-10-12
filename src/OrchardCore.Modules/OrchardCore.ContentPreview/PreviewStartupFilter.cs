using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.ContentPreview
{
    public class PreviewStartupFilter : IStartupFilter
    {
        public PreviewStartupFilter(IHostEnvironment hostEnvironment)
        {
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (context, next) =>
                {
                    if (context.Request.Method == "POST" && context.Request.Path.Value == "/OrchardCore.ContentPreview/Preview/Render")
                    {
                        await next();

                        string previewPath = null;

                        if (context.Items.TryGetValue("PreviewPath", out var previewPathObject) && previewPathObject != null)
                        {
                            previewPath = previewPathObject.ToString();
                        }

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
                    }
                    else
                    {
                        await next();
                    }
                });

                next(app);
            };
        }
    }
}
