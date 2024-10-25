using System.Text.Json.Nodes;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Handlers;

/// <summary>
/// Represents a <see cref="UserContextBase"/> for updated user roles.
/// </summary>
public class UpdateUserContext : UserContextBase
{
    /// <summary>
    /// Creates a new instance of <see cref="UpdateUserContext"/>.
    /// </summary>
    /// <param name="user">The <see cref="IUser"/>.</param>
    /// <param name="loginProvider">The login provider.</param>
    /// <param name="externalClaims">The user claims.</param>
    /// <param name="userProperties">The user properties.</param>
    public UpdateUserContext(IUser user, string loginProvider, IEnumerable<SerializableClaim> externalClaims, JsonObject userProperties) : base(user)
    {
        ExternalClaims = externalClaims.AsEnumerable();
        LoginProvider = loginProvider;
        UserProperties = userProperties.DeepClone() as JsonObject;
    }

    /// <summary>
    /// Gets the login provider.
    /// </summary>
    public string LoginProvider { get; }

    /// <summary>
    /// Gets a list of external claims.
    /// </summary>
    public IEnumerable<SerializableClaim> ExternalClaims { get; }

    /// <summary>
    /// Gets the user properties.
    /// </summary>
    public JsonObject UserProperties { get; }

    /// <summary>
    /// Gets the user's roles.
    /// </summary>
    public IEnumerable<string> UserRoles { get; set; } = [];

    /// <summary>
    /// Gets the user's roles.
    /// </summary>
    public IEnumerable<UserClaim> UserClaims { get; set; } = [];

    /// <summary>
    /// Gets the roles to be added to the user roles.
    /// </summary>
    public List<string> RolesToAdd { get; init; } = [];

    /// <summary>
    /// Gets the roles to be removed from the user roles.
    /// </summary>
    public List<string> RolesToRemove { get; init; } = [];

    /// <summary>
    /// Gets the claims to be added from the user claims.
    /// </summary>
    public List<UserClaim> ClaimsToUpdate { get; init; } = [];

    /// <summary>
    /// Gets the claims to be removed from the user claims.
    /// </summary>
    public List<UserClaim> ClaimsToRemove { get; init; } = [];

    /// <summary>
    /// Gets the user properties to update the user
    /// </summary>
    public JsonObject PropertiesToUpdate { get; set; }
}
