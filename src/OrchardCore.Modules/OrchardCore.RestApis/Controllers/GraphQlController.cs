using System;
using System.Net.Http;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL.Validation.Complexity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrchardCore.RestApis.Controllers
{
    [Route("graphql")]
    public class GraphQLController : Controller
    {
        private readonly IDocumentExecuter _documentExecuter;
        private readonly ISchema _schema;
        private readonly ILogger _logger;

        public GraphQLController(
            IDocumentExecuter documentExecuter,
            ISchema schema,
            ILogger<GraphQLController> logger)
        {
            _documentExecuter = documentExecuter;
            _schema = schema;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAsync(string query)
        {
            var executionOptions = new ExecutionOptions { Schema = _schema, Query = query };

            try
            {
                var result = await _documentExecuter.ExecuteAsync(executionOptions);

                if (result.Errors?.Count > 0)
                {
                    _logger.LogError("GraphQL errors: {0}", result.Errors);
                    return BadRequest(result);
                }

                _logger.LogDebug("GraphQL execution result: {result}", JsonConvert.SerializeObject(result.Data));
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Document exexuter exception", ex);
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> PostAsync([FromBody] GraphQLQuery query)
        {
            var inputs = query.Variables.ToInputs();
            var queryToExecute = query.Query;

            //if (!string.IsNullOrWhiteSpace(query.NamedQuery))
            //{
            //    queryToExecute = _namedQueries[query.NamedQuery];
            //}

            var result = await _documentExecuter.ExecuteAsync(_ =>
            {
                _.Schema = _schema;
                _.Query = queryToExecute;
                _.OperationName = query.OperationName;
                _.Inputs = inputs;

                _.ExposeExceptions = true;

                _.ComplexityConfiguration = new ComplexityConfiguration { MaxDepth = 15 };
                _.FieldMiddleware.Use<InstrumentFieldsMiddleware>();

            });


            return Json(result);
        }
    }


 public class GraphQLQuery
{
    public string OperationName { get; set; }
    public string NamedQuery { get; set; }
    public string Query { get; set; }
    public string Variables { get; set; }
}

public static class ObjectGraphTypeExtensions
    {
        public static void Field(
            this IObjectGraphType obj,
            string name,
            IGraphType type,
            Func<ResolveFieldContext, object> resolve = null)
        {
            var field = new FieldType();
            field.Name = name;
            field.ResolvedType = type;
            field.Resolver = resolve != null ? new FuncFieldResolver<object>(resolve) : null;
            obj.AddField(field);
        }
    }
}
