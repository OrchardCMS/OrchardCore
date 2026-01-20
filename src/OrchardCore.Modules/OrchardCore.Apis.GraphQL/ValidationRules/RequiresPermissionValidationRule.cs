using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Apis.GraphQL.ValidationRules
{
    public class RequiresPermissionValidationRule : IValidationRule
    {
        public static readonly string ErrorCode = "Unauthorized";
        private readonly IAuthorizationService _authorizationService;
        protected readonly IStringLocalizer S;

        public RequiresPermissionValidationRule(
            IAuthorizationService authorizationService,
            IStringLocalizer<RequiresPermissionValidationRule> localizer)
        {
            _authorizationService = authorizationService;
            S = localizer;
        }

        public async Task<INodeVisitor> ValidateAsync(ValidationContext validationContext)
        {
            // shouldn't we access UserContext from validationcontext inside MatchingNodeVisitor actions?
            var userContext = (GraphQLUserContext)validationContext.UserContext;

            return await Task.FromResult(new NodeVisitors(
                new MatchingNodeVisitor<Operation>(async (astType, validationContext) =>
                {
                    await AuthorizeOperationAsync(astType, validationContext, userContext, astType.OperationType, astType.Name);
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
            var permissions = fieldType?.GetPermissions();

            if (permissions == null)
            {
                return;
            }

            var totalPermissions = permissions.Count();

            if (totalPermissions == 0)
            {
                return;
            }

            if (totalPermissions == 1)
            {
                var permission = permissions.First();
                // small optimization for the single policy - no 'new List<>()', no 'await Task.WhenAll()'
                if (!await _authorizationService.AuthorizeAsync(userContext.User, permission.Permission, permission.Resource))
                {
                    AddPermissionValidationError(validationContext, node, fieldType.Name);
                }
            }
            else
            {
                var tasks = new List<Task<bool>>();

                foreach (var permission in permissions)
                {
                    tasks.Add(_authorizationService.AuthorizeAsync(userContext.User, permission.Permission, permission.Resource));
                }

                var authorizationResults = await Task.WhenAll(tasks);

                if (authorizationResults.Any(isAuthorized => !isAuthorized))
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
