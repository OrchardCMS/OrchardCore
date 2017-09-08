//using System.Threading.Tasks;
//using GraphQL.Types;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Orchard.ContentManagement;
//using Orchard.Environment.Shell;
//using YesSql;

//namespace Orchard.RestApis.Filters
//{
//    public class GraphqlFilter : IAsyncActionFilter
//    {
//        private readonly ShellSettings _shellSettings;
//        private readonly IApiContentManager _contentManager;

//        private string _tenantPath;

//        private static string ContentType = "application/graphql";

//        public GraphqlFilter(
//            ShellSettings shellSettings,
//            IApiContentManager contentManager)
//        {
//            _shellSettings = shellSettings;
//            _contentManager = contentManager;

//            _tenantPath = "/" + _shellSettings.RequestUrlPrefix;
//        }

//        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
//        {
//            var isValidRequest = context.HttpContext.Request.ContentType == ContentType;

//            var actionExecutedContext = await next();

//            if (!isValidRequest)
//            {
//                return;
//            }

//            var result = actionExecutedContext.Result as ObjectResult;

//            if (result == null)
//            {
//                return;
//            }

//            actionExecutedContext.HttpContext.Response.ContentType = ContentType;

//        }
//    }
//}
