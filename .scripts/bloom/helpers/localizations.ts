const translations: Record<string, string> = {};

/**
 * Returns the current translations record.
 */
export function getTranslations(): Record<string, string> {
  return translations;
}

/**
 * Merges the given translations into the store.
 */
export function setTranslations(t: Record<string, string>): void {
  Object.assign(translations, t);
}
