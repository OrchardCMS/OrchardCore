const overrideCheckbox = document.getElementById("OverrideSitemapConfig") as HTMLInputElement | null;

overrideCheckbox?.addEventListener("change", (e) => {
    const checked = (e.target as HTMLInputElement).checked;
    document.querySelectorAll<HTMLElement>(".sitemap-form-g").forEach((element) => {
        element.style.display = checked ? "" : "none";
    });
});

export {};
