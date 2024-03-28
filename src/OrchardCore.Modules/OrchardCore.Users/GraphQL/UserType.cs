using System;
using System.Linq;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.GraphQL;

public class UserType : ObjectGraphType<User>
{
    protected readonly IStringLocalizer<UserType> S;

    public UserType(IStringLocalizer<UserType> localizer)
    {
        S = localizer;

        Name = "User";
        Description = S["Represents the currently authenticated user."];

        Field(u => u.UserId).Description(S["The id of the user."]);
        Field(u => u.UserName).Description(S["The name of the user."]);
        Field(u => u.Email, nullable: true).Description(S["The email of the user."]);
        Field(u => u.PhoneNumber, nullable: true).Description(S["The phone number of the user."]);
    }

    // Adds a custom user settings field
    internal void AddField(ISchema schema, ContentTypeDefinition typeDefinition)
    {
        var contentItemType = schema.AdditionalTypeInstances.SingleOrDefault(t => t.Name == typeDefinition.Name);

        if (contentItemType == null)
        {
            // This error would indicate that this graph type is build too early.
            throw new InvalidOperationException("ContentTypeDefinition has not been registered in GraphQL");
        }

        Field<ContentItemInterface, ContentItem>(typeDefinition.Name)
           .Type(contentItemType)
           .Description(S["Custom user settings of {0}.", typeDefinition.DisplayName])
           .ResolveAsync(static async context =>
           {
               // We don't want to create an empty content item if it does not exist.
               if (context.Source is User user &&
                   user.Properties.ContainsKey(context.FieldDefinition.ResolvedType.Name))
               {
                   var customUserSettingsService = context.RequestServices!.GetRequiredService<CustomUserSettingsService>();
                   var settingsType = await customUserSettingsService.GetSettingsTypeAsync(context.FieldDefinition.ResolvedType.Name);

                   return await customUserSettingsService.GetSettingsAsync(user, settingsType);
               }

               return null;
           });
    }
}
