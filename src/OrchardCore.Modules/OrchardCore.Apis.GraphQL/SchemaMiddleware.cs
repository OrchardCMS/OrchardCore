using System.Net;
using System.Threading.Tasks;
using GraphQL.Utilities;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Apis.GraphQL
{
    public class SchemaMiddleware
    {
        private readonly RequestDelegate _next;

        public SchemaMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ISchemaFactory schemaService)
        {
            var schema = await schemaService.GetSchemaAsync();

            using (var printer = new SchemaPrinter(schema))
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.OK;

                await context.Response.WriteAsync(printer.Print());
            }
        }
    }
}
