using System.Net;
using System.Net.Mime;
using System.Text;
using GraphQL;
using GraphQL.Execution;
using GraphQL.SystemTextJson;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;
using GraphQL.Validation.Rules.Custom;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.ValidationRules;
using OrchardCore.Routing;

namespace OrchardCore.Apis.GraphQL;

public class GraphQLMiddleware : IMiddleware
{
    private readonly ILogger _logger;
    private readonly GraphQLSettings _settings;
    private readonly IGraphQLTextSerializer _graphQLTextSerializer;
    private readonly IGraphQLSerializer _serializer;
    private readonly IDocumentExecuter _executer;
    internal static readonly Encoding _utf8Encoding = new UTF8Encoding(false);
    private static readonly MediaType _jsonMediaType = new("application/json");
    private static readonly MediaType _graphQlMediaType = new("application/graphql");

    public GraphQLMiddleware(
        IOptions<GraphQLSettings> settingsOption,
        IDocumentExecuter executer,
        IGraphQLSerializer serializer,
        IGraphQLTextSerializer graphQLTextSerializer,
        ILogger<GraphQLMiddleware> logger)
    {
        _settings = settingsOption.Value;
        _executer = executer;
        _serializer = serializer;
        _graphQLTextSerializer = graphQLTextSerializer;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!IsGraphQLRequest(context))
        {
            await next(context);
        }
        else
        {
            var authenticationService = context.RequestServices.GetService<IAuthenticationService>();
            var authenticateResult = await authenticationService.AuthenticateAsync(context, "Api");
            if (authenticateResult.Succeeded)
            {
                context.User = authenticateResult.Principal;
            }
            var authorizationService = context.RequestServices.GetService<IAuthorizationService>();
            var authorized = await authorizationService.AuthorizeAsync(context.User, CommonPermissions.ExecuteGraphQL);

            if (authorized)
            {
                await ExecuteAsync(context);
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

    private async Task ExecuteAsync(HttpContext context)
    {
        GraphQLNamedQueryRequest request = null;

        // c.f. https://graphql.org/learn/serving-over-http/#post-request

        try
        {
            if (HttpMethods.IsPost(context.Request.Method))
            {
                var mediaType = new MediaType(context.Request.ContentType);

                if (mediaType.IsSubsetOf(_jsonMediaType) || mediaType.IsSubsetOf(_graphQlMediaType))
                {
                    using var sr = new StreamReader(context.Request.Body);
                    if (mediaType.IsSubsetOf(_graphQlMediaType))
                    {
                        request = new GraphQLNamedQueryRequest
                        {
                            Query = await sr.ReadToEndAsync()
                        };
                    }
                    else
                    {
                        request = _graphQLTextSerializer.Deserialize<GraphQLNamedQueryRequest>(await sr.ReadToEndAsync());
                    }
                }
                else
                {
                    request = CreateRequestFromQueryString(context);
                }
            }
            else if (HttpMethods.IsGet(context.Request.Method))
            {
                request = CreateRequestFromQueryString(context, true);
            }

            if (request == null)
            {
                throw new InvalidOperationException("Unable to create a graphqlrequest from this request");
            }
        }
        catch (Exception e)
        {
            await _serializer.WriteErrorAsync(context, "An error occurred while processing the GraphQL query", e);
            _logger.LogError(e, "An error occurred while processing the GraphQL query.");

            return;
        }

        var queryToExecute = request.Query;

        if (!string.IsNullOrEmpty(request.NamedQuery))
        {
            var namedQueries = context.RequestServices.GetServices<INamedQueryProvider>();

            var queries = namedQueries
                .SelectMany(dict => dict.Resolve())
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            queryToExecute = queries[request.NamedQuery];
        }

        var schemaService = context.RequestServices.GetService<ISchemaFactory>();
        var schema = await schemaService.GetSchemaAsync();
        var dataLoaderDocumentListener = context.RequestServices.GetRequiredService<IDocumentExecutionListener>();
        var result = await _executer.ExecuteAsync(options =>
        {
            options.Schema = schema;
            options.Query = queryToExecute;
            options.OperationName = request.OperationName;
            options.Variables = request.Variables;
            options.UserContext = _settings.BuildUserContext?.Invoke(context);
            options.ValidationRules = DocumentValidator.CoreRules
                .Concat(context.RequestServices.GetServices<IValidationRule>())
                .Append(new ComplexityValidationRule(new ComplexityConfiguration
                {
                    MaxDepth = _settings.MaxDepth,
                    MaxComplexity = _settings.MaxComplexity,
                    FieldImpact = _settings.FieldImpact
                }));
            options.Listeners.Add(dataLoaderDocumentListener);
            options.RequestServices = context.RequestServices;
        });

        context.Response.StatusCode = (int)(result.Errors == null || result.Errors.Count == 0
            ? HttpStatusCode.OK
            : result.Errors.Any(x => x is ValidationError ve && ve.Number == RequiresPermissionValidationRule.ErrorCode)
                ? HttpStatusCode.Unauthorized
                : HttpStatusCode.BadRequest);

        context.Response.ContentType = MediaTypeNames.Application.Json;

        await _serializer.WriteAsync(context.Response.Body, result);
    }

    private GraphQLNamedQueryRequest CreateRequestFromQueryString(HttpContext context, bool validateQueryKey = false)
    {
        if (!context.Request.Query.ContainsKey("query"))
        {
            if (validateQueryKey)
            {
                throw new InvalidOperationException("The 'query' query string parameter is missing");
            }

            return null;
        }

        var request = new GraphQLNamedQueryRequest
        {
            Query = context.Request.Query["query"]
        };

        if (context.Request.Query.ContainsKey("variables"))
        {
            request.Variables = _graphQLTextSerializer.Deserialize<Inputs>(context.Request.Query["variables"]);
        }

        if (context.Request.Query.ContainsKey("operationName"))
        {
            request.OperationName = context.Request.Query["operationName"];
        }

        return request;
    }
}
