using System;

namespace OrchardCore.Roles.ViewModels;

public class PermissionGroupKey
{
    private readonly string _key;

    public string Title { get; set; }

    public string Source { get; set; }

    public PermissionGroupKey(string key)
    {
        if (String.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(nameof(key));
        }

        _key = key;
    }

    public PermissionGroupKey(string key, string title)
        : this(key)
    {
        Title = title;
    }

    public override int GetHashCode()
    {
        return _key.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        var other = obj as PermissionGroupKey;

        return _key.Equals(other?._key);
    }
}
