import { reactive } from "vue";

const translations = reactive<Record<string, string>>({});

/**
 * Composable for managing translations passed from the server-side Razor view.
 */
export function useLocalizations() {
  const setTranslations = (t: Record<string, string>) => {
    Object.assign(translations, t);
  };

  return { translations, setTranslations };
}
