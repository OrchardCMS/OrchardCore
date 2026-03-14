import { createVfm } from "vue-final-modal";
import { useLocalizations } from "../composables/useLocalizations";
import { mockTranslations } from "./mockdata";

/**
 * Returns global mount options (plugins, stubs) for Vue Test Utils.
 */
export function getGlobalMountOptions(stubs: Record<string, unknown> = {}) {
  return {
    plugins: [createVfm()],
    stubs: {
      "fa-icon": true,
      teleport: true,
      ...stubs,
    },
  };
}

/**
 * Set up mock translations before component tests.
 */
export function setupTranslations() {
  const { setTranslations } = useLocalizations();
  setTranslations(mockTranslations);
}
