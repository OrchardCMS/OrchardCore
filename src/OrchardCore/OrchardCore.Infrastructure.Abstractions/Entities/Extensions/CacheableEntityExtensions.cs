namespace OrchardCore.Entities;

public static class CacheableEntityExtensions
{
    public static bool TryGetValue(this ICacheableEntity cache, string key, out object value)
    {
        value = cache.Get(key);

        return value != null;
    }

    public static bool TryGetValue<T>(this ICacheableEntity cache, string key, out T value)
    {
        if (cache.TryGetValue(key, out object result))
        {
            if (result is T castedValue)
            {
                value = castedValue;

                return true;
            }
        }

        value = default;

        return false;
    }
}
