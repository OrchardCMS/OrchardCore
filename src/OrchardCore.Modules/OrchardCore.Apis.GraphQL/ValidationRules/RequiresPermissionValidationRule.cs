using GraphQL;
using GraphQL.Types;
using GraphQL.Validation;
using GraphQLParser.AST;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Apis.GraphQL.ValidationRules;

public class RequiresPermissionValidationRule : IValidationRule
{
    public const string ErrorCode = "Unauthorized";

    private readonly IAuthorizationService _authorizationService;

    protected readonly IStringLocalizer S;

    public RequiresPermissionValidationRule(
        IAuthorizationService authorizationService,
        IStringLocalizer<RequiresPermissionValidationRule> localizer)
    {
        _authorizationService = authorizationService;
        S = localizer;
    }

    public async ValueTask<INodeVisitor> ValidateAsync(ValidationContext validationContext)
    {
        // shouldn't we access UserContext from validation-context inside MatchingNodeVisitor actions?
        var userContext = (GraphQLUserContext)validationContext.UserContext;

        return await Task.FromResult(new NodeVisitors(
            new MatchingNodeVisitor<GraphQLOperationDefinition>(async (operationDefinition, validationContext) =>
            {
                await AuthorizeOperationAsync(operationDefinition, validationContext, userContext, operationDefinition.Operation, operationDefinition?.Name?.StringValue);
            }),
            new MatchingNodeVisitor<GraphQLObjectField>(async (objectFieldAst, validationContext) =>
            {
                if (validationContext.TypeInfo.GetArgument()?.ResolvedType.GetNamedType() is IComplexGraphType argumentType)
                {
                    var fieldType = argumentType.GetField(objectFieldAst.Name);
                    await AuthorizeNodePermissionAsync(objectFieldAst, fieldType, validationContext, userContext);
                }
            }),
            new MatchingNodeVisitor<GraphQLField>(async (fieldAst, validationContext) =>
            {
                var fieldDef = validationContext.TypeInfo.GetFieldDef();

                if (fieldDef == null)
                {
                    return;
                }

                // check target field
                await AuthorizeNodePermissionAsync(fieldAst, fieldDef, validationContext, userContext);
                // check returned graph type
                //   AuthorizeNodePermissionAsync(fieldAst, fieldDef.ResolvedType.GetNamedType(), validationContext, userContext).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this
            })
        ));
    }

    private async Task AuthorizeOperationAsync(ASTNode node, ValidationContext validationContext, GraphQLUserContext userContext, OperationType? operationType, string operationName)
    {
        if (operationType == OperationType.Mutation && !(await _authorizationService.AuthorizeAsync(userContext.User, CommonPermissions.ExecuteGraphQLMutations)))
        {
            validationContext.ReportError(new ValidationError(
                validationContext.Document.Source,
                ErrorCode,
                S["Authorization is required to access {0}.", operationName],
                node));
        }
    }

    private async Task AuthorizeNodePermissionAsync(ASTNode node, FieldType fieldType, ValidationContext validationContext, GraphQLUserContext userContext)
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

    private void AddPermissionValidationError(ValidationContext validationContext, ASTNode node, string nodeName)
    {
        validationContext.ReportError(new ValidationError(
                   validationContext.Document.Source,
                   ErrorCode,
                   S["Authorization is required to access the node. {0}", nodeName],
                   node));
    }
}
