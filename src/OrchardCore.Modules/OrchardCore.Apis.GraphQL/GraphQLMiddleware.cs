using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Services;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly GraphQLSettings _settings;
        private readonly IDocumentExecuter _executer;
        private readonly IDocumentWriter _writer;

        public GraphQLMiddleware(
            RequestDelegate next,
            GraphQLSettings settings,
            IDocumentExecuter executer,
            IDocumentWriter writer)
        {
            _next = next;
            _settings = settings;
            _executer = executer;
            _writer = writer;
        }

        public async Task Invoke(HttpContext context, IAuthorizationService authorizationService, ISchemaService schemaService)
        {
            if (!IsGraphQLRequest(context))
            {
                await _next(context);
            }
            else
            {
                var authorized = await authorizationService.AuthorizeAsync(
                    context.User, Permissions.ExecuteGraphQL);

                if (authorized)
                {
                    await ExecuteAsync(context, schemaService);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
            }
        }

        private bool IsGraphQLRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments(_settings.Path)
                && String.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase);
        }

        private async Task ExecuteAsync(HttpContext context, ISchemaService schemaService)
        {
            var schema = await schemaService.GetSchema();

            string body;
            using (var streamReader = new StreamReader(context.Request.Body))
            {
                body = await streamReader.ReadToEndAsync().ConfigureAwait(true);
            }

            var request = JsonConvert.DeserializeObject<GraphQLRequest>(body);

            var queryToExecute = request.Query;
            if (!String.IsNullOrWhiteSpace(request.NamedQuery))
            {
                var namedQueries = context.RequestServices.GetServices<INamedQueryProvider>();

                var queries = namedQueries
                    .SelectMany(dict => dict.Resolve())
                    .ToDictionary(pair => pair.Key, pair => pair.Value);

                queryToExecute = queries[request.NamedQuery];
            }

            var result = await _executer.ExecuteAsync(_ =>
            {
                _.Schema = schema;
                _.Query = queryToExecute;
                _.OperationName = request.OperationName;
                _.Inputs = request.Variables.ToInputs();
                _.UserContext = _settings.BuildUserContext?.Invoke(context);
            });

            await WriteResponseAsync(context, result);
        }

        private Task WriteResponseAsync(HttpContext context, ExecutionResult result)
        {
            var json = _writer.Write(result);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = result.Errors?.Any() == true ? (int)HttpStatusCode.BadRequest : (int)HttpStatusCode.OK;

            return context.Response.WriteAsync(json);
        }
    }
}
