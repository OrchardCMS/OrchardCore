using System;
using System.Collections.Generic;

namespace OrchardCore.Security
{
    /// <summary>
    /// Defines well-known default and system role names.
    /// </summary>
    public static class RoleNames
    {
        private static readonly HashSet<string> s_systemRoleNames = new(StringComparer.OrdinalIgnoreCase)
        {
            Anonymous,
            Authenticated,
        };

        public const string Administrator = "Administrator";
        public const string Anonymous = "Anonymous";
        public const string Authenticated = "Authenticated";
        public const string Author = "Author";
        public const string Contributor = "Contributor";
        public const string Editor = "Editor";
        public const string Moderator = "Moderator";

        public static IEnumerable<string> SystemRoles => s_systemRoleNames;

        public static bool IsSystemRole(string roleName) => s_systemRoleNames.Contains(roleName);
    }
}
