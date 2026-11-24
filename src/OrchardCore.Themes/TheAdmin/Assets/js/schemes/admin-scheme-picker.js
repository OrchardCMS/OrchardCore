// The scheme picker of TheAdmin's user menu (ToggleTheme.cshtml). The list of the picker
// carries 'data-admin-scheme-picker', the attribute of <html> the theme's schemes stylesheet selects on
// ('data-scheme-attribute'), the name of the cookie the layouts read ('data-cookie-name') and the site's default
// scheme ('data-site-scheme'); each option carries its scheme in 'data-admin-scheme-value'. A pick is applied to the
// page at once and kept in the cookie, so the next page is rendered in it by the server rather than repainted after it
// loads. The site's default scheme carries an empty value: picking it removes the cookie, so the user follows the
// site's default scheme again.

(() => {
    const oneYearInSeconds = 60 * 60 * 24 * 365;

    const applyScheme = (list, choice) => {
        const root = document.documentElement;
        const attribute = list.dataset.schemeAttribute;
        const scheme = choice || list.dataset.siteScheme;

        if (attribute) {
            if (scheme) {
                root.setAttribute(attribute, scheme);
            } else {
                root.removeAttribute(attribute);
            }
        }

        list.querySelectorAll("[data-admin-scheme-value]").forEach((option) => {
            const isChosen = option.dataset.adminSchemeValue === choice;
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
        const option = event.target.closest("[data-admin-scheme-value]");
        const list = option?.closest("[data-admin-scheme-picker]");

        if (!list) {
            return;
        }

        const choice = option.dataset.adminSchemeValue;
        applyScheme(list, choice);
        storeScheme(list, choice);
    });
})();
