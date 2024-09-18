using System.Text.Json.Nodes;
using System.Text.Json.Settings;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;

namespace OrchardCore.Users;

public static class UserManagerHelper
{
    private static readonly JsonMergeSettings _jsonMergeSettings = new()
    {
        MergeArrayHandling = MergeArrayHandling.Replace,
        MergeNullValueHandling = MergeNullValueHandling.Merge,
    };

    public static async Task<bool> UpdateUserPropertiesAsync(UserManager<IUser> userManager, User user, UpdateUserContext context)
    {
        await userManager.AddToRolesAsync(user, context.RolesToAdd.Distinct());
        await userManager.RemoveFromRolesAsync(user, context.RolesToRemove.Distinct());

        var userNeedUpdate = false;
        if (context.PropertiesToUpdate != null)
        {
            var currentProperties = user.Properties.DeepClone();
            user.Properties.Merge(context.PropertiesToUpdate, _jsonMergeSettings);
            userNeedUpdate = !JsonNode.DeepEquals(currentProperties, user.Properties);
        }

        var currentClaims = user.UserClaims
            .Where(x => !string.IsNullOrEmpty(x.ClaimType))
            .DistinctBy(x => new { x.ClaimType, x.ClaimValue })
            .ToList();

        var claimsChanged = false;
        if (context.ClaimsToRemove?.Count > 0)
        {
            var claimsToRemove = context.ClaimsToRemove.ToHashSet();
            foreach (var item in claimsToRemove)
            {
                var exists = currentClaims.FirstOrDefault(claim => claim.ClaimType == item.ClaimType && claim.ClaimValue == item.ClaimValue);
                if (exists is not null)
                {
                    currentClaims.Remove(exists);
                    claimsChanged = true;
                }
            }
        }

        if (context.ClaimsToUpdate?.Count > 0)
        {
            foreach (var item in context.ClaimsToUpdate)
            {
                var existing = currentClaims.FirstOrDefault(claim => claim.ClaimType == item.ClaimType && claim.ClaimValue == item.ClaimValue);
                if (existing is null)
                {
                    currentClaims.Add(item);
                    claimsChanged = true;
                }
            }
        }

        if (claimsChanged)
        {
            user.UserClaims = currentClaims;
            userNeedUpdate = true;
        }

        return userNeedUpdate;
    }
}
