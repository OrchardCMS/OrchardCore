// The strings of the designers. Each one is named by a key, and has an English text used when no translation is known
// for the key. A designer loads its translations from the server (an IJSLocalizer group), and may add its own English
// wording for the shared keys, such as "The workflow was changed elsewhere" for "ConflictTitle".
let translations: Record<string, string> = {};
let defaults: Record<string, string> = {};

/**
 * Replaces the translations, usually the dictionary the server rendered into the designer's configuration.
 */
export const loadTranslations = (values: Record<string, string> | null | undefined) => {
    translations = { ...(values ?? {}) };
};

/**
 * Sets the designer's own English wording of keys, used when a key has no translation. It wins over the English text
 * of the shared components.
 */
export const setDefaultTexts = (values: Record<string, string> | null | undefined) => {
    defaults = { ...(values ?? {}) };
};

export const format = (template: string, args: (string | number)[]) =>
    args.length === 0 ? template : template.replace(/\{(\d+)\}/g, (match, index) => (index < args.length ? String(args[index]) : match));

/**
 * Returns the text of `key`, replacing {0}, {1}... with `args`: its translation, else the designer's wording, else
 * `fallback`.
 */
export const t = (key: string, fallback: string, ...args: (string | number)[]) =>
    format(translations[key] || defaults[key] || fallback, args);

/**
 * Whether a key has a translation or a designer wording.
 */
export const hasText = (key: string) => !!(translations[key] || defaults[key]);
