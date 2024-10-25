using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
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
        Description = S["Represents a user."];

        Field(u => u.UserId).Description(S["The id of the user."]);
        Field(u => u.UserName).Description(S["The name of the user."]);
        Field(u => u.Email, nullable: true).Description(S["The email of the user."]);
        Field(u => u.PhoneNumber, nullable: true).Description(S["The phone number of the user."]);
    }

    public override void Initialize(ISchema schema)
    {
        // Add custom user settings by reusing previously registered content types with the
        // stereotype "CustomUserSettings".
        foreach (var contentItemType in schema.AdditionalTypeInstances.Where(t => t.Metadata.TryGetValue("Stereotype", out var stereotype) && stereotype as string == "CustomUserSettings"))
        {
            Field(contentItemType.Name, contentItemType)
                .Description(S["Custom user settings of {0}.", contentItemType.Name])
                .ResolveAsync(static async context =>
                {
                    // We don't want to create an empty content item if it does not exist.
                    if (context.Source is User user &&
                        user.Properties.ContainsKey(context.FieldDefinition.ResolvedType.Name))
                    {
                        var customUserSettingsService = context.RequestServices.GetRequiredService<CustomUserSettingsService>();
                        var settingsType = await customUserSettingsService.GetSettingsTypeAsync(context.FieldDefinition.ResolvedType.Name);

                        return await customUserSettingsService.GetSettingsAsync(user, settingsType);
                    }

                    return null;
                });
        }

        base.Initialize(schema);
    }
}
