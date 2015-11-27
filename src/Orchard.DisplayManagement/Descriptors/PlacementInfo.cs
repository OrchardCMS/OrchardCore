using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Descriptors
{
    public class PlacementInfo
    {
        private static readonly char[] Delimiters = { ':', '#', '@' };

        public PlacementInfo()
        {
            Alternates = Enumerable.Empty<string>();
            Wrappers = Enumerable.Empty<string>();
        }

        public string Location { get; set; }
        public string Source { get; set; }
        public string ShapeType { get; set; }
        public IEnumerable<string> Alternates { get; set; }
        public IEnumerable<string> Wrappers { get; set; }

        public string GetZone()
        {
            var firstDelimiter = Location.IndexOfAny(Delimiters);
            if (firstDelimiter == -1)
            {
                return Location.TrimStart('/');
            }

            return Location.Substring(0, firstDelimiter).TrimStart('/');
        }

        public string GetPosition()
        {
            var contentDelimiter = Location.IndexOf(':');
            if (contentDelimiter == -1)
            {
                return "";
            }

            var secondDelimiter = Location.IndexOfAny(Delimiters, contentDelimiter + 1);
            if (secondDelimiter == -1)
            {
                return Location.Substring(contentDelimiter + 1);
            }

            return Location.Substring(contentDelimiter + 1, secondDelimiter - contentDelimiter - 1);
        }

        public bool IsLayoutZone()
        {
            return Location.StartsWith("/");
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

        public string GetGroup()
        {
            var groupDelimiter = Location.IndexOf('@');
            if (groupDelimiter == -1)
            {
                return "";
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