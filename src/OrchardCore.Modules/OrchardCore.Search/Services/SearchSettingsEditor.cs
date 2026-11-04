using OrchardCore.Indexing;
using OrchardCore.Search.Models;

namespace OrchardCore.Search.Services;

internal static class SearchSettingsEditor
{
    internal static async Task<bool> ValidateAsync(IIndexProfileStore profiles, SearchSettings current, SearchSettings proposed)
    {
        if (proposed.DefaultIndexProfileName == current.DefaultIndexProfileName) { return true; }
        if (string.IsNullOrWhiteSpace(proposed.DefaultIndexProfileName)) { proposed.DefaultIndexProfileName = null; }
        if (proposed.DefaultIndexProfileName is null || proposed.DefaultIndexProfileName == current.DefaultIndexProfileName)
        {
            return true;
        }
        var index = await profiles.FindByNameAsync(proposed.DefaultIndexProfileName);
        if (index is null) { return false; }
        proposed.DefaultIndexProfileName = index.Name;
        return true;
    }

    internal static bool Apply(SearchSettings current, SearchSettings proposed)
    {
        if (current.DefaultIndexProfileName == proposed.DefaultIndexProfileName
            && current.Placeholder == proposed.Placeholder && current.PageTitle == proposed.PageTitle)
        {
            return false;
        }
        current.DefaultIndexProfileName = proposed.DefaultIndexProfileName;
        current.Placeholder = proposed.Placeholder;
        current.PageTitle = proposed.PageTitle;
        return true;
    }
}
