using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Security
{
    public static class BuiltInRole
    {
        [Display(Name = nameof(Administrator))]
        public const string Administrator = "Administrator";

        public const string Anonymous = "Anonymous";

        public const string Authenticated = "Authenticated";

        public const string Author = "Author";

        public const string Contributor = "Contributor";

        public const string Editor = "Editor";

        public const string Moderator = "Moderator";
    }
}
