const getDatasetBoolean = (element: HTMLElement, key: string) => element.dataset[key] === "true";

const getDatasetNumber = (element: HTMLElement, key: string) => Number(element.dataset[key]);

// Swallows a malformed value to `undefined` rather than throwing - most callers compute the
// attribute server-side (always valid JSON), but some (e.g. a user-authored Monaco "options"
// blob) reflect free-form admin input that isn't guaranteed to parse.
const getDatasetJson = <T>(element: HTMLElement, key: string): T | undefined => {
    const raw = element.dataset[key];

    if (!raw) {
        return undefined;
    }

    try {
        return JSON.parse(raw) as T;
    } catch {
        return undefined;
    }
};

export { getDatasetBoolean, getDatasetNumber, getDatasetJson };
