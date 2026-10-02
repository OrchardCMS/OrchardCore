// Per-browser conveniences of the designer (panel width, collapsed panes). They are only preferences, so
// storage failures (private windows, blocked site data) are ignored.
const PREFIX = "orchardcore:workflows-designer:";

export const readPreference = (key: string): string | null => {
    try {
        return window.localStorage.getItem(PREFIX + key);
    } catch {
        return null;
    }
};

export const writePreference = (key: string, value: string) => {
    try {
        window.localStorage.setItem(PREFIX + key, value);
    } catch {
        // Ignored, see above.
    }
};
