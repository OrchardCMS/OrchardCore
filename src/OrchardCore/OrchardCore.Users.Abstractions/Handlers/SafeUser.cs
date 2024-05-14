using System.Collections.Generic;
using System.Text.Json.Nodes;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Handlers;

/// <summary>
/// Indicates a user object that does not contain sensitive information about the user.
/// </summary>
public class SafeUser : IUser
{
    public string UserName { get; set; }

    public IEnumerable<string> UserRoles { get; set; }

    public IEnumerable<UserClaim> UserClaims { get; set; }

    public JsonObject UserProperties { get; set; }
}
