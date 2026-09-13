using OrchardCore.ContentLocalization.Models;

namespace OrchardCore.ContentLocalization.Services;

internal static class ContentCultureSettingsEditor
{
    internal static bool Apply(ContentCulturePickerSettings current, ContentCulturePickerSettings proposed)
    {
        if (current.SetCookie == proposed.SetCookie && current.RedirectToHomepage == proposed.RedirectToHomepage)
        {
            return false;
        }
        current.SetCookie = proposed.SetCookie;
        current.RedirectToHomepage = proposed.RedirectToHomepage;
        return true;
    }

    internal static bool Apply(ContentRequestCultureProviderSettings current, ContentRequestCultureProviderSettings proposed)
    {
        if (current.SetCookie == proposed.SetCookie)
        {
            return false;
        }
        current.SetCookie = proposed.SetCookie;
        return true;
    }
}
