const sources = document.getElementsByName("Source");
const stereotypeValue = (document.getElementById("SourceStereotype") as HTMLInputElement | null)?.value;
const contentTypesContainer = document.getElementById("ContentTypesContainer");
const stereotypeContainer = document.getElementById("StereotypesContainer");

sources.forEach((source) => {
    source.addEventListener("change", (event) => {
        const target = event.target as HTMLInputElement;

        if (!target.checked) {
            return;
        }

        if (target.value === stereotypeValue) {
            contentTypesContainer?.classList.add("d-none");
            stereotypeContainer?.classList.remove("d-none");
        } else {
            contentTypesContainer?.classList.remove("d-none");
            stereotypeContainer?.classList.add("d-none");
        }
    });
    source.dispatchEvent(new Event("change"));
});

export {};
