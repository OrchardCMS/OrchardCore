namespace OrchardCore.Entities;

public static class ICacheableEntityExtensions
{
    public static bool TryGetValue(this ICacheableEntity cache, string key, out object value)
    {
        value = cache.Get(key);

        return value != null;
    }

    public static bool TryGetValue<T>(this ICacheableEntity cache, string key, out T value)
    {
        var obj = cache.Get(key);

        if (obj is T castedValue)
        {
            value = castedValue;

            return true;
        }

        value = default;

        return false;
    }
}
