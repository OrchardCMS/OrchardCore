import { reactive } from "vue";
import { getTranslations } from "@bloom/helpers/localizations";

// Wrap the shared translations store in a Vue reactive proxy.
// Since reactive() proxies the same object, writes via Object.assign
// on the proxy trigger Vue's reactivity system.
const translations = reactive(getTranslations());

/**
 * Vue composable for managing translations with reactivity.
 * Delegates storage to the framework-agnostic localizations module.
 */
export function useLocalizations() {
  const setTranslations = (t: Record<string, string>) => {
    // Write through the reactive proxy so Vue detects the changes.
    Object.assign(translations, t);
  };

  return { translations, setTranslations };
}
