// The editor of a scheme (Schemes/Edit.cshtml). Each color has a native picker and a hex field kept in step; the
// optional ones may be left empty. As the fields change, the editor posts them to the server, which answers with the
// CSS of the scheme being edited (the same derivation, contrast fixes, fonts and scales included, as a saved scheme),
// and applies it to the previews.

(() => {
    const form = document.querySelector("[data-ta-scheme-editor]");

    if (!form) {
        return;
    }

    const previewUrl = form.dataset.taPreviewUrl;
    const style = document.createElement("style");
    document.head.appendChild(style);

    const hexPattern = /^#[0-9a-f]{6}$/i;
    let timer = 0;
    let request = 0;

    const refresh = async () => {
        // Every field of the form, the anti-forgery token included; an invalid one makes the server refuse the
        // preview, which then keeps showing the last valid scheme.
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

    form.querySelectorAll("[data-ta-color-picker]").forEach((picker) => {
        const hex = form.querySelector(`[data-ta-color-hex="${picker.dataset.taColorPicker}"]`);

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
