// Per-browser conveniences of a designer (panel width, collapsed panes). They are only preferences, so storage
// failures (private windows, blocked site data) are ignored.

/**
 * The preferences of a designer, stored under `prefix`, such as "orchardcore:workflows-designer:".
 */
export const createPreferences = (prefix: string) => ({
    readPreference(key: string): string | null {
        try {
            return window.localStorage.getItem(prefix + key);
        } catch {
            return null;
        }
    },

    writePreference(key: string, value: string) {
        try {
            window.localStorage.setItem(prefix + key, value);
        } catch {
            // Ignored, see above.
        }
    },
});
