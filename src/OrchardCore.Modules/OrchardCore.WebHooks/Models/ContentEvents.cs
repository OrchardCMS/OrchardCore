using System.Collections.Generic;

namespace OrchardCore.WebHooks.Models
{
    public class ContentEvents
    {
        public static string Created = "created";

        public static string Updated = "updated";

        public static string Removed = "removed";

        public static string Published = "published";

        public static string Unpublished = "unpublished";

        public static IEnumerable<string> AllEvents => new[]
        {
            Created,
            Updated,
            Removed,
            Published,
            Unpublished
        };
    }
}