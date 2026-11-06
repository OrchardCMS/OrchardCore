namespace OrchardCore.OpenId;

public static class OpenIdConstants
{
    public static class Claims
    {
        public const string EntityType = "oc:entyp";
    }

    public static class Client
    {
        /// <summary>
        /// The provider name, also the authentication scheme, of the client registration configured in the
        /// OpenID Connect client settings of the site.
        /// </summary>
        public const string DefaultProviderName = "OpenIdConnect";

        /// <summary>
        /// The default path, relative to the tenant, of the redirection endpoint of the client registration
        /// configured in the OpenID Connect client settings of the site.
        /// </summary>
        public const string DefaultCallbackPath = "signin-oidc";

        /// <summary>
        /// The default path, relative to the tenant, of the post-logout redirection endpoint of the client
        /// registration configured in the OpenID Connect client settings of the site.
        /// </summary>
        public const string DefaultSignedOutCallbackPath = "signout-callback-oidc";

        /// <summary>
        /// The key of the property of an OpenIddict client registration that holds the Orchard Core
        /// settings the registration was created from.
        /// </summary>
        public const string SettingsProperty = "OrchardCore.OpenId.ClientSettings";

        /// <summary>
        /// The response types a client registration can use.
        /// </summary>
        public static class ResponseTypes
        {
            public const string None = "none";
            public const string Code = "code";
            public const string IdToken = "id_token";
            public const string CodeIdToken = "code id_token";
            public const string CodeToken = "code token";
            public const string IdTokenToken = "id_token token";
            public const string CodeIdTokenToken = "code id_token token";
        }
    }

    public static class EntityTypes
    {
        public const string Application = "application";
        public const string User = "user";
    }

    public static class Features
    {
        public const string Client = "OrchardCore.OpenId.Client";
        public const string Core = "OrchardCore.OpenId";
        public const string Management = "OrchardCore.OpenId.Management";

        /// <summary>
        /// The optional feature that loads OpenID Connect server keys from secrets.
        /// </summary>
        public const string Secrets = "OrchardCore.OpenId.Secrets";

        public const string Server = "OrchardCore.OpenId.Server";
        public const string Validation = "OrchardCore.OpenId.Validation";
    }

    public static class Prefixes
    {
        public const string Tenant = "oct:";
    }

    public static class Properties
    {
        public const string Roles = "Roles";
    }

    public static class SecretNames
    {
        public const string ClientSecret = "OpenIdClient.ClientSecret";
        public const string SigningCertificate = "OpenId.SigningCertificate";
        public const string EncryptionCertificate = "OpenId.EncryptionCertificate";
    }
}
