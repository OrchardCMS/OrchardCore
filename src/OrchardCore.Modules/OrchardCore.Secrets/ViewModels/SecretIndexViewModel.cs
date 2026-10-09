using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Secrets.ViewModels;

public class SecretIndexViewModel
{
    public IList<SecretEntryViewModel> Secrets { get; set; } = [];

    public IList<SecretTypeViewModel> AvailableTypes { get; set; } = [];

    public SecretIndexOptions Options { get; set; } = new();

    [BindNever]
    public dynamic Pager { get; set; }

    [BindNever]
    public int ExpiredCount { get; set; }

    [BindNever]
    public int ExpiringCount { get; set; }
}

public class SecretIndexOptions
{
    public string Search { get; set; }

    public SecretsBulkAction BulkAction { get; set; }

    [BindNever]
    public List<SelectListItem> BulkActions { get; set; } = [];
}

public enum SecretsBulkAction
{
    None,
    Remove,
}

public class SecretEntryViewModel
{
    public string Name { get; set; }
    public string Store { get; set; }
    public string Type { get; set; }
    public string TypeDisplayName { get; set; }
    public string Description { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public DateTime? ExpiresUtc { get; set; }

    /// <summary>
    /// Gets the value of the bulk selection checkbox, which identifies the secret in its store.
    /// </summary>
    public string Id => SecretEntryId.Create(Store, Name);

    public bool IsExpired => ExpiresUtc.HasValue && ExpiresUtc.Value < DateTime.UtcNow;
    public bool IsExpiringSoon => ExpiresUtc.HasValue && !IsExpired && ExpiresUtc.Value < DateTime.UtcNow.AddDays(30);
}

/// <summary>
/// Identifies a secret in a store in a single form value, as store names never contain a colon.
/// </summary>
public static class SecretEntryId
{
    private const char Separator = ':';

    public static string Create(string store, string name)
        => store + Separator + name;

    public static bool TryParse(string id, out string store, out string name)
    {
        var index = id?.IndexOf(Separator) ?? -1;

        if (index <= 0 || index == id.Length - 1)
        {
            store = null;
            name = null;

            return false;
        }

        store = id[..index];
        name = id[(index + 1)..];

        return true;
    }
}
