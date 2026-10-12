const disabledElement = document.getElementById("Disabled") as HTMLInputElement | null;

disabledElement?.addEventListener("change", (e) => {
    const checked = (e.target as HTMLInputElement).checked;
    document.querySelectorAll<HTMLElement>(".autoroute-disabled").forEach((element) => {
        element.style.display = checked ? "none" : "";
    });
});

export {};
