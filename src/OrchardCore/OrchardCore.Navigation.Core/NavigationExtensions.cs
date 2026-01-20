using Microsoft.Extensions.Localization;

namespace OrchardCore.Navigation
{
    public static class NavigationExtensions
    {
        public const string AdminMenuPositionPrefix = "am";
        public const string Separator = "-";

        public static string PrefixPosition(this string position, string sublevel = "", string prefix = AdminMenuPositionPrefix, string separator = Separator)
        {
            return prefix + sublevel + separator + position;
        }

        public static string PrefixPosition(this LocalizedString position, string sublevel = "", string prefix = AdminMenuPositionPrefix, string separator = Separator)
        {
            return position.ToString().PrefixPosition(sublevel, prefix, separator);
        }
    }
}
