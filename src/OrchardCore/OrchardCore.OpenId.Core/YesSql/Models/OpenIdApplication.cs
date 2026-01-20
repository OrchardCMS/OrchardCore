using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json.Nodes;

namespace OrchardCore.OpenId.YesSql.Models;

public class OpenIdApplication
{
    /// <summary>
    /// The name of the collection that is used for this type.
    /// </summary>
    public const string OpenIdCollection = "OpenId";

    /// <summary>
    /// Gets or sets the unique identifier associated with the current application.
    /// </summary>
    public string ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the application type associated with the current application.
    /// </summary>
    public string ApplicationType { get; set; }

    /// <summary>
    /// Gets or sets the client identifier associated with the current application.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret associated with the current application.
    /// Note: depending on the application manager used to create this instance,
    /// this property may be hashed or encrypted for security reasons.
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the consent type associated with the current application.
    /// </summary>
    public string ConsentType { get; set; }

    /// <summary>
    /// Gets or sets the display name associated with the current application.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the localized display names associated with the application.
    /// </summary>
    public ImmutableDictionary<CultureInfo, string> DisplayNames { get; set; }
        = ImmutableDictionary.Create<CultureInfo, string>();

    /// <summary>
    /// Gets or sets the physical identifier associated with the current application.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the JSON Web Key Set associated with the current application.
    /// </summary>
    // TODO: change the property type to JsonWebKeySet after migrating to System.Text.Json.
    public JsonObject JsonWebKeySet { get; set; }

    /// <summary>
    /// Gets or sets the permissions associated with the application.
    /// </summary>
    public ImmutableArray<string> Permissions { get; set; } = [];

    /// <summary>
    /// Gets the logout callback URLs associated with the current application.
    /// </summary>
    public ImmutableArray<string> PostLogoutRedirectUris { get; set; } = [];

    /// <summary>
    /// Gets or sets the additional properties associated with the current application.
    /// </summary>
    public JsonObject Properties { get; set; }

    /// <summary>
    /// Gets or sets the callback URLs associated with the current application.
    /// </summary>
    public ImmutableArray<string> RedirectUris { get; set; } = [];

    /// <summary>
    /// Gets or sets the requirements associated with the current application.
    /// </summary>
    public ImmutableArray<string> Requirements { get; set; } = [];

    /// <summary>
    /// Gets or sets the roles associated with the application.
    /// </summary>
    public ImmutableArray<string> Roles { get; set; } = [];

    /// <summary>
    /// Gets or sets the settings associated with the application.
    /// </summary>
    public ImmutableDictionary<string, string> Settings { get; set; }
        = ImmutableDictionary.Create<string, string>();

    /// <summary>
    /// Gets or sets the client type associated with the current application.
    /// </summary>
    public string Type { get; set; }
}
