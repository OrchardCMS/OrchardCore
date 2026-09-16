namespace OrchardCore.Seo.Services;

internal static class RobotsSettingsEditor
{
    public static bool Apply(RobotsSettings settings, bool allowAllAgents, bool disallowAdmin, string additionalRules)
    {
        var changed = settings.AllowAllAgents != allowAllAgents || settings.DisallowAdmin != disallowAdmin
            || settings.AdditionalRules != additionalRules;
        settings.AllowAllAgents = allowAllAgents;
        settings.DisallowAdmin = disallowAdmin;
        settings.AdditionalRules = additionalRules;
        return changed;
    }
}
