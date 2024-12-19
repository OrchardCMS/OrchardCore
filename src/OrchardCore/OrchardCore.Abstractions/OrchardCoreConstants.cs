namespace OrchardCore;

public static class OrchardCoreConstants
{
    public static class Shell
    {
        public const string TenantsFileName = "tenants.json";
    }

    public static class Configuration
    {
        public const string ApplicationSettingsFileName = "appsettings.json";
    }

    public static class Roles
    {
        public const string Administrator = "Administrator";

        public const string Editor = "Editor";

        public const string Moderator = "Moderator";

        public const string Author = "Author";

        public const string Contributor = "Contributor";

        public const string Authenticated = "Authenticated";

        public const string Anonymous = "Anonymous";
    }

    public static class ConfigureOrder
    {
        public const int Default = 0;

        // The configuration for authentication should be set up early, prior to any non-security modules.
        public const int Authentication = -150;

        // The reverse proxy should always be configured before the 'Authentication' and security initialization logic.
        public const int ReverseProxy = Authentication * 2;

        // The CORS module should be registered after the reverse proxy module to ensure that the correct host is used.
        public const int Cors = ReverseProxy + 10;

        // The Security module should be registered after the reverse proxy module.
        public const int Security = ReverseProxy + 10;

        public const int Media = Default;

        // Image cache overrides Media configurations and services.
        // The order number should always be greater than Media module. 
        public const int ImageSharpCache = Media + 5;

        // Image cache overrides Media configurations and services.
        // The order number should always be greater than Media module. 
        public const int AzureImageSharpCache = Media + 5;

        // Azure media storage overrides Media configurations and services.
        // The order number should always be greater than Media module.
        public const int AzureMediaStorage = Media + 10;

        public const int DataProtection = Default;

        // Azure DataProtection will override default data-protection configurations.
        // The order number should always be greater than data protection modules. 
        public const int AzureDataProtection = DataProtection + 10;

        public const int Autoroute = -100;

        public const int HomeRoute = -150;

        public const int AdminPages = 1000;

        // Services that should always be registered before everything else.
        public const int InfrastructureService = int.MinValue + 100;

        // The UrlRewriting module should be registered before any other module that deals with URLs.
        public const int UrlRewriting = InfrastructureService + 100;
    }
}
