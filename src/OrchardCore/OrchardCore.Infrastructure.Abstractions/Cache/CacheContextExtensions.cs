namespace OrchardCore.Environment.Cache;

public static class CacheContextExtensions
{
    /// <summary>
    /// Whether the contexts of a cache context contain the given context, ignoring the case.
    /// </summary>
    public static bool ContainsContext(this IEnumerable<string> contexts, string context)
    {
        // The contexts of a cache context are held in a hash set, iterated without allocating its boxed enumerator.
        if (contexts is HashSet<string> hashSet)
        {
            foreach (var item in hashSet)
            {
                if (string.Equals(item, context, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        if (contexts is string[] array)
        {
            foreach (var item in array)
            {
                if (string.Equals(item, context, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        foreach (var item in contexts)
        {
            if (string.Equals(item, context, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
