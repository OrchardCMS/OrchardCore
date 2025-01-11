const darkThemeName = "dark";
const lightThemeName = "light";
const themeStoreKeySuffix = "theme";

const getTenantName = () =>
    document.documentElement.getAttribute("data-tenant") || "default";

const getStoreKeySuffix = () => themeStoreKeySuffix || "theme";

const getStoreKey = () => `${getTenantName()}-${getStoreKeySuffix()}`;

const getStoredTheme = () => localStorage.getItem(getStoreKey());

const setStoredTheme = (theme: string | null) => {
    if (theme) {
        localStorage.setItem(getStoreKey(), theme);
    }
};

const isDarkMedia = () =>
    window.matchMedia("(prefers-color-scheme: dark)").matches;

const getPreferredTheme = () => {
    const storedTheme = getStoredTheme();
    if (storedTheme) {
        return storedTheme;
    }

    return isDarkMedia() ? darkThemeName : lightThemeName;
};

const setTheme = (theme: string | null) => {
    if (!theme) {
        return;
    }

    if (theme === "auto") {
        document.documentElement.setAttribute(
            "data-bs-theme",
            isDarkMedia() ? darkThemeName : lightThemeName
        );
    } else {
        document.documentElement.setAttribute("data-bs-theme", theme ?? "");
    }
};

export {
    darkThemeName,
    lightThemeName,
    themeStoreKeySuffix,
    getTenantName,
    getStoreKeySuffix,
    getStoreKey,
    getStoredTheme,
    setStoredTheme,
    isDarkMedia,
    getPreferredTheme,
    setTheme,
};
