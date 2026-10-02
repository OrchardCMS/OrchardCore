import { getTranslations, setTranslations } from "@bloom/helpers/localizations";

// Client strings come from the 'workflows-designer' IJSLocalizer group (WorkflowsDesignerJSLocalizer.cs).
// The key is shown when a translation is missing, so every key used here must exist on the server.
export const loadTranslations = (translations: Record<string, string>) => setTranslations(translations ?? {});

/**
 * Returns the localized string for `key`, replacing {0}, {1}... with `args`.
 */
export const t = (key: string, ...args: (string | number)[]) => {
    const template = getTranslations()[key] ?? key;

    return args.length === 0 ? template : template.replace(/\{(\d+)\}/g, (match, index) => (index < args.length ? String(args[index]) : match));
};
