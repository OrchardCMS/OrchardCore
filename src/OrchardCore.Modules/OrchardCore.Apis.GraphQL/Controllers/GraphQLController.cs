using System.Threading.Tasks;
using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.Types;
using GraphQL.Validation.Complexity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Apis.Controllers
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
        public Task<IActionResult> GetAsync(string query)
        {
            return PostAsync(new GraphQLQuery { Query = query, Variables = "" });
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
}