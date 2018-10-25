using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly GraphQLSettings _settings;
        private readonly IDocumentExecuter _executer;
        private readonly IDocumentWriter _writer;

        private readonly static JsonSerializer _serializer = new JsonSerializer();

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

        public async Task Invoke(HttpContext context, IAuthorizationService authorizationService, IAuthenticationService authenticationService, ISchemaFactory schemaService)
        {
            if (!IsGraphQLRequest(context))
            {
                await _next(context);
            }
            else
            {
                var principal = context.User;

                var authenticateResult = await authenticationService.AuthenticateAsync(context, "Api");

                if (authenticateResult.Succeeded)
                {
                    principal = authenticateResult.Principal;
                }

                var authorized = await authorizationService.AuthorizeAsync(principal, Permissions.ExecuteGraphQL);

                if (authorized)
                {
                    await ExecuteAsync(context, schemaService);
                }
                else
                {
                    await context.ChallengeAsync("Api");
                }
            }
        }

        private bool IsGraphQLRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments(_settings.Path)
                && String.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase);
        }

        private async Task ExecuteAsync(HttpContext context, ISchemaFactory schemaService)
        {
            var schema = await schemaService.GetSchema();

            GraphQLRequest request;

            using (var sr = new StreamReader(context.Request.Body))
            {
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    request = _serializer.Deserialize<GraphQLRequest>(jsonTextReader);
                }
            }

            var queryToExecute = request.Query;
            if (!String.IsNullOrEmpty(request.NamedQuery))
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
#if DEBUG
                _.ExposeExceptions = true;
#endif
            });

            await WriteResponseAsync(context, result);
        }

        private async Task WriteResponseAsync(HttpContext context, ExecutionResult result)
        {
            var json = await _writer.WriteToStringAsync(result);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = result.Errors?.Any() == true ? (int)HttpStatusCode.BadRequest : (int)HttpStatusCode.OK;

            await context.Response.WriteAsync(json);
        }
    }
}
