// The scheme picker of the top bar (ToggleTheme.cshtml). A pick is applied to the page at once, as the
// 'data-ta-scheme' attribute of <html> that the schemes stylesheet styles, and kept in a cookie the layouts read, so
// the next page is rendered in it by the server rather than repainted after it loads. 'Site default' (an empty value)
// removes the cookie, so the user follows the scheme picked in Settings > Admin again.

(() => {
    const oneYearInSeconds = 60 * 60 * 24 * 365;

    const applyScheme = (list, choice) => {
        const root = document.documentElement;
        const scheme = choice || list.dataset.siteScheme;

        if (scheme) {
            root.setAttribute("data-ta-scheme", scheme);
        } else {
            root.removeAttribute("data-ta-scheme");
        }

        list.querySelectorAll("[data-ta-scheme-value]").forEach((option) => {
            const isChosen = option.dataset.taSchemeValue === choice;
            option.classList.toggle("active", isChosen);
            option.setAttribute("aria-pressed", isChosen ? "true" : "false");
        });
    };

    const storeScheme = (list, choice) => {
        const name = encodeURIComponent(list.dataset.cookieName);
        const secure = window.location.protocol === "https:" ? "; secure" : "";
        const value = choice ? `${encodeURIComponent(choice)}; max-age=${oneYearInSeconds}` : "; max-age=0";

        document.cookie = `${name}=${value}; path=/; samesite=lax${secure}`;
    };

    document.addEventListener("click", (event) => {
        const option = event.target.closest("[data-ta-scheme-value]");
        const list = option?.closest(".ta-scheme-list");

        if (!list) {
            return;
        }

        const choice = option.dataset.taSchemeValue;
        applyScheme(list, choice);
        storeScheme(list, choice);
    });
})();
