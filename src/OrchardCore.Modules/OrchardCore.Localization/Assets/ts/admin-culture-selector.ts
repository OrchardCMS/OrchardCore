const picker = document.getElementById("oc-culture-picker");

if (picker) {
    const cookieName = picker.dataset.cookieName ?? "";
    const cookiePath = picker.dataset.cookiePath ?? "";

    document.querySelectorAll<HTMLElement>(".language-menu-item").forEach((item) => {
        const culture = item.dataset.languageValue ?? "";

        item.addEventListener("click", () => {
            Cookies.set(cookieName, `c=${culture}|uic=${culture}`, { path: cookiePath });
            window.location.reload();
        });
    });
}
