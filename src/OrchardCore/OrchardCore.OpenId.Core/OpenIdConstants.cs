namespace OrchardCore.OpenId;

public static class OpenIdConstants
{
    public static class Claims
    {
        public const string EntityType = "oc:entyp";
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

    // Breadcrumb trail names.
    public const string ApplicationsList = "OpenIdApplications";
    public const string ApplicationsCreate = "OpenIdApplicationsCreate";
    public const string ApplicationsEdit = "OpenIdApplicationsEdit";
    public const string ScopesList = "OpenIdScopes";
    public const string ScopesCreate = "OpenIdScopesCreate";
    public const string ScopesEdit = "OpenIdScopesEdit";
    public const string ServerConfiguration = "OpenIdServerConfiguration";
    public const string ValidationConfiguration = "OpenIdValidationConfiguration";
}
