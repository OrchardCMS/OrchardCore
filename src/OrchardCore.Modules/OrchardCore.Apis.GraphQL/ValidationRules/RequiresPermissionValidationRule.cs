using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Apis.GraphQL.ValidationRules
{
    public class RequiresPermissionValidationRule : IValidationRule
    {
        public static readonly string ErrorCode = "Unauthorized";
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer<RequiresPermissionValidationRule> S;

        public RequiresPermissionValidationRule(IAuthorizationService authorizationService, IStringLocalizer<RequiresPermissionValidationRule> s)
        {
            _authorizationService = authorizationService;
            S = s;
        }

        public async Task<INodeVisitor> ValidateAsync(ValidationContext validationContext)
        {
            var operationType = OperationType.Query;
            var userContext = (GraphQLUserContext)validationContext.UserContext;

            return await Task.FromResult(new NodeVisitors(
                new MatchingNodeVisitor<Operation>(async (astType, validationContext) =>
                {
                    operationType = astType.OperationType;
                    await AuthorizeOperationAsync(astType, validationContext, userContext, operationType, astType.Name);
                }),
                new MatchingNodeVisitor<ObjectField>(async (objectFieldAst, validationContext) =>
                {
                    if (validationContext.TypeInfo.GetArgument()?.ResolvedType.GetNamedType() is IComplexGraphType argumentType)
                    {
                        var fieldType = argumentType.GetField(objectFieldAst.Name);
                        await AuthorizeNodePermissionAsync(objectFieldAst, fieldType, validationContext, userContext);
                    }
                }),
                new MatchingNodeVisitor<Field>(async (fieldAst, validationContext) =>
                {
                    var fieldDef = validationContext.TypeInfo.GetFieldDef();

                    if (fieldDef == null)
                        return;

                    // check target field
                    await AuthorizeNodePermissionAsync(fieldAst, fieldDef, validationContext, userContext);
                    // check returned graph type
                    //   AuthorizeNodePermissionAsync(fieldAst, fieldDef.ResolvedType.GetNamedType(), validationContext, userContext).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this
                })
            ));
        }


        //private static bool Authorize(IProvideMetadata type, GraphQLUserContext context)
        //{
        //    //var authorizationManager = context.ServiceProvider.GetService<IAuthorizationService>();

        //    //// awaitable IValidationRule in graphql dotnet is coming soon:
        //    //// https://github.com/graphql-dotnet/graphql-dotnet/issues/1140
        //    //return type.GetPermissions().All(x => authorizationManager.AuthorizeAsync(context.User, x.Permission, x.Resource).GetAwaiter().GetResult());


        //}

        private async Task AuthorizeOperationAsync(INode node, ValidationContext validationContext, GraphQLUserContext userContext, OperationType? operationType, string operationName)
        {
            if (operationType == OperationType.Mutation && !(await _authorizationService.AuthorizeAsync(userContext.User, Permissions.ExecuteGraphQLMutations)))
            {
                validationContext.ReportError(new ValidationError(
                    validationContext.Document.OriginalQuery,
                    ErrorCode,
                    S["Authorization is required to access {0}.", operationName],
                    node));
            }
        }

        private async Task AuthorizeNodePermissionAsync(INode node, IFieldType fieldType, ValidationContext validationContext, GraphQLUserContext userContext)
        {
            if (!fieldType.HasPermissions()) {
                return;
            }

            var permissions = fieldType?.GetPermissions() ?? Enumerable.Empty<GraphQLPermissionContext>();

            if (permissions.Count() == 1)
            {
                var permission = permissions.First();
                // small optimization for the single policy - no 'new List<>()', no 'await Task.WhenAll()'
                var authorizationResult = await _authorizationService.AuthorizeAsync(userContext.User, permission.Permission, permission.Resource);
                if (!authorizationResult)
                    AddPermissionValidationError(validationContext, node, fieldType.Name);
            }
            else 
            {
                var tasks = new List<Task<bool>>(permissions.Count());

                foreach (var permission in permissions)
                {
                    var task = _authorizationService.AuthorizeAsync(userContext.User, permission.Permission, permission.Resource);
                    tasks.Add(task);
                }

                var authorizationResults = await Task.WhenAll(tasks);

                if (authorizationResults.Any(x => !true))
                {
                    AddPermissionValidationError(validationContext, node, fieldType.Name);
                }
            }
        }

        private void AddPermissionValidationError(ValidationContext validationContext, INode node, string nodeName)
        {
            validationContext.ReportError(new ValidationError(
                       validationContext.Document.OriginalQuery,
                       ErrorCode,
                       S["Authorization is required to access the node. {0}", nodeName],
                       node));
        }
    }
}
