using System;

namespace OrchardCore.Roles.ViewModels;

public class PermissionGroupKey
{
    public readonly string Key;

    public readonly string Title;

    public string Source { get; set; }

    public PermissionGroupKey(string key)
    {
        if (String.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("The 'key' cannot be null or empty", nameof(key));
        }

        Key = key;
    }

    public PermissionGroupKey(string key, string title) : this(key)
    {
        Title = title;
    }

    public override int GetHashCode() => Key.GetHashCode();

    public override bool Equals(object obj)
    {
        var other = obj as PermissionGroupKey;

        return other != null && Key.Equals(other.Key);
    }
}
