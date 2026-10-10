// The UI strings are rendered by the server into config.translations (a key/value dictionary). Every call names its
// English text, used when the dictionary has no value for the key, so the designer works without translations.
let translations: Record<string, string> = {};

export const loadTranslations = (values: Record<string, string> | null | undefined) => {
    translations = values ?? {};
};

const format = (template: string, args: (string | number)[]) =>
    args.length === 0 ? template : template.replace(/\{(\d+)\}/g, (match, index) => (index < args.length ? String(args[index]) : match));

/**
 * Returns the localized string for `key`, or the English `fallback`, replacing {0}, {1}... with `args`.
 */
export const t = (key: string, fallback: string, ...args: (string | number)[]) => format(translations[key] || fallback, args);
