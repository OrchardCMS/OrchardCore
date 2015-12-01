using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.UI
{
    public class FlatPositionComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y)
                return 0;

            // null == "before; "" == "0"
            x = x == null
                ? "before"
                : x.Trim().Length == 0 ? "0" : x.Trim(':').TrimEnd('.'); // ':' is _sometimes_ used as a partition identifier
            y = y == null
                ? "before"
                : y.Trim().Length == 0 ? "0" : y.Trim(':').TrimEnd('.');

            var xParts = x.Split(new[] { '.', ':' });
            var yParts = y.Split(new[] { '.', ':' });
            for (var i = 0; i < xParts.Count(); i++)
            {
                if (yParts.Length < i + 1) // x is further defined meaning it comes after y (e.g. x == 1.2.3 and y == 1.2)
                    return 1;

                int xPos;
                int yPos;
                var xPart = string.IsNullOrWhiteSpace(xParts[i]) ? "before" : xParts[i];
                var yPart = string.IsNullOrWhiteSpace(yParts[i]) ? "before" : yParts[i];

                xPart = NormalizeKnownPartitions(xPart);
                yPart = NormalizeKnownPartitions(yPart);

                var xIsInt = int.TryParse(xPart, out xPos);
                var yIsInt = int.TryParse(yPart, out yPos);

                if (!xIsInt && !yIsInt)
                    return string.Compare(string.Join(".", xParts), string.Join(".", yParts), StringComparison.OrdinalIgnoreCase);
                if (!xIsInt || (yIsInt && xPos > yPos)) // non-int after int or greater x pos than y pos (which is an int)
                    return 1;
                if (!yIsInt || xPos < yPos)
                    return -1;
            }

            if (xParts.Length < yParts.Length) // all things being equal y might be further defined than x (e.g. x == 1.2 and y == 1.2.3)
                return -1;

            return 0;
        }

        private static string NormalizeKnownPartitions(string partition)
        {
            if (partition.Length < 5) // known partitions are long
                return partition;

            if (string.Compare(partition, "before", StringComparison.OrdinalIgnoreCase) == 0)
                return "-9999";
            if (string.Compare(partition, "after", StringComparison.OrdinalIgnoreCase) == 0)
                return "9999";

            return partition;
        }
    }
}