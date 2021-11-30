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
        private readonly IStringLocalizer<RequiresPermissionValidationRule> _localizer;

        public RequiresPermissionValidationRule(IAuthorizationService authorizationService, IStringLocalizer<RequiresPermissionValidationRule> localizer)
        {
            _authorizationService = authorizationService;
            _localizer = localizer;
        }

        public Task<INodeVisitor> ValidateAsync(ValidationContext validationContext)
        {
            var graphQLContext = (GraphQLContext)validationContext.UserContext;

            return Task.FromResult((INodeVisitor)new NodeVisitors(
                new MatchingNodeVisitor<Operation>((astType, context) =>
                {
                    if (astType.OperationType == OperationType.Mutation)
                    {
                        if (!_authorizationService.AuthorizeAsync(graphQLContext.User, Permissions.ExecuteGraphQLMutations).GetAwaiter().GetResult())
                        {
                            validationContext.ReportError(new ValidationError(
                                validationContext.Document.OriginalQuery,
                                ErrorCode,
                                _localizer["Authorization is required to access {0}.", astType.Name],
                                astType));
                        }
                    }
                }),
                new MatchingNodeVisitor<ObjectField>((objectFieldAst, context) =>
                {
                    if (context.TypeInfo.GetArgument()?.ResolvedType.GetNamedType() is IComplexGraphType argumentType)
                    {
                        var fieldDef = argumentType.GetField(objectFieldAst.Name);

                        if (fieldDef.HasPermissions() && !Authorize(fieldDef, graphQLContext))
                        {
                            validationContext.ReportError(new ValidationError(
                                validationContext.Document.OriginalQuery,
                                ErrorCode,
                                _localizer["Authorization is required to access the field. {0}", objectFieldAst.Name],
                                objectFieldAst));
                        }
                    }
                }),
                  new MatchingNodeVisitor<Field>((fieldAst, context) =>
                  {
                      var fieldDef = validationContext.TypeInfo.GetFieldDef();

                      if (fieldDef.HasPermissions() && !Authorize(fieldDef, graphQLContext))
                      {
                          validationContext.ReportError(new ValidationError(
                              validationContext.Document.OriginalQuery,
                              ErrorCode,
                              _localizer["Authorization is required to access the field. {0}", fieldAst.Name],
                              fieldAst));
                      }
                  })));
        }

        private bool Authorize(IProvideMetadata type, GraphQLContext context)
        {
            // awaitable IValidationRule in graphql dotnet is coming soon:
            // https://github.com/graphql-dotnet/graphql-dotnet/issues/1140
            return type.GetPermissions().All(x => _authorizationService.AuthorizeAsync(context.User, x.Permission, x.Resource).GetAwaiter().GetResult());
        }
    }
}
