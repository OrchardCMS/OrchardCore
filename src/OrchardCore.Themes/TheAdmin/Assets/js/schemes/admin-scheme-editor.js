// The editor of a scheme (Schemes/Edit.cshtml of the Themes module). Each color has a native picker and a hex field
// kept in step; the optional ones may be left empty. As the fields change, the editor posts them to the server, which
// answers with the CSS of the scheme being edited, written by the theme (the same derivation, contrast fixes, fonts
// and scales included, as a saved scheme), and applies it to the theme's preview.

(() => {
    const form = document.querySelector("[data-admin-scheme-editor]");

    if (!form) {
        return;
    }

    const previewUrl = form.dataset.previewUrl;
    const style = document.createElement("style");
    document.head.appendChild(style);

    const hexPattern = /^#[0-9a-f]{6}$/i;
    let timer = 0;
    let request = 0;

    // Whether every color field holds a '#rrggbb' color, or is empty for an optional one. A color being typed, e.g.
    // '#0f7', is not sent: the server would refuse it, and the browser would log each refused request as an error.
    const hasValidColors = () =>
        Array.from(form.querySelectorAll("[data-scheme-color-hex]")).every((hex) => {
            const value = hex.value.trim();

            return hexPattern.test(value) || (value === "" && !hex.required);
        });

    const refresh = async () => {
        // The preview keeps showing the last valid scheme until every color is valid again.
        if (!hasValidColors()) {
            return;
        }

        // Every field of the form, the anti-forgery token included.
        const current = ++request;

        try {
            const response = await fetch(previewUrl, {
                method: "POST",
                body: new FormData(form),
                credentials: "same-origin",
            });

            if (response.ok && current === request) {
                style.textContent = await response.text();
            }
        } catch {
            // The preview keeps the last valid scheme.
        }
    };

    const schedule = () => {
        window.clearTimeout(timer);
        timer = window.setTimeout(refresh, 200);
    };

    form.querySelectorAll("[data-scheme-color-picker]").forEach((picker) => {
        const hex = form.querySelector(`[data-scheme-color-hex="${picker.dataset.schemeColorPicker}"]`);

        picker.addEventListener("input", () => {
            hex.value = picker.value;
            schedule();
        });

        hex.addEventListener("input", () => {
            if (hexPattern.test(hex.value.trim())) {
                picker.value = hex.value.trim().toLowerCase();
            }
        });
    });

    form.addEventListener("input", schedule);
    form.addEventListener("change", schedule);

    refresh();
})();
