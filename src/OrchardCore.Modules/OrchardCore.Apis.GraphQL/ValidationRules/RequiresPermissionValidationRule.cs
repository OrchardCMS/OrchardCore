using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL.ValidationRules
{
    public class RequiresPermissionValidationRule : IValidationRule
    {
        public INodeVisitor Validate(ValidationContext context)
        {
            var userContext = context.UserContext as GraphQLContext;
            var authenticated = userContext.User?.Identity?.IsAuthenticated ?? false;

            return new EnterLeaveListener(_ =>
            {
                _.Match<Operation>(op =>
                {
                    if (op.OperationType == OperationType.Mutation && !authenticated)
                    {
                        context.ReportError(new ValidationError(
                            context.OriginalQuery,
                            "auth-required",
                            $"Authorization is required to access {op.Name}.",
                            op));
                    }
                });

                // this could leak info about hidden fields in error messages
                // it would be better to implement a filter on the schema so it
                // acts as if they just don't exist vs. an auth denied error
                // - filtering the schema is not currently supported
                _.Match<Field>(fieldAst =>
                {
                    var fieldDef = context.TypeInfo.GetFieldDef();

                    if (RequiresPermissions(fieldDef) && (!authenticated || Authorize(fieldDef, userContext)))
                    {
                        context.ReportError(new ValidationError(
                            context.OriginalQuery,
                            "auth-required",
                            $"You are not authorized to run this query.",
                            fieldAst));
                    }
                });
            });
        }

        private static bool RequiresPermissions(IProvideMetadata type)
        {
            return type.GetMetadata(PermissionsExtensions.PermissionsKey, Enumerable.Empty<Permission>()).Any();
        }

        public static bool Authorize(IProvideMetadata type, GraphQLContext context)
        {
            var authorizationManager = context.ServiceProvider.GetService<IAuthorizationService>();
            var permissions = type.GetMetadata(PermissionsExtensions.PermissionsKey, Enumerable.Empty<Permission>());

            // awaitable IValidationRule in graphql dotnet is coming soon:
            // https://github.com/graphql-dotnet/graphql-dotnet/issues/1140
            return permissions.All(x => authorizationManager.AuthorizeAsync(context.User, x).GetAwaiter().GetResult());
        }
    }
}
