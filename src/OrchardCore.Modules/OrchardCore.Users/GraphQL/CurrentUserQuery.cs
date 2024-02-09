using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.GraphQL;

/// <summary>
/// Registers the current user including its custom user settings as a query.
/// </summary>
internal class CurrentUserQuery : ISchemaBuilder
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptions<GraphQLContentOptions> _contentOptionsAccessor;
    protected readonly IStringLocalizer S;

    public CurrentUserQuery(
        IHttpContextAccessor httpContextAccessor,
        IOptions<GraphQLContentOptions> contentOptionsAccessor,
        IStringLocalizer<CurrentUserQuery> localizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _contentOptionsAccessor = contentOptionsAccessor;
        S = localizer;
    }

    public async Task BuildAsync(ISchema schema)
    {
        // Build a user type that includes all custom user settings.
        var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
        var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();
        var contentTypes = await contentDefinitionManager.ListTypeDefinitionsAsync();

        var userType = serviceProvider.GetRequiredService<UserType>();

        // Note: The content types are already added to GraphQL by the ContentTypeQuery. Just add them to the user type.
        foreach (var typeDefinition in contentTypes.Where(t => t.StereotypeEquals("CustomUserSettings")))
        {
            // Skip hidden types
            if (_contentOptionsAccessor.Value.ShouldHide(typeDefinition))
            {
                continue;
            }

            userType.AddField(schema, typeDefinition);
        }

        var currentUserField = new FieldType
        {
            Name = "me",
            Description = S["Gets the currently authenticated user."],
            ResolvedType = userType,
            Resolver = new FuncFieldResolver<User>(async context =>
            {
                var userService = context.RequestServices!.GetRequiredService<IUserService>();
                var user = await userService.GetAuthenticatedUserAsync(((GraphQLUserContext)context.UserContext).User);

                return user as User;
            }),
        };

        schema.Query.AddField(currentUserField);
    }

    public Task<string> GetIdentifierAsync()
    {
        var contentDefinitionManager = _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<IContentDefinitionManager>();
        return contentDefinitionManager.GetIdentifierAsync();
    }
}
