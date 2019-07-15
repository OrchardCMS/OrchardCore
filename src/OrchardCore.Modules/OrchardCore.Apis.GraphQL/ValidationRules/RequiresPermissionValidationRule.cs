using System;
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
        public INodeVisitor Validate(ValidationContext validationContext)
        {
            var context = validationContext.UserContext as GraphQLContext;

            return new EnterLeaveListener(_ =>
            {
                _.Match<Operation>(op =>
                {
           
                    if (op.OperationType == OperationType.Mutation)
                    {
                        var authorizationManager = context.ServiceProvider.GetService<IAuthorizationService>();

                        if (!authorizationManager.AuthorizeAsync(context.User, Permissions.ExecuteGraphQLMutations).GetAwaiter().GetResult())

                        validationContext.ReportError(new ValidationError(
                            validationContext.OriginalQuery,
                            "Forbidden",
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
                    var fieldDef = validationContext.TypeInfo.GetFieldDef();

                    if (RequiresPermissions(fieldDef) && !Authorize(fieldDef, context))
                    {
                        validationContext.ReportError(new ValidationError(
                            validationContext.OriginalQuery,
                            "Forbidden",
                            $"Authorization is required to access this field.",
                            fieldAst));
                    }
                });
            });
        }

        private static bool RequiresPermissions(IProvideMetadata type)
        {
            return type.HasMetadata(PermissionsExtensions.PermissionsKey);
        }

        public static bool Authorize(IProvideMetadata type, GraphQLContext context)
        {
            var authorizationManager = context.ServiceProvider.GetService<IAuthorizationService>();
            var permissions = type.GetMetadata(PermissionsExtensions.PermissionsKey, Enumerable.Empty<Tuple<Permission, object>>());

            // awaitable IValidationRule in graphql dotnet is coming soon:
            // https://github.com/graphql-dotnet/graphql-dotnet/issues/1140
            return permissions.All(x => authorizationManager.AuthorizeAsync(context.User, x.Item1, x.Item2).GetAwaiter().GetResult());
        }
    }
}
