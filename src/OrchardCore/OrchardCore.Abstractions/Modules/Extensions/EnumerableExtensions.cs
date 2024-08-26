#nullable enable

namespace OrchardCore.Modules;

public static class EnumerableExtensions
{
    /// <summary>
    /// Clears the contents of <paramref name="target"/>. If <paramref name="source"/> is not <see langword="null"/>,
    /// its contents are copied over. If the two are the same, nothing happens.
    /// </summary>
    public static void SetItems<TKey, TValue>(this IDictionary<TKey, TValue?> target, IDictionary<TKey, TValue?>? source)
    {
        if (Equals(target, source))
        {
            return;
        }

        target.Clear();

        if (source != null)
        {
            foreach (var (key, template) in source)
            {
                target[key] = template;
            }
        }
    }
}
