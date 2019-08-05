using System.Linq;
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
                        {
                            var localizer = context.ServiceProvider.GetService<IStringLocalizer<RequiresPermissionValidationRule>>();

                            validationContext.ReportError(new ValidationError(
                                validationContext.OriginalQuery,
                                localizer["Forbidden"],
                                localizer[$"Authorization is required to access {op.Name}."],
                                op));
                        }
                    }
                });

                _.Match<Field>(fieldAst =>
                {
                    var fieldDef = validationContext.TypeInfo.GetFieldDef();

                    if (fieldDef.HasPermissions() && !Authorize(fieldDef, context))
                    {
                        var localizer = context.ServiceProvider.GetService<IStringLocalizer<RequiresPermissionValidationRule>>();

                        validationContext.ReportError(new ValidationError(
                            validationContext.OriginalQuery,
                            localizer["Forbidden"],
                            localizer["Authorization is required to access this field."],
                            fieldAst));
                    }
                });
            });
        }

        private static bool Authorize(IProvideMetadata type, GraphQLContext context)
        {
            var authorizationManager = context.ServiceProvider.GetService<IAuthorizationService>();

            // awaitable IValidationRule in graphql dotnet is coming soon:
            // https://github.com/graphql-dotnet/graphql-dotnet/issues/1140
            return type.GetPermissions().All(x => authorizationManager.AuthorizeAsync(context.User, x.Permission, x.Resource).GetAwaiter().GetResult());
        }
    }
}
