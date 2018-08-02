using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OrchardCore.Environment.Shell;
using Microsoft.AspNetCore.Mvc.Controllers;
using NSwag.Annotations;

namespace OrchardCore.Apis.OpenApi
{
    public class OpenApiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly OpenApiOptions _settings;
        private readonly ShellSettings _shellSettings;

        public OpenApiMiddleware(
            RequestDelegate next,
            IOptions<OpenApiOptions> settings,
            ShellSettings shellSettings)
        {
            _next = next;
            _settings = settings.Value;
            _shellSettings = shellSettings;
        }

        public Task Invoke(HttpContext context)
        {
            if (!IsOpenApiRequest(context))
            {
                return _next(context);
            }
            else
            {
                return ExecuteAsync(context);
            }
        }

        private bool IsOpenApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments(_settings.Path)
                && String.Equals(context.Request.Method, "GET", StringComparison.OrdinalIgnoreCase);
        }

        private async Task ExecuteAsync(HttpContext context)
        {
            var descriptionProvider = context
                .RequestServices
                .GetService<IApiDescriptionGroupCollectionProvider>();


            var document = new OpenApiDocument();

            document.Info = new OpenApiInfo
            {
                Title = _shellSettings.Name
            };

            document.Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = context.Request.Path }
            };

            document.Paths = new OpenApiPaths();

            foreach (var group in descriptionProvider.ApiDescriptionGroups.Items)
            {
                foreach (var description in group.Items)
                {
                    var responses = new OpenApiResponses();

                    var controller = ((ControllerActionDescriptor)description.ActionDescriptor);

                    var skippedMethod = controller
                        .MethodInfo.GetCustomAttribute<SwaggerIgnoreAttribute>();

                    if (skippedMethod != null)
                    {
                        continue;
                    }

                    var supportedResponses = controller
                        .MethodInfo.GetCustomAttributes<SwaggerResponseAttribute>();
                    
                    foreach (var supportedResponse in supportedResponses)
                    {
                        responses[supportedResponse.StatusCode] = new OpenApiResponse {
                            Description = supportedResponse.Description,
                        };
                    }

                    document.Paths.Add(
                        description.RelativePath,
                        new OpenApiPathItem
                        {
                            Operations = new Dictionary<OperationType, OpenApiOperation>
                            {
                                [(OperationType)Enum.Parse(typeof(OperationType), description.HttpMethod, true)] = new OpenApiOperation {
                                     Responses = responses
                                } 
                            }
                        }
                    );
                }
            }

            await context.WriteModelAsync(document);
        }
    }
}
