namespace OrchardCore.Infrastructure;

public static class MediaTypeNamesExtended
{
    public static class Application
    {
        public const string JsonVendeorPrefix = "application/vnd.api+json";

        public const string GraphiQL = "application/graphiql";

        public const string JsonLinkedData = "application/ld+json";

        public const string XmlRss = "application/rss+xml";

        public const string XmlRsd = "application/rsd+xml";

        public const string XmlWindowsLiveWriter = "application/wlwmanifest+xml";
    }

    public static class Text
    {
        public const string SQL = "text/x-sql";

        public const string SQLite = "text/x-sqlite";

        public const string SqlServer = "text/x-mssql";

        public const string MySQL = "text/x-mysql";

        public const string PostgreSQL = "text/x-pgsql";
    }
}
