using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Descriptors
{
    public class PlacementInfo
    {
        private static readonly char[] Delimiters = { ':', '#', '@' };

        public string Location { get; set; }
        public string Source { get; set; }
        public string ShapeType { get; set; }
        public string DefaultPosition { get; set; }
        public AlternatesCollection Alternates { get; set; }
        public AlternatesCollection Wrappers { get; set; }

        public bool IsLayoutZone()
        {
            return Location.StartsWith('/');
        }

        /// <summary>
        /// Returns the list of zone names.
        /// e.g., <code>Content.Metadata:1</code> will return 'Content', 'Metadata'
        /// </summary>
        /// <returns></returns>
        public string[] GetZones()
        {
            string zones;
            var location = Location;

            // Strip the Layout marker
            if (IsLayoutZone())
            {
                location = location.Substring(1);
            }

            var firstDelimiter = location.IndexOfAny(Delimiters);
            if (firstDelimiter == -1)
            {
                zones = location;
            }
            else
            {
                zones = location.Substring(0, firstDelimiter);
            }

            return zones.Split('.');
        }

        public string GetPosition()
        {
            var contentDelimiter = Location.IndexOf(':');
            if (contentDelimiter == -1)
            {
                return DefaultPosition ?? "";
            }

            var secondDelimiter = Location.IndexOfAny(Delimiters, contentDelimiter + 1);
            if (secondDelimiter == -1)
            {
                return Location.Substring(contentDelimiter + 1);
            }

            return Location.Substring(contentDelimiter + 1, secondDelimiter - contentDelimiter - 1);
        }

        public string GetTab()
        {
            var tabDelimiter = Location.IndexOf('#');
            if (tabDelimiter == -1)
            {
                return "";
            }

            var nextDelimiter = Location.IndexOfAny(Delimiters, tabDelimiter + 1);
            if (nextDelimiter == -1)
            {
                return Location.Substring(tabDelimiter + 1);
            }

            return Location.Substring(tabDelimiter + 1, nextDelimiter - tabDelimiter - 1);
        }

        /// <summary>
        /// Extracts the group information from a location string, or <c>null</c> if it is not present.
        /// e.g., Content:12@search
        /// </summary>
        public string GetGroup()
        {
            var groupDelimiter = Location.IndexOf('@');
            if (groupDelimiter == -1)
            {
                return null;
            }

            var nextDelimiter = Location.IndexOfAny(Delimiters, groupDelimiter + 1);
            if (nextDelimiter == -1)
            {
                return Location.Substring(groupDelimiter + 1);
            }

            return Location.Substring(groupDelimiter + 1, nextDelimiter - groupDelimiter - 1);
        }
    }
}
