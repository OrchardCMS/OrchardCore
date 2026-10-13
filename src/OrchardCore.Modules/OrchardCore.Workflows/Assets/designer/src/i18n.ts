import { getTranslations, setTranslations } from "@bloom/helpers/localizations";
import { format, loadTranslations as loadSharedTranslations } from "@bloom/designer/core/i18n";

// Client strings come from the 'workflows-designer' IJSLocalizer group (WorkflowsDesignerJSLocalizer.cs).
// The key is shown when a translation is missing, so every key used here must exist on the server. The shared
// components (@bloom/designer) read the same translations, and fall back to their English text.
export const loadTranslations = (translations: Record<string, string>) => {
    setTranslations(translations ?? {});
    loadSharedTranslations(translations);
};

/**
 * Returns the localized string for `key`, replacing {0}, {1}... with `args`.
 */
export const t = (key: string, ...args: (string | number)[]) => format(getTranslations()[key] ?? key, args);
