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

namespace OrchardCore.OpenApi.Swagger
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";

                var swaggerPath = httpContextAccessor.HttpContext.Request.PathBase + new PathString("/swagger");

                foreach (var definition in serviceProvider.GetServices<IOpenApiDefinition>())
                {
                    c.SwaggerEndpoint($"{swaggerPath}/{definition.Name}/swagger.json", definition.Name);
                }
            });
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var apiDefinitions = serviceProvider.GetServices<IOpenApiDefinition>();

                c.DocInclusionPredicate((name, description) => FilterDocInclusion(name, description, apiDefinitions));
                c.EnableAnnotations();
                PopulateSwagger(c, apiDefinitions);
            });
        }

        private bool FilterDocInclusion(string name, ApiDescription description, IEnumerable<IOpenApiDefinition> apiDefinitions)
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

        private void PopulateSwagger(SwaggerGenOptions c, IEnumerable<IOpenApiDefinition> apiDefinitions)
        {
            foreach (var definition in apiDefinitions)
            {
                c.SwaggerDoc(definition.Name, definition.Document.Info);
            }
        }
    }
}