import { ref } from "vue";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const translations = ref<any>({});

/**
 * Composable for managing translations passed from the server-side Razor view.
 */
export function useLocalizations() {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const setTranslations = (t: any) => {
    translations.value = t;
  };

  return { translations, setTranslations };
}
