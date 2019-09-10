using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace OrchardCore.Swagger
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                var routeTenantPrefix = String.Empty;

                var request = httpContextAccessor?.HttpContext?.Request;
                if (request != null && request.PathBase.HasValue)
                {
                    // remove trailing and leading slashes, and then add one, just to be sure
                    routeTenantPrefix = $"{request.PathBase.Value.TrimEnd('/').TrimStart('/')}/";
                }

                c.RoutePrefix = "swagger";
                foreach (var definition in serviceProvider.GetServices<ISwaggerApiDefinition>())
                {
                    c.SwaggerEndpoint($"/{routeTenantPrefix}swagger/{definition.Name}/swagger.json", definition.Name);
                }
            });
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var apiDefinitions = serviceProvider.GetServices<ISwaggerApiDefinition>();

                c.DocInclusionPredicate((name, description) => FilterDocInclusion(name, description, apiDefinitions));
                c.EnableAnnotations();
                PopulateSwagger(c, apiDefinitions);
            });
        }

        private bool FilterDocInclusion(string name, ApiDescription description, IEnumerable<ISwaggerApiDefinition> apiDefinitions)
        {
            foreach (var documentProvider in apiDefinitions)
            {
                if (
                    documentProvider.Name == name &&
                    documentProvider.ApiDescriptionFilterPredicate != null &&
                    documentProvider.ApiDescriptionFilterPredicate.Invoke(name, description))
                    return true;
            }

            return false;
        }

        private void PopulateSwagger(SwaggerGenOptions c, IEnumerable<ISwaggerApiDefinition> apiDefinitions)
        {
            foreach (var definition in apiDefinitions)
            {
                c.SwaggerDoc(definition.Name, definition.Document.Info);
            }
        }
    }

    [Feature("OrchardCore.Swagger.API")]
    public class SwaggerDocumentationStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ISwaggerApiDefinition, OrchardApiDefinition>();
        }
    }
}