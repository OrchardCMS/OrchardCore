import { reactive } from "vue";

const translations = reactive<Record<string, string>>({});

/**
 * Composable for managing translations passed from the server-side Razor view.
 *
 * @example
 * // In your root component or app entry point, seed the translations once:
 * const { setTranslations } = useLocalizations();
 * setTranslations(window.__localizations ?? {});
 *
 * // In any child component:
 * const { translations: t } = useLocalizations();
 * console.log(t["Save"]); // → localized value
 */
export function useLocalizations() {
    const setTranslations = (t: Record<string, string>) => {
        Object.assign(translations, t);
    };

    return { translations, setTranslations };
}
