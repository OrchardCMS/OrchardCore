using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.ValidationRules;
using OrchardCore.Routing;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly GraphQLSettings _settings;
        private readonly IDocumentExecuter _executer;

        internal static readonly Encoding _utf8Encoding = new UTF8Encoding(false);
        private readonly static MediaType _jsonMediaType = new MediaType("application/json");
        private readonly static MediaType _graphQlMediaType = new MediaType("application/graphql");

        public GraphQLMiddleware(
            RequestDelegate next,
            GraphQLSettings settings,
            IDocumentExecuter executer)
        {
            _next = next;
            _settings = settings;
            _executer = executer;
        }

        public async Task Invoke(HttpContext context, IAuthorizationService authorizationService, IAuthenticationService authenticationService, ISchemaFactory schemaService)
        {
            if (!IsGraphQLRequest(context))
            {
                await _next(context);
            }
            else
            {
                var authenticateResult = await authenticationService.AuthenticateAsync(context, "Api");

                if (authenticateResult.Succeeded)
                {
                    context.User = authenticateResult.Principal;
                }

                var authorized = await authorizationService.AuthorizeAsync(context.User, Permissions.ExecuteGraphQL);

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
            return context.Request.Path.StartsWithNormalizedSegments(_settings.Path, StringComparison.OrdinalIgnoreCase);
        }

        private async Task ExecuteAsync(HttpContext context, ISchemaFactory schemaService)
        {
            var schema = await schemaService.GetSchemaAsync();

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
                            // Asynchronous read is mandatory.
                            var json = await sr.ReadToEndAsync();
                            request = JObject.Parse(json).ToObject<GraphQLRequest>();
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
                        request = new GraphQLRequest
                        {
                            Query = context.Request.Query["query"]
                        };

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

                request = new GraphQLRequest
                {
                    Query = context.Request.Query["query"]
                };
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

            var dataLoaderDocumentListener = context.RequestServices.GetRequiredService<IDocumentExecutionListener>();

            var result = await _executer.ExecuteAsync(_ =>
            {
                _.Schema = schema;
                _.Query = queryToExecute;
                _.OperationName = request.OperationName;
                _.Inputs = request.Variables.ToInputs();
                _.UserContext = _settings.BuildUserContext?.Invoke(context);
                _.ExposeExceptions = _settings.ExposeExceptions;
                _.ValidationRules = DocumentValidator.CoreRules()
                                    .Concat(context.RequestServices.GetServices<IValidationRule>());
                _.ComplexityConfiguration = new ComplexityConfiguration
                {
                    MaxDepth = _settings.MaxDepth,
                    MaxComplexity = _settings.MaxComplexity,
                    FieldImpact = _settings.FieldImpact
                };
                _.Listeners.Add(dataLoaderDocumentListener);
            });

            context.Response.StatusCode = (int)(result.Errors == null || result.Errors.Count == 0
                ? HttpStatusCode.OK
                : result.Errors.Any(x => x.Code == RequiresPermissionValidationRule.ErrorCode)
                    ? HttpStatusCode.Unauthorized
                    : HttpStatusCode.BadRequest);

            context.Response.ContentType = "application/json";

            // Asynchronous write to the response body is mandatory.
            var encodedBytes = _utf8Encoding.GetBytes(JObject.FromObject(result).ToString());
            await context.Response.Body.WriteAsync(encodedBytes, 0, encodedBytes.Length);
        }

        private async Task WriteErrorAsync(HttpContext context, string message, Exception e = null)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var errorResult = new ExecutionResult
            {
                Errors = new ExecutionErrors()
            };

            if (e == null)
            {
                errorResult.Errors.Add(new ExecutionError(message));
            }
            else
            {
                errorResult.Errors.Add(new ExecutionError(message, e));
            }

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            // Asynchronous write to the response body is mandatory.
            var encodedBytes = _utf8Encoding.GetBytes(JObject.FromObject(errorResult).ToString());
            await context.Response.Body.WriteAsync(encodedBytes, 0, encodedBytes.Length);
        }
    }
}
