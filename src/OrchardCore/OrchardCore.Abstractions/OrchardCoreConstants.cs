namespace OrchardCore;

public static class OrchardCoreConstants
{
    public const char DataLocalizationSeparator = ':';

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

        /// <summary>
        /// The configuration for authentication should be set up early, prior to any non-security modules.
        /// </summary>
        public const int Authentication = -150;

        /// <summary>
        /// The reverse proxy should always be configured before the 'Authentication' and security initialization logic.
        /// </summary>
        public const int ReverseProxy = Authentication * 2;

        /// <summary>
        /// The CORS module should be registered after the reverse proxy module to ensure that the correct host is used.
        /// </summary>
        public const int Cors = ReverseProxy + 10;

        /// <summary>
        /// The Security module should be registered after the reverse proxy module.
        /// </summary>
        public const int Security = ReverseProxy + 10;

        /// <summary>
        /// When the .UseStaticFiles() is registered and should be early before rate limitting.
        /// </summary>
        public const int StaticFiles = -5;

        /// <summary>
        /// The Media module should be registered after the StaticFiles module to ensure that media assets are served correctly.
        /// </summary>
        public const int Media = Default + StaticFiles;

        /// <summary>
        /// Image cache overrides Media configurations and services. The order number should always be greater than Media module.
        /// </summary>
        public const int ResizedImageCache = Media + 5;

        /// <summary>
        /// Image cache overrides Media configurations and services. The order number should always be greater than Media module.
        /// </summary>
        public const int AzureResizedImageCache = Media + 5;

        /// <summary>
        /// Azure media storage overrides Media configurations and services. The order number should always be greater than Media module.
        /// </summary>
        public const int AzureMediaStorage = Media + 10;

        /// <summary>
        /// The FileProvider module should be registered after the StaticFiles module to ensure that the tenant file provider is used for static assets.
        /// </summary>
        public const int FileProvider = StaticFiles + 10;

        /// <summary>
        /// The RateLimiter module should be registered after the FileProvider module to ensure that the rate limiting policies do not impact assets.
        /// </summary>
        public const int RateLimiter = FileProvider + 5;

        public const int DataProtection = Default;

        /// <summary>
        /// Azure DataProtection will override default data-protection configurations. The order number should always be greater than data protection modules.
        /// </summary>
        public const int AzureDataProtection = DataProtection + 10;

        public const int Autoroute = -100;

        public const int HomeRoute = -150;

        public const int AdminPages = 1000;

        /// <summary>
        /// Services that should always be registered before everything else.
        /// </summary>
        public const int InfrastructureService = int.MinValue + 100;

        /// <summary>
        /// The UrlRewriting module should be registered before any other module that deals with URLs.
        /// </summary>
        public const int UrlRewriting = InfrastructureService + 100;

        /// <summary>
        /// The Workflows content handler should be registered before other content handlers to ensure it processes content events last.
        /// </summary>
        public const int WorkflowsContentHandlers = InfrastructureService + 100;
    }

    public static class DisplayType
    {
        public const string Detail = "Detail";

        public const string Summary = "Summary";

        public const string DetailAdmin = "DetailAdmin";

        public const string SummaryAdmin = "SummaryAdmin";
    }

    public static class Security
    {
        public const string ScriptingEncryptionPurpose = "oc-scripting";
    }
}
