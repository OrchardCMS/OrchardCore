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
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private readonly static MediaType _jsonMediaType = new MediaType("application/json");
        private readonly static MediaType _graphQlMediaType = new MediaType("application/graphql");

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
            return context.Request.Path.StartsWithSegments(_settings.Path);
        }

        private async Task ExecuteAsync(HttpContext context, ISchemaFactory schemaService)
        {
            var schema = await schemaService.GetSchema();

            GraphQLRequest request = null;

            // c.f. https://graphql.org/learn/serving-over-http/#post-request

            if (HttpMethods.IsPost(context.Request.Method))
            {

                var mediaType = new MediaType(context.Request.ContentType);

                try
                {
                    if (mediaType.IsSubsetOf(_jsonMediaType))
                    {
                        using (var sr = new StreamReader(context.Request.Body))
                        {
                            using (var jsonTextReader = new JsonTextReader(sr))
                            {
                                request = _serializer.Deserialize<GraphQLRequest>(jsonTextReader);
                            }
                        }
                    }
                    else if (mediaType.IsSubsetOf(_graphQlMediaType))
                    {
                        request = new GraphQLRequest();

                        using (var sr = new StreamReader(context.Request.Body))
                        {
                            request.Query = await sr.ReadToEndAsync();
                        }
                    }
                    else if (context.Request.Query.ContainsKey("query"))
                    {
                        request = new GraphQLRequest();

                        request.Query = context.Request.Query["query"];

                        if (context.Request.Query.ContainsKey("variables"))
                        {
                            request.Variables = JObject.Parse(context.Request.Query["variables"]);
                        }

                        if (context.Request.Query.ContainsKey("operationName"))
                        {
                            request.OperationName = context.Request.Query["operationName"];
                        }
                    }
                }
                catch (Exception e)
                {
                    await WriteErrorAsync(context, "An error occurred while processing the GraphQL query", e);
                    return;
                }
            }
            else if (HttpMethods.IsGet(context.Request.Method))
            {
                if (!context.Request.Query.ContainsKey("query"))
                {
                    await WriteErrorAsync(context, "The 'query' query string parameter is missing");
                    return;
                }

                request = new GraphQLRequest();

                request.Query = context.Request.Query["query"];
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
                _.ExposeExceptions = _settings.ExposeExceptions;
            });

            var httpResult = result.Errors?.Count > 0
                ? HttpStatusCode.BadRequest
                : HttpStatusCode.OK;

            context.Response.StatusCode = (int)httpResult;
            context.Response.ContentType = "application/json";

            await _writer.WriteAsync(context.Response.Body, result);
        }

        private async Task WriteErrorAsync(HttpContext context, string message, Exception e = null)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var errorResult = new ExecutionResult();
            errorResult.Errors = new ExecutionErrors();

            if (e == null)
            {
                errorResult.Errors.Add(new ExecutionError(message));
            }
            else
            {
                errorResult.Errors.Add(new ExecutionError(message, e));
            }

            context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            await _writer.WriteAsync(context.Response.Body, errorResult);
        }
    }
}
